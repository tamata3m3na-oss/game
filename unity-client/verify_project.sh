#!/bin/bash

# =====================================================
# Unity Project Verification Script
# =====================================================
# This script verifies that the Unity project is in a good state

echo "=========================================="
echo "Unity Project Verification Script"
echo "=========================================="
echo ""

# Change to script directory
cd "$(dirname "$0")" || exit 1

PROJECT_ROOT="$(pwd)"
ERRORS=0
WARNINGS=0

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "Project root: $PROJECT_ROOT"
echo ""

# Check 1: Critical files exist
echo "Check 1: Critical project files"
echo "-----------------------------------"

CRITICAL_FILES=(
    "Packages/manifest.json"
    "ProjectSettings/ProjectVersion.txt"
    "ProjectSettings/ProjectSettings.asset"
    "Assets"
)

for file in "${CRITICAL_FILES[@]}"; do
    if [ -e "$file" ]; then
        echo -e "${GREEN}[OK]${NC} Found: $file"
    else
        echo -e "${RED}[ERROR]${NC} Missing: $file"
        ((ERRORS++))
    fi
done
echo ""

# Check 2: Unity directories should not exist (clean state)
echo "Check 2: Clean state (Unity directories should be absent)"
echo "-----------------------------------"

SHOULD_NOT_EXIST=(
    "Library"
    "Temp"
    "obj"
    "Logs"
    "UserSettings"
)

for dir in "${SHOULD_NOT_EXIST[@]}"; do
    if [ -d "$dir" ]; then
        echo -e "${YELLOW}[WARNING]${NC} Still exists (should be cleaned): $dir"
        ((WARNINGS++))
    else
        echo -e "${GREEN}[OK]${NC} Cleaned: $dir (not found as expected)"
    fi
done
echo ""

# Check 3: Git status
echo "Check 3: Git repository status"
echo "-----------------------------------"

if [ -d ".git" ]; then
    if git rev-parse --git-dir > /dev/null 2>&1; then
        echo -e "${GREEN}[OK]${NC} Git repository is valid"

        # Check for uncommitted changes
        if git diff-index --quiet HEAD --; then
            echo -e "${GREEN}[OK]${NC} No uncommitted changes"
        else
            echo -e "${YELLOW}[WARNING]${NC} There are uncommitted changes"
            git status --short
            ((WARNINGS++))
        fi

        # Check for untracked files
        UNTRACKED=$(git ls-files --others --exclude-standard | wc -l)
        if [ "$UNTRACKED" -eq 0 ]; then
            echo -e "${GREEN}[OK]${NC} No untracked files"
        else
            echo -e "${YELLOW}[WARNING]${NC} Found $UNTRACKED untracked files"
            git ls-files --others --exclude-standard | head -10
            ((WARNINGS++))
        fi
    else
        echo -e "${RED}[ERROR]${NC} .git directory exists but not a valid git repo"
        ((ERRORS++))
    fi
else
    echo -e "${YELLOW}[WARNING]${NC} Not a git repository"
    ((WARNINGS++))
fi
echo ""

# Check 4: Project version
echo "Check 4: Project version"
echo "-----------------------------------"

if [ -f "ProjectSettings/ProjectVersion.txt" ]; then
    VERSION=$(grep "m_EditorVersion:" "ProjectSettings/ProjectVersion.txt" | cut -d' ' -f2)
    echo "Current version: $VERSION"

    if [ "$VERSION" = "2022.3.62f3" ]; then
        echo -e "${GREEN}[OK]${NC} Project version is correct (2022.3.62f3)"
    else
        echo -e "${YELLOW}[WARNING]${NC} Project version is $VERSION (expected 2022.3.62f3)"
        ((WARNINGS++))
    fi
else
    echo -e "${RED}[ERROR]${NC} ProjectVersion.txt not found"
    ((ERRORS++))
fi
echo ""

# Check 5: Manifest validation
echo "Check 5: Package manifest validation"
echo "-----------------------------------"

if [ -f "Packages/manifest.json" ]; then
    if python3 -m json.tool "Packages/manifest.json" > /dev/null 2>&1; then
        echo -e "${GREEN}[OK]${NC} manifest.json is valid JSON"
    else
        echo -e "${YELLOW}[WARNING]${NC} Could not validate JSON (python3 not available)"
    fi

    # Check for required packages
    REQUIRED_PACKAGES=(
        "com.unity.inputsystem"
        "com.unity.textmeshpro"
        "com.unity.ugui"
        "com.unity.addressables"
        "com.unity.render-pipelines.universal"
    )

    for pkg in "${REQUIRED_PACKAGES[@]}"; do
        if grep -q "\"$pkg\"" "Packages/manifest.json"; then
            echo -e "${GREEN}[OK]${NC} Found package: $pkg"
        else
            echo -e "${YELLOW}[WARNING]${NC} Missing package: $pkg"
            ((WARNINGS++))
        fi
    done
else
    echo -e "${RED}[ERROR]${NC} manifest.json not found"
    ((ERRORS++))
fi
echo ""

# Check 6: Assets directory structure
echo "Check 6: Assets directory structure"
echo "-----------------------------------"

EXPECTED_FOLDERS=(
    "Assets/Scripts"
    "Assets/Scenes"
    "Assets/Prefabs"
)

for folder in "${EXPECTED_FOLDERS[@]}"; do
    if [ -d "$folder" ]; then
        echo -e "${GREEN}[OK]${NC} Found: $folder"
    else
        echo -e "${YELLOW}[WARNING]${NC} Not found: $folder"
        ((WARNINGS++))
    fi
done
echo ""

# Summary
echo "=========================================="
echo "Verification Complete"
echo "=========================================="
echo ""

if [ $ERRORS -eq 0 ] && [ $WARNINGS -eq 0 ]; then
    echo -e "${GREEN}✓ Project is in perfect condition!${NC}"
    echo ""
    echo "You can now open the project in Unity 2022.3.62f3"
    echo "Wait for the Library folder to be regenerated"
    exit 0
elif [ $ERRORS -eq 0 ]; then
    echo -e "${YELLOW}⚠ Project is OK with $WARNINGS warning(s)${NC}"
    echo ""
    echo "You can open the project in Unity, but review the warnings above"
    exit 0
else
    echo -e "${RED}✗ Project has $ERRORS error(s) and $WARNINGS warning(s)${NC}"
    echo ""
    echo "Please fix the errors before opening in Unity"
    exit 1
fi
