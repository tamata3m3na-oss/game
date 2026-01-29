import { Injectable, NestMiddleware, Logger } from '@nestjs/common';
import { Request, Response, NextFunction } from 'express';

@Injectable()
export class LoggerMiddleware implements NestMiddleware {
  private readonly logger = new Logger('HTTP');

  use(req: Request, res: Response, next: NextFunction) {
    const { method, originalUrl, body, query } = req;
    const start = Date.now();

    this.logger.log(`[REQUEST] ${method} ${originalUrl}`);
    
    if (Object.keys(body).length > 0 && method !== 'GET') {
      const sanitizedBody = { ...body };
      if (sanitizedBody.password) {
        sanitizedBody.password = '***';
      }
      this.logger.debug(`[BODY] ${JSON.stringify(sanitizedBody)}`);
    }
    
    if (Object.keys(query).length > 0) {
      this.logger.debug(`[QUERY] ${JSON.stringify(query)}`);
    }

    res.on('finish', () => {
      const duration = Date.now() - start;
      const { statusCode } = res;
      
      if (statusCode >= 500) {
        this.logger.error(`[RESPONSE] ${method} ${originalUrl} - ${statusCode} - ${duration}ms`);
      } else if (statusCode >= 400) {
        this.logger.warn(`[RESPONSE] ${method} ${originalUrl} - ${statusCode} - ${duration}ms`);
      } else {
        this.logger.log(`[RESPONSE] ${method} ${originalUrl} - ${statusCode} - ${duration}ms`);
      }
    });

    next();
  }
}
