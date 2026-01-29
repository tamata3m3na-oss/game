import { Controller, Post, Body, HttpCode, HttpStatus, Logger } from '@nestjs/common';
import { AuthService } from './auth.service';
import { RegisterDto } from './dto/register.dto';
import { LoginDto } from './dto/login.dto';
import { RefreshTokenDto } from './dto/refresh-token.dto';

@Controller('auth')
export class AuthController {
  private readonly logger = new Logger(AuthController.name);

  constructor(private readonly authService: AuthService) {}

  @Post('register')
  async register(@Body() registerDto: RegisterDto) {
    this.logger.log(`[REGISTER] Attempting registration for email: ${registerDto.email}`);
    try {
      const result = await this.authService.register(registerDto);
      this.logger.log(`[REGISTER] Success: User ${result.user.username} (ID: ${result.user.id}) registered`);
      return result;
    } catch (error) {
      this.logger.error(`[REGISTER] Failed for ${registerDto.email}: ${error.message}`);
      throw error;
    }
  }

  @Post('login')
  @HttpCode(HttpStatus.OK)
  async login(@Body() loginDto: LoginDto) {
    this.logger.log(`[LOGIN] Login attempt for email: ${loginDto.email}`);
    try {
      const result = await this.authService.login(loginDto);
      this.logger.log(`[LOGIN] Success: User ${result.user.username} (ID: ${result.user.id}) logged in`);
      return result;
    } catch (error) {
      this.logger.warn(`[LOGIN] Failed for ${loginDto.email}: ${error.message}`);
      throw error;
    }
  }

  @Post('refresh')
  @HttpCode(HttpStatus.OK)
  async refresh(@Body() refreshTokenDto: RefreshTokenDto) {
    this.logger.log(`[REFRESH] Token refresh attempt`);
    try {
      const result = await this.authService.refresh(refreshTokenDto.refreshToken);
      this.logger.log(`[REFRESH] Success: Token refreshed`);
      return result;
    } catch (error) {
      this.logger.warn(`[REFRESH] Failed: ${error.message}`);
      throw error;
    }
  }
}
