# API Testing Guide

## Quick Start

### Automated Testing
Run all tests with the provided script:
```bash
./test-api.sh
```

### Manual Testing

#### 1. Register a New User
```bash
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "myusername",
    "password": "mypassword123"
  }'
```

**Expected Response (200):**
```json
{
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "myusername",
    "rating": 1000,
    "wins": 0,
    "losses": 0
  },
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### 2. Login
```bash
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "mypassword123"
  }'
```

**Expected Response (200):**
```json
{
  "user": { ... },
  "accessToken": "...",
  "refreshToken": "..."
}
```

#### 3. Get My Profile (Protected)
```bash
# Save your access token first
ACCESS_TOKEN="your_access_token_here"

curl -X GET http://localhost:3000/player/me \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**Expected Response (200):**
```json
{
  "id": 1,
  "email": "user@example.com",
  "username": "myusername",
  "rating": 1000,
  "wins": 0,
  "losses": 0
}
```

#### 4. Get Any Player Profile (Public)
```bash
curl -X GET http://localhost:3000/player/1
```

**Expected Response (200):**
```json
{
  "id": 1,
  "username": "myusername",
  "rating": 1000,
  "wins": 0,
  "losses": 0
}
```

#### 5. Refresh Access Token
```bash
REFRESH_TOKEN="your_refresh_token_here"

curl -X POST http://localhost:3000/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\": \"$REFRESH_TOKEN\"}"
```

**Expected Response (200):**
```json
{
  "accessToken": "...",
  "refreshToken": "..."
}
```

## Error Scenarios

### Validation Errors

#### Invalid Email
```bash
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "invalid-email",
    "username": "test",
    "password": "password123"
  }'
```

**Expected Response (400):**
```json
{
  "message": ["email must be an email"],
  "error": "Bad Request",
  "statusCode": 400
}
```

#### Short Username
```bash
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "ab",
    "password": "password123"
  }'
```

**Expected Response (400):**
```json
{
  "message": ["username must be longer than or equal to 3 characters"],
  "error": "Bad Request",
  "statusCode": 400
}
```

#### Short Password
```bash
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "12345"
  }'
```

**Expected Response (400):**
```json
{
  "message": ["password must be longer than or equal to 6 characters"],
  "error": "Bad Request",
  "statusCode": 400
}
```

### Conflict Errors

#### Duplicate Email
```bash
# Try to register with an existing email
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "newusername",
    "password": "password123"
  }'
```

**Expected Response (409):**
```json
{
  "message": "Email already exists",
  "error": "Conflict",
  "statusCode": 409
}
```

#### Duplicate Username
```bash
# Try to register with an existing username
curl -X POST http://localhost:3000/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newemail@example.com",
    "username": "myusername",
    "password": "password123"
  }'
```

**Expected Response (409):**
```json
{
  "message": "Username already exists",
  "error": "Conflict",
  "statusCode": 409
}
```

### Authentication Errors

#### Invalid Credentials
```bash
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "wrongpassword"
  }'
```

**Expected Response (401):**
```json
{
  "message": "Invalid credentials",
  "statusCode": 401
}
```

#### Unauthorized Access (No Token)
```bash
curl -X GET http://localhost:3000/player/me
```

**Expected Response (401):**
```json
{
  "message": "Unauthorized",
  "statusCode": 401
}
```

#### Invalid Token
```bash
curl -X GET http://localhost:3000/player/me \
  -H "Authorization: Bearer invalid_token"
```

**Expected Response (401):**
```json
{
  "message": "Unauthorized",
  "statusCode": 401
}
```

### Not Found Errors

#### Non-existent Player
```bash
curl -X GET http://localhost:3000/player/99999
```

**Expected Response (404):**
```json
{
  "message": "User not found",
  "error": "Not Found",
  "statusCode": 404
}
```

## Testing with Postman

1. Import the collection:
   - Open Postman
   - Click Import
   - Select `postman_collection.json`

2. Test the endpoints:
   - Registration → Save the tokens
   - Login → Verify tokens are returned
   - Get My Profile → Add token to Authorization header
   - Get Player Profile → No auth needed
   - Refresh Token → Use refresh token from registration

3. Use Postman variables:
   - Set `accessToken` variable after login
   - Use `{{accessToken}}` in Authorization headers

## Testing with HTTPie (Alternative)

If you prefer HTTPie over curl:

```bash
# Install HTTPie
pip install httpie

# Register
http POST localhost:3000/auth/register \
  email=test@example.com \
  username=testuser \
  password=password123

# Login
http POST localhost:3000/auth/login \
  email=test@example.com \
  password=password123

# Get profile with token
http GET localhost:3000/player/me \
  "Authorization: Bearer YOUR_TOKEN"
```

## Database Verification

After testing, verify data in database:

```bash
# Access database
docker exec -it chess_postgres psql -U postgres -d chess_game

# View users
SELECT id, email, username, rating, wins, losses, created_at FROM users;

# View matches (empty for now)
SELECT * FROM matches;

# Exit
\q
```

## Load Testing

For load testing, you can use tools like:

### Apache Bench
```bash
# Test registration endpoint
ab -n 100 -c 10 -T 'application/json' \
  -p register.json \
  http://localhost:3000/auth/register
```

### Artillery
```bash
# Install Artillery
npm install -g artillery

# Create artillery.yml
# Run load test
artillery run artillery.yml
```

## Common Issues

### Port Already in Use
```bash
# Find process using port 3000
lsof -i :3000

# Kill the process
kill -9 <PID>
```

### Database Connection Failed
```bash
# Check if PostgreSQL is running
docker compose ps

# Check database logs
docker compose logs postgres

# Restart services
docker compose restart
```

### JWT Token Expired
- Access tokens expire after 1 hour
- Use the refresh endpoint to get new tokens
- Or login again

## Test Checklist

Use this checklist to verify all functionality:

- [ ] Register new user with valid data
- [ ] Register fails with invalid email
- [ ] Register fails with short username (< 3 chars)
- [ ] Register fails with short password (< 6 chars)
- [ ] Register fails with duplicate email
- [ ] Register fails with duplicate username
- [ ] Login with valid credentials
- [ ] Login fails with wrong password
- [ ] Login fails with non-existent email
- [ ] Get my profile with valid token
- [ ] Get my profile fails without token
- [ ] Get my profile fails with invalid token
- [ ] Get player profile by ID (public)
- [ ] Get player profile returns 404 for non-existent user
- [ ] Refresh token with valid refresh token
- [ ] Refresh token fails with invalid token
- [ ] JWT token contains correct user ID
- [ ] Password is properly hashed (not stored in plain text)
- [ ] Database tables created correctly
- [ ] Foreign key constraints work

## Success Criteria

All tests should pass with:
- ✅ Correct HTTP status codes
- ✅ Proper JSON responses
- ✅ Validation error messages
- ✅ JWT tokens generated and validated
- ✅ Database records created
- ✅ No server errors or crashes
