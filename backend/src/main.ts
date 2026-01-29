import { NestFactory } from '@nestjs/core';
import { ValidationPipe, Logger } from '@nestjs/common';
import { AppModule } from './app.module';
import { LoggerMiddleware } from './logger.middleware';

async function bootstrap() {
  const logger = new Logger('Bootstrap');
  
  const app = await NestFactory.create(AppModule, {
    logger: ['error', 'warn', 'log', 'debug', 'verbose'],
  });

  // Enable CORS
  app.enableCors({
    origin: process.env.CORS_ORIGIN || 'http://localhost:3000',
    credentials: true,
  });

  // Global validation pipe
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true,
      forbidNonWhitelisted: true,
      transform: true,
    }),
  );

  // Logging middleware
  app.use(new LoggerMiddleware().use);

  const port = process.env.PORT || 3000;
  await app.listen(port);

  logger.log(`========================================`);
  logger.log(`🚀 Server is running on http://localhost:${port}`);
  logger.log(`📡 WebSocket endpoint: ws://localhost:${port}/pvp`);
  logger.log(`🔧 Environment: ${process.env.NODE_ENV || 'development'}`);
  logger.log(`========================================`);
}

bootstrap().catch((err) => {
  const logger = new Logger('Bootstrap');
  logger.error(`❌ Failed to start server: ${err.message}`, err.stack);
  process.exit(1);
});
