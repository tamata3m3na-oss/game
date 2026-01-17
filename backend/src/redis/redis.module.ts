import { Global, Module } from '@nestjs/common';
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
        const host = configService.get<string>('REDIS_HOST') || 'localhost';
        const port = configService.get<string>('REDIS_PORT') || '6379';

        const client = createClient({
          url: `redis://${host}:${port}`,
        });

        client.on('error', () => {
          // Swallow to prevent unhandled errors from crashing the process;
          // connection issues will be surfaced via failed commands.
        });

        if (!client.isOpen) {
          await client.connect();
        }

        return client;
      },
    },
  ],
  exports: [REDIS_CLIENT],
})
export class RedisModule {}
