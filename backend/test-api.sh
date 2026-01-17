#!/bin/bash

echo "=== Testing Chess Game API ==="
echo ""

BASE_URL="http://localhost:3000"

echo "1. Testing Registration..."
REGISTER_RESPONSE=$(curl -s -X POST $BASE_URL/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "player2@example.com", "username": "player2", "password": "password123"}')
echo $REGISTER_RESPONSE | jq
ACCESS_TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.accessToken')
REFRESH_TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.refreshToken')
echo ""

echo "2. Testing Login..."
LOGIN_RESPONSE=$(curl -s -X POST $BASE_URL/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "player2@example.com", "password": "password123"}')
echo $LOGIN_RESPONSE | jq
echo ""

echo "3. Testing Get My Profile (Protected)..."
curl -s -X GET $BASE_URL/player/me \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq
echo ""

echo "4. Testing Get Player Profile by ID..."
curl -s -X GET $BASE_URL/player/1 | jq
echo ""

echo "5. Testing Refresh Token..."
curl -s -X POST $BASE_URL/auth/refresh \
  -H "Content-Type: application/json" \
  -d "{\"refreshToken\": \"$REFRESH_TOKEN\"}" | jq
echo ""

echo "6. Testing Unauthorized Access..."
curl -s -X GET $BASE_URL/player/me | jq
echo ""

echo "7. Testing Validation Errors..."
curl -s -X POST $BASE_URL/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "invalid", "username": "ab", "password": "12345"}' | jq
echo ""

echo "8. Testing Duplicate Email..."
curl -s -X POST $BASE_URL/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email": "player2@example.com", "username": "player3", "password": "password123"}' | jq
echo ""

echo "=== All tests completed ==="
