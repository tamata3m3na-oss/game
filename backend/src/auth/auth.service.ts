import { Injectable, ConflictException, UnauthorizedException, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { JwtService } from '@nestjs/jwt';
import { ConfigService } from '@nestjs/config';
import * as bcrypt from 'bcrypt';
import { User } from '../database/entities/user.entity';
import { RegisterDto } from './dto/register.dto';
import { LoginDto } from './dto/login.dto';

@Injectable()
export class AuthService {
  private readonly logger = new Logger(AuthService.name);

  constructor(
    @InjectRepository(User)
    private userRepository: Repository<User>,
    private jwtService: JwtService,
    private configService: ConfigService,
  ) {
    this.logger.log('AuthService initialized');
  }

  async register(registerDto: RegisterDto) {
    const { email, username, password } = registerDto;

    this.logger.debug(`[REGISTER] Checking for existing user: ${email}`);
    const existingUser = await this.userRepository.findOne({
      where: [{ email }, { username }],
    });

    if (existingUser) {
      if (existingUser.email === email) {
        this.logger.warn(`[REGISTER] Email already exists: ${email}`);
        throw new ConflictException('Email already exists');
      }
      if (existingUser.username === username) {
        this.logger.warn(`[REGISTER] Username already exists: ${username}`);
        throw new ConflictException('Username already exists');
      }
    }

    this.logger.debug(`[REGISTER] Hashing password for: ${email}`);
    const hashedPassword = await bcrypt.hash(password, 10);

    const user = this.userRepository.create({
      email,
      username,
      passwordHash: hashedPassword,
    });

    this.logger.debug(`[REGISTER] Saving user to database: ${email}`);
    await this.userRepository.save(user);

    const tokens = await this.generateTokens(user.id);

    this.logger.log(`[REGISTER] User created successfully: ${username} (ID: ${user.id})`);

    return {
      user: {
        id: user.id,
        email: user.email,
        username: user.username,
        rating: user.rating,
        wins: user.wins,
        losses: user.losses,
      },
      ...tokens,
    };
  }

  async login(loginDto: LoginDto) {
    const { email, password } = loginDto;

    this.logger.debug(`[LOGIN] Finding user by email: ${email}`);
    const user = await this.userRepository.findOne({ where: { email } });

    if (!user) {
      this.logger.warn(`[LOGIN] User not found: ${email}`);
      throw new UnauthorizedException('Invalid credentials');
    }

    this.logger.debug(`[LOGIN] Comparing password for user: ${email}`);
    const isPasswordValid = await bcrypt.compare(password, user.passwordHash);

    if (!isPasswordValid) {
      this.logger.warn(`[LOGIN] Invalid password for user: ${email}`);
      throw new UnauthorizedException('Invalid credentials');
    }

    const tokens = await this.generateTokens(user.id);

    this.logger.log(`[LOGIN] User logged in successfully: ${user.username} (ID: ${user.id})`);

    return {
      user: {
        id: user.id,
        email: user.email,
        username: user.username,
        rating: user.rating,
        wins: user.wins,
        losses: user.losses,
      },
      ...tokens,
    };
  }

  async refresh(refreshToken: string) {
    try {
      this.logger.debug('[REFRESH] Verifying refresh token');
      const payload = this.jwtService.verify(refreshToken, {
        secret: this.configService.get('JWT_REFRESH_SECRET'),
      });

      this.logger.debug(`[REFRESH] Generating new tokens for user: ${payload.sub}`);
      const tokens = await this.generateTokens(payload.sub);

      this.logger.log(`[REFRESH] Tokens refreshed successfully for user: ${payload.sub}`);
      return tokens;
    } catch (error) {
      this.logger.error(`[REFRESH] Token verification failed: ${error.message}`);
      throw new UnauthorizedException('Invalid refresh token');
    }
  }

  private async generateTokens(userId: number) {
    this.logger.debug(`[TOKENS] Generating tokens for user: ${userId}`);
    const payload = { sub: userId };

    const accessToken = this.jwtService.sign(payload, {
      secret: this.configService.get('JWT_SECRET'),
      expiresIn: this.configService.get('JWT_EXPIRES_IN'),
    });

    const refreshToken = this.jwtService.sign(payload, {
      secret: this.configService.get('JWT_REFRESH_SECRET'),
      expiresIn: this.configService.get('JWT_REFRESH_EXPIRES_IN'),
    });

    return {
      accessToken,
      refreshToken,
    };
  }
}
