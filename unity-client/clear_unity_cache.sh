#!/bin/bash

# Unity Cache Clear and Project Update Script
# This script clears all Unity cache files and pulls latest updates from repository

echo "=========================================="
echo "Unity Cache Clear and Project Update Script"
echo "=========================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if we're in the correct directory
if [ ! -f "Packages/manifest.json" ]; then
    echo -e "${RED}Error: This script must be run from the unity-client directory${NC}"
    exit 1
fi

echo -e "${YELLOW}Step 1: Checking for Unity cache directories...${NC}"
echo ""

# Function to remove directory if exists
remove_if_exists() {
    if [ -d "$1" ]; then
        echo -e "${RED}Removing: $1${NC}"
        rm -rf "$1"
        echo -e "${GREEN}✓ Removed${NC}"
    else
        echo -e "${GREEN}✓ Not found (OK)${NC}"
    fi
}

# Remove Unity cache directories
remove_if_exists "Library"
remove_if_exists "Temp"
remove_if_exists "obj"
remove_if_exists ".vs"
remove_if_exists ".gradle"
remove_if_exists "Logs"
remove_if_exists "UserSettings"

echo ""
echo -e "${YELLOW}Step 2: Checking for Visual Studio solution files...${NC}"
echo ""

# Remove VS solution files
find . -maxdepth 1 -name "*.sln" -type f 2>/dev/null | while read file; do
    echo -e "${RED}Removing: $file${NC}"
    rm -f "$file"
    echo -e "${GREEN}✓ Removed${NC}"
done

find . -maxdepth 1 -name "*.csproj" -type f 2>/dev/null | while read file; do
    echo -e "${RED}Removing: $file${NC}"
    rm -f "$file"
    echo -e "${GREEN}✓ Removed${NC}"
done

echo ""
echo -e "${YELLOW}Step 3: Pulling latest updates from repository...${NC}"
echo ""

# Go to parent directory and pull from git
cd ..
git fetch origin
if [ $? -ne 0 ]; then
    echo -e "${RED}Error: Failed to fetch from remote repository${NC}"
    exit 1
fi

# Check if we're on a feature branch
current_branch=$(git rev-parse --abbrev-ref HEAD)
echo -e "${GREEN}Current branch: $current_branch${NC}"

# Reset to match origin/main
echo -e "${YELLOW}Resetting to origin/main...${NC}"
git reset --hard origin/main
if [ $? -ne 0 ]; then
    echo -e "${RED}Error: Failed to reset to origin/main${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Successfully reset to origin/main${NC}"

echo ""
echo -e "${YELLOW}Step 4: Verifying GameStateManager.cs...${NC}"
echo ""

# Verify GameStateManager.cs exists and has correct content
if [ -f "unity-client/Assets/Scripts/Managers/GameStateManager.cs" ]; then
    lines=$(wc -l < "unity-client/Assets/Scripts/Managers/GameStateManager.cs")
    echo -e "${GREEN}✓ GameStateManager.cs exists ($lines lines)${NC}"
    
    # Check for key indicators of correct file
    if grep -q "NetworkManager.NetworkGameState" "unity-client/Assets/Scripts/Managers/GameStateManager.cs"; then
        echo -e "${GREEN}✓ File contains NetworkManager.NetworkGameState (correct)${NC}"
    else
        echo -e "${RED}✗ Missing NetworkManager.NetworkGameState${NC}"
    fi
else
    echo -e "${RED}✗ GameStateManager.cs not found!${NC}"
fi

echo ""
echo "=========================================="
echo -e "${GREEN}Cache Clear and Update Complete!${NC}"
echo "=========================================="
echo ""
echo "Next Steps:"
echo "1. Open Unity Editor"
echo "2. Let Unity rebuild the project (3-4 minutes)"
echo "3. Do NOT interrupt the build process"
echo "4. After rebuild, go to Assets > Reimport All"
echo "5. Verify all CS0426 errors are gone"
echo ""
echo -e "${YELLOW}⚠️  Important: Do NOT open Unity before completing this script!${NC}"
echo ""
