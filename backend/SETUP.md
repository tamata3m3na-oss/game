# Backend Setup Guide

## Development Setup

### 1. Install Dependencies
```bash
cd backend
npm install
```

### 2. Configure Environment
Copy `.env` file and update the values:
```bash
cp .env .env.local  # Optional: for local overrides
```

Important environment variables:
- `JWT_SECRET` - Secret key for JWT tokens (CHANGE IN PRODUCTION!)
- `JWT_REFRESH_SECRET` - Secret key for refresh tokens (CHANGE IN PRODUCTION!)
- `DB_PASSWORD` - PostgreSQL password
- `CORS_ORIGIN` - Frontend URL

### 3. Start Database Services
```bash
docker compose up -d
```

This will start:
- PostgreSQL on port 5432
- Redis on port 6379

Verify services are running:
```bash
docker compose ps
```

### 4. Start Development Server
```bash
npm run start:dev
```

The server will start on http://localhost:3000 with hot reload enabled.

### 5. Verify Setup
Check the logs for successful database connection:
```bash
tail -f server.log
```

You should see:
- TypeORM connection established
- Tables created (users, matches)
- Routes registered
- "Server is running on http://localhost:3000"

## Testing the API

### Using the Test Script
```bash
./test-api.sh
```

### Using Postman
Import the Postman collection:
```
File -> Import -> postman_collection.json
```

### Manual Testing with curl

1. Register a user:
```bash
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "password123"
  }'
```

2. Login:
```bash
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'
```

Save the `accessToken` from the response.

3. Get your profile:
```bash
curl -X GET http://localhost:3000/player/me \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## Production Setup

### 1. Environment Configuration
Update `.env` for production:
```bash
NODE_ENV=production
JWT_SECRET=<generate-strong-random-secret>
JWT_REFRESH_SECRET=<generate-strong-random-secret>
DB_PASSWORD=<strong-database-password>
CORS_ORIGIN=https://your-production-domain.com
```

Generate secrets:
```bash
node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
```

### 2. Database Setup
For production, use TypeORM migrations instead of synchronize:

Update `database.module.ts`:
```typescript
synchronize: false, // Disable auto-sync in production
migrations: ['dist/database/migrations/*.js'],
migrationsRun: true,
```

### 3. Build and Deploy
```bash
npm run build
npm run start:prod
```

### 4. Database Migrations
Generate migration:
```bash
npm run typeorm migration:generate -- -n MigrationName
```

Run migrations:
```bash
npm run typeorm migration:run
```

## Troubleshooting

### Database Connection Issues
1. Check if PostgreSQL is running:
```bash
docker compose ps
```

2. Test connection:
```bash
docker exec chess_postgres psql -U postgres -d chess_game -c "SELECT 1;"
```

3. Check database logs:
```bash
docker compose logs postgres
```

### Server Won't Start
1. Check for port conflicts:
```bash
lsof -i :3000
```

2. Verify all dependencies are installed:
```bash
npm install
```

3. Check TypeScript compilation:
```bash
npm run build
```

### JWT Token Issues
1. Verify JWT secrets are set in `.env`
2. Check token expiration times
3. Ensure Authorization header format: `Bearer <token>`

### Validation Errors
The API uses class-validator for input validation. Common errors:
- Email must be a valid email address
- Username must be at least 3 characters
- Password must be at least 6 characters

## Database Management

### Access PostgreSQL
```bash
docker exec -it chess_postgres psql -U postgres -d chess_game
```

### Common SQL Commands
```sql
-- List all tables
\dt

-- Describe table structure
\d users
\d matches

-- View users
SELECT id, email, username, rating, wins, losses FROM users;

-- View matches
SELECT * FROM matches;

-- Delete all data (careful!)
TRUNCATE users CASCADE;
TRUNCATE matches CASCADE;
```

### Backup Database
```bash
docker exec chess_postgres pg_dump -U postgres chess_game > backup.sql
```

### Restore Database
```bash
docker exec -i chess_postgres psql -U postgres chess_game < backup.sql
```

## Performance Optimization

### Database Indexing
The following indexes are automatically created:
- Users: email (unique), username (unique)
- Matches: player1_id, player2_id, winner_id (foreign keys)

### Connection Pooling
TypeORM uses connection pooling by default. Adjust in `database.module.ts`:
```typescript
extra: {
  max: 20, // Maximum pool size
  connectionTimeoutMillis: 2000,
}
```

### Redis Caching
Redis is configured but not yet implemented. Future use cases:
- Session storage
- Rate limiting
- Game state caching
- Leaderboard caching

## Security Considerations

### Production Checklist
- [ ] Change default JWT secrets
- [ ] Use strong database passwords
- [ ] Enable HTTPS
- [ ] Configure proper CORS origins
- [ ] Disable TypeORM synchronize
- [ ] Use environment variables for secrets
- [ ] Enable rate limiting
- [ ] Implement request logging
- [ ] Set up monitoring and alerts
- [ ] Regular security updates

### Password Security
- Passwords are hashed with bcrypt (10 rounds)
- Never log passwords
- Passwords are validated for minimum length (6 characters)

### JWT Security
- Access tokens expire after 1 hour
- Refresh tokens expire after 7 days
- Tokens are signed with HS256
- Use different secrets for access and refresh tokens

## Monitoring

### Check Server Health
```bash
curl http://localhost:3000/player/1
```

### View Server Logs
```bash
tail -f server.log
```

### Monitor Database Connections
```sql
SELECT * FROM pg_stat_activity WHERE datname = 'chess_game';
```

## Next Steps

After basic setup is complete:
1. Implement WebSocket for real-time gameplay
2. Add match-making system
3. Implement chess game logic
4. Add rating calculation (ELO)
5. Create leaderboards
6. Add game history
7. Implement move validation
8. Add spectator mode
