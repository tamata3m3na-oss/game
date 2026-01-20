@echo off
REM =====================================================
REM Unity Project Deep Clean Script for Windows
REM =====================================================
REM This script performs a comprehensive clean of Unity project
REM to resolve version conflicts and cache issues

echo ==========================================
echo Unity Project Deep Clean Script (Windows)
echo ==========================================
echo.

REM Change to script directory
cd /d "%~dp0"
set PROJECT_ROOT=%CD%
echo Project root: %PROJECT_ROOT%
echo.

REM Step 1: Remove Unity-generated directories
echo Step 1: Removing Unity-generated directories...

if exist "Library" (
    echo   - Removing: Library
    rmdir /s /q "Library"
) else (
    echo   - Skipped (not found): Library
)

if exist "Temp" (
    echo   - Removing: Temp
    rmdir /s /q "Temp"
) else (
    echo   - Skipped (not found): Temp
)

if exist "obj" (
    echo   - Removing: obj
    rmdir /s /q "obj"
) else (
    echo   - Skipped (not found): obj
)

if exist "Logs" (
    echo   - Removing: Logs
    rmdir /s /q "Logs"
) else (
    echo   - Skipped (not found): Logs
)

if exist "UserSettings" (
    echo   - Removing: UserSettings
    rmdir /s /q "UserSettings"
) else (
    echo   - Skipped (not found): UserSettings
)

if exist "MemoryCaptures" (
    echo   - Removing: MemoryCaptures
    rmdir /s /q "MemoryCaptures"
) else (
    echo   - Skipped (not found): MemoryCaptures
)

echo.

REM Step 2: Remove Unity cache files
echo Step 2: Removing Unity cache files...

del /f /q *.pidb 2>nul && echo   - Removed: *.pidb files
del /f /q *.pdb 2>nul && echo   - Removed: *.pdb files
del /f /q *.mdb 2>nul && echo   - Removed: *.mdb files
del /f /q *.opendb 2>nul && echo   - Removed: *.opendb files
del /f /q "*.VC.db" 2>nul && echo   - Removed: *.VC.db files
del /f /q sysinfo.txt 2>nul && echo   - Removed: sysinfo.txt

echo.

REM Step 3: Clean Addressables cache
echo Step 3: Cleaning Addressables cache...
if exist "Assets\AddressableAssetsData" (
    del /f /s /q "Assets\AddressableAssetsData\*.bin*" 2>nul
    echo   - Addressables cache cleaned
) else (
    echo   - No Addressables directory found
)

echo.

REM Step 4: Git operations (if git is available)
echo Step 4: Resetting git tracked files...
where git >nul 2>nul
if %errorlevel% equ 0 (
    echo   - Running: git checkout .
    git checkout .
    echo.

    echo   - Running: git clean -fdx
    git clean -fdx
) else (
    echo   - Git not found in PATH, skipping git operations
)

echo.

REM Step 5: Verify critical files
echo Step 5: Verifying critical project files...

if exist "Packages\manifest.json" (
    echo   [OK] Found: Packages\manifest.json
) else (
    echo   [MISSING] Packages\manifest.json
)

if exist "ProjectSettings\ProjectVersion.txt" (
    echo   [OK] Found: ProjectSettings\ProjectVersion.txt
) else (
    echo   [MISSING] ProjectSettings\ProjectVersion.txt
)

if exist "ProjectSettings\ProjectSettings.asset" (
    echo   [OK] Found: ProjectSettings\ProjectSettings.asset
) else (
    echo   [MISSING] ProjectSettings\ProjectSettings.asset
)

echo.

REM Summary
echo ==========================================
echo Clean Complete!
echo ==========================================
echo.
echo Next steps:
echo 1. Open the project in Unity 2022.3.62f3
echo 2. Wait for Unity to reimport assets
echo 3. Check Console for any errors
echo 4. Verify packages are installed correctly
echo.

pause
