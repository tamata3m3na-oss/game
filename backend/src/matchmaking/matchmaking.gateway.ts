import {
  WebSocketGateway,
  WebSocketServer,
  SubscribeMessage,
  OnGatewayConnection,
  OnGatewayDisconnect,
  ConnectedSocket,
  MessageBody,
  OnGatewayInit,
} from '@nestjs/websockets';
import { Server, Socket } from 'socket.io';
import { Logger, UseGuards } from '@nestjs/common';
import { MatchmakingService } from './matchmaking.service';
import { WsJwtGuard } from './guards/ws-jwt.guard';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from '../database/entities/user.entity';
import { PvpSessionService } from './pvp-session.service';
import * as jwt from 'jsonwebtoken';
import { JwtPayload } from 'jsonwebtoken';
import { ConfigService } from '@nestjs/config';

@WebSocketGateway({
  namespace: '/pvp',
  cors: {
    origin: process.env.CORS_ORIGIN || 'http://localhost:3000',
    credentials: true,
  },
  pingTimeout: 30000,
  pingInterval: 25000,
})
export class MatchmakingGateway
  implements OnGatewayInit, OnGatewayConnection, OnGatewayDisconnect
{
  @WebSocketServer()
  server: Server;

  private readonly logger = new Logger(MatchmakingGateway.name);

  constructor(
    private readonly matchmakingService: MatchmakingService,
    @InjectRepository(User)
    private readonly userRepository: Repository<User>,
    private readonly sessions: PvpSessionService,
    private readonly configService: ConfigService,
  ) {}

  afterInit(server: Server) {
    this.sessions.setServer(server);
    this.logger.log('WebSocket Gateway initialized on namespace /pvp');
  }

  async handleConnection(client: Socket) {
    try {
      const token =
        client.handshake.auth?.token ||
        client.handshake.headers?.authorization?.split(' ')[1];

      if (!token) {
        this.logger.warn(`Client ${client.id} disconnected: No token provided`);
        client.disconnect();
        return;
      }

      const user = await this.validateToken(token);
      if (!user) {
        this.logger.warn(`Client ${client.id} disconnected: Invalid token`);
        client.disconnect();
        return;
      }

      client.data.userId = user.id;
      client.data.user = user;
      this.sessions.register(user.id, client.id);

      this.logger.log(
        `Client connected: ${client.id} (User: ${user.id} - ${user.username})`,
      );
    } catch (error) {
      this.logger.error(`Error handling connection: ${(error as Error).message}`);
      client.disconnect();
    }
  }

  async handleDisconnect(client: Socket) {
    const userId = client.data.userId;

    if (userId) {
      try {
        this.sessions.unregister(userId);
        await this.matchmakingService.handleDisconnect(userId);
        this.logger.log(`Client disconnected: ${client.id} (User: ${userId})`);
      } catch (error) {
        this.logger.error(
          `Error handling disconnect for user ${userId}: ${(error as Error).message}`,
        );
      }
    }
  }

  @UseGuards(WsJwtGuard)
  @SubscribeMessage('queue:join')
  async handleJoinQueue(@ConnectedSocket() client: Socket) {
    const userId = client.data.userId;

    try {
      const status = await this.matchmakingService.joinQueue(userId);
      client.emit('queue:status', status);
      this.logger.log(`User ${userId} joined matchmaking queue`);
    } catch (error) {
      this.logger.error(
        `Error joining queue for user ${userId}: ${(error as Error).message}`,
      );
      client.emit('queue:status', { position: 0, estimatedWait: 0 });
    }
  }

  @UseGuards(WsJwtGuard)
  @SubscribeMessage('queue:leave')
  async handleLeaveQueue(@ConnectedSocket() client: Socket) {
    const userId = client.data.userId;

    try {
      await this.matchmakingService.leaveQueue(userId);
      client.emit('queue:status', { position: 0, estimatedWait: 0 });
      this.logger.log(`User ${userId} left matchmaking queue`);
    } catch (error) {
      this.logger.error(
        `Error leaving queue for user ${userId}: ${(error as Error).message}`,
      );
      client.emit('queue:status', { position: 0, estimatedWait: 0 });
    }
  }

  @UseGuards(WsJwtGuard)
  @SubscribeMessage('match:ready')
  async handleMatchReady(
    @ConnectedSocket() client: Socket,
    @MessageBody() data: { matchId: number },
  ) {
    const userId = client.data.userId;
    const { matchId } = data;

    try {
      await this.matchmakingService.markPlayerReady(userId, matchId);
      this.logger.log(`User ${userId} marked ready for match ${matchId}`);
    } catch (error) {
      this.logger.error(
        `Error handling match ready for user ${userId}: ${(error as Error).message}`,
      );
    }
  }

  private async validateToken(token: string): Promise<User | null> {
    try {
      const secret = this.configService.get<string>('JWT_SECRET');
      if (!secret) {
        return null;
      }

      const decoded = jwt.verify(token, secret);
      
      if (typeof decoded === 'string') {
        return null;
      }

      const payload = decoded as JwtPayload;
      const sub =
        typeof payload.sub === 'string'
          ? parseInt(payload.sub, 10)
          : typeof payload.sub === 'number'
            ? payload.sub
            : null;

      if (!sub) {
        return null;
      }

      const user = await this.userRepository.findOne({
        where: { id: sub },
      });

      return user;
    } catch {
      return null;
    }
  }
}
