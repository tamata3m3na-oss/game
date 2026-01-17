import { Injectable } from '@nestjs/common';
import { Server } from 'socket.io';

@Injectable()
export class PvpSessionService {
  private server: Server | null = null;
  private readonly playerToSocketId = new Map<number, string>();

  setServer(server: Server) {
    this.server = server;
  }

  register(playerId: number, socketId: string) {
    this.playerToSocketId.set(playerId, socketId);
  }

  unregister(playerId: number) {
    this.playerToSocketId.delete(playerId);
  }

  getSocketId(playerId: number): string | undefined {
    return this.playerToSocketId.get(playerId);
  }

  emitToPlayer(playerId: number, event: string, payload: unknown) {
    const socketId = this.playerToSocketId.get(playerId);
    if (!this.server || !socketId) {
      return;
    }

    this.server.to(socketId).emit(event, payload);
  }
}
