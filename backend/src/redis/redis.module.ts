import { Global, Module, Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { createClient } from 'redis';

export const REDIS_CLIENT = Symbol('REDIS_CLIENT');

@Global()
@Module({
  providers: [
    {
      provide: REDIS_CLIENT,
      inject: [ConfigService],
      useFactory: async (configService: ConfigService) => {
        const logger = new Logger('RedisModule');
        const host = configService.get<string>('REDIS_HOST') || 'localhost';
        const port = configService.get<string>('REDIS_PORT') || '6379';
        const url = `redis://${host}:${port}`;

        logger.log(`[REDIS] Connecting to Redis at ${host}:${port}...`);

        const client = createClient({
          url,
        });

        client.on('error', (err) => {
          logger.error(`[REDIS] Connection error: ${err.message}`);
        });

        client.on('connect', () => {
          logger.log(`[REDIS] Connected successfully`);
        });

        client.on('reconnecting', () => {
          logger.warn(`[REDIS] Reconnecting...`);
        });

        try {
          if (!client.isOpen) {
            await client.connect();
            logger.log(`[REDIS] Client ready and connected`);
          }
        } catch (error) {
          logger.error(`[REDIS] Failed to connect: ${error.message}`);
          throw error;
        }

        return client;
      },
    },
  ],
  exports: [REDIS_CLIENT],
})
export class RedisModule {}
