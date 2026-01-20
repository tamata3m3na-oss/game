#!/bin/bash

# =====================================================
# Unity Project Deep Clean Script
# =====================================================
# This script performs a comprehensive clean of Unity project
# to resolve version conflicts and cache issues

echo "=========================================="
echo "Unity Project Deep Clean Script"
echo "=========================================="
echo ""

# Change to project directory
cd "$(dirname "$0")" || exit 1

PROJECT_ROOT="$(pwd)"
echo "Project root: $PROJECT_ROOT"
echo ""

# Step 1: Remove Unity-generated directories
echo "Step 1: Removing Unity-generated directories..."
DIRECTORIES_TO_REMOVE=(
    "Library"
    "Temp"
    "obj"
    "Logs"
    "UserSettings"
    "MemoryCaptures"
)

for dir in "${DIRECTORIES_TO_REMOVE[@]}"; do
    if [ -d "$dir" ]; then
        echo "  - Removing: $dir"
        rm -rf "$dir"
    else
        echo "  - Skipped (not found): $dir"
    fi
done
echo ""

# Step 2: Remove Unity cache files
echo "Step 2: Removing Unity cache files..."
CACHE_FILES=(
    "*.pidb"
    "*.pdb"
    "*.mdb"
    "*.opendb"
    "*.VC.db"
    "sysinfo.txt"
)

for pattern in "${CACHE_FILES[@]}"; do
    find "$PROJECT_ROOT" -type f -name "$pattern" -delete 2>/dev/null && echo "  - Removed: $pattern" || echo "  - No files found: $pattern"
done
echo ""

# Step 3: Clean Addressables cache
echo "Step 3: Cleaning Addressables cache..."
if [ -d "Assets/AddressableAssetsData" ]; then
    find "Assets/AddressableAssetsData" -type f -name "*.bin*" -delete 2>/dev/null
    echo "  - Addressables cache cleaned"
else
    echo "  - No Addressables directory found"
fi
echo ""

# Step 4: Reset git tracked files to clean state
echo "Step 4: Resetting git tracked files..."
if [ -d ".git" ]; then
    echo "  - Running: git checkout ."
    git checkout . 2>&1 | grep -E "^(M|D|Updated)" || echo "  - No tracked files modified"
    echo ""

    echo "  - Running: git clean -fdx"
    git clean -fdx 2>&1 | grep -v "^$" || echo "  - No untracked files"
else
    echo "  - Not a git repository, skipping git operations"
fi
echo ""

# Step 5: Verify critical files
echo "Step 5: Verifying critical project files..."
CRITICAL_FILES=(
    "Packages/manifest.json"
    "ProjectSettings/ProjectVersion.txt"
    "ProjectSettings/ProjectSettings.asset"
)

ALL_EXIST=true
for file in "${CRITICAL_FILES[@]}"; do
    if [ -f "$file" ]; then
        echo "  ✓ Found: $file"
    else
        echo "  ✗ Missing: $file"
        ALL_EXIST=false
    fi
done
echo ""

# Summary
echo "=========================================="
echo "Clean Complete!"
echo "=========================================="
echo ""
echo "Next steps:"
echo "1. Open the project in Unity 2022.3.62f3"
echo "2. Wait for Unity to reimport assets"
echo "3. Check Console for any errors"
echo "4. Verify packages are installed correctly"
echo ""

if [ "$ALL_EXIST" = true ]; then
    echo "✓ All critical project files are present"
else
    echo "✗ Some critical project files are missing!"
    echo "  Please restore them from git or backup"
fi
