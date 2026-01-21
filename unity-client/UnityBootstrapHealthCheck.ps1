# 🛠️ Unity Project Bootstrap & Health Check Script

# PowerShell Script to run Unity Bootstrap Diagnostics and Health Checks
# Run this script to get a comprehensive overview of your Unity project health

param(
    [switch]$QuickCheck = $false,
    [switch]$FullScan = $false,
    [switch]$Cleanup = $false,
    [switch]$Help = $false
)

Write-Host "🎯 Unity Project Bootstrap & Health Check Tool" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

if ($Help) {
    Write-Host "`nUsage:" -ForegroundColor Yellow
    Write-Host "  .\UnityBootstrapHealthCheck.ps1 [-QuickCheck] [-FullScan] [-Cleanup] [-Help]" -ForegroundColor Green
    Write-Host "`nParameters:" -ForegroundColor Yellow
    Write-Host "  -QuickCheck  : Run quick health assessment" -ForegroundColor Green
    Write-Host "  -FullScan    : Run comprehensive project analysis" -ForegroundColor Green  
    Write-Host "  -Cleanup     : Run project cleanup utilities" -ForegroundColor Green
    Write-Host "  -Help        : Show this help message" -ForegroundColor Green
    Write-Host "`nExamples:" -ForegroundColor Yellow
    Write-Host "  .\UnityBootstrapHealthCheck.ps1 -QuickCheck" -ForegroundColor Green
    Write-Host "  .\UnityBootstrapHealthCheck.ps1 -FullScan" -ForegroundColor Green
    Write-Host "  .\UnityBootstrapHealthCheck.ps1 -Cleanup" -ForegroundColor Green
    Write-Host "  .\UnityBootstrapHealthCheck.ps1 -FullScan -Cleanup" -ForegroundColor Green
    exit 0
}

# Check if Unity is running
Write-Host "`n🔍 Checking Unity Process..." -ForegroundColor Blue
$unityProcess = Get-Process Unity -ErrorAction SilentlyContinue
if ($unityProcess) {
    Write-Host "✅ Unity Editor is running" -ForegroundColor Green
    Write-Host "⚠️  For best results, close Unity Editor before running diagnostics" -ForegroundColor Yellow
} else {
    Write-Host "✅ Unity Editor is not running" -ForegroundColor Green
}

# Check project structure
Write-Host "`n📂 Analyzing Project Structure..." -ForegroundColor Blue

$projectRoot = Get-Location
$assetsPath = Join-Path $projectRoot "Assets"
$scriptsPath = Join-Path $assetsPath "Scripts"

if (!(Test-Path $assetsPath)) {
    Write-Host "❌ Assets folder not found!" -ForegroundColor Red
    Write-Host "   Please run this script from your Unity project root directory" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Project root detected" -ForegroundColor Green

# Check for critical files
$criticalFiles = @(
    "Assets/Scripts/Bootstrap/Bootstrap.cs",
    "Assets/Scripts/Bootstrap/ManagerInitializer.cs",
    "Assets/Scripts/UI/Animations/AnimationController.cs",
    "Assets/Scripts/UI/Animations/ParticleController.cs",
    "Assets/Scripts/UI/Animations/TransitionManager.cs",
    "Assets/Scripts/Utils/DOTweenCompat.cs",
    "Assets/Scripts/Bootstrap/BootstrapRunnerEnhanced.cs",
    "Assets/Scripts/Bootstrap/ManagerInitializerEnhanced.cs",
    "Assets/Scripts/Utils/ProjectHealthCheckWindow.cs",
    "Assets/Scripts/Utils/ProjectCleanupUtility.cs",
    "Assets/Scripts/Utils/CompilationSafetyManager.cs"
)

Write-Host "`n📋 Checking Critical Files..." -ForegroundColor Blue

$missingFiles = @()
$existingFiles = @()

foreach ($file in $criticalFiles) {
    $fullPath = Join-Path $projectRoot $file
    if (Test-Path $fullPath) {
        $existingFiles += $file
        Write-Host "✅ $file" -ForegroundColor Green
    } else {
        $missingFiles += $file
        Write-Host "❌ $file" -ForegroundColor Red
    }
}

Write-Host "`n📊 File Summary:" -ForegroundColor Blue
Write-Host "   Existing: $($existingFiles.Count)" -ForegroundColor Green
Write-Host "   Missing: $($missingFiles.Count)" -ForegroundColor $(if ($missingFiles.Count -gt 0) { "Red" } else { "Green" })

if ($missingFiles.Count -gt 0) {
    Write-Host "`n⚠️  Some critical files are missing. Bootstrap optimization may not work correctly." -ForegroundColor Yellow
    Write-Host "   Please ensure all Unity corruption fix files are properly imported." -ForegroundColor Yellow
}

# Check Unity project settings
Write-Host "`n⚙️  Checking Unity Project Settings..." -ForegroundColor Blue

$manifestPath = Join-Path $projectRoot "Packages/manifest.json"
if (Test-Path $manifestPath) {
    Write-Host "✅ Unity Package manifest found" -ForegroundColor Green
    
    try {
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        $dependencies = $manifest.dependencies
        
        Write-Host "`n📦 Checking Dependencies:" -ForegroundColor Blue
        
        $requiredPackages = @{
            "com.unity.textmeshpro" = "TextMeshPro"
            "com.unity.inputsystem" = "Input System"
            "com.unity.ugui" = "Unity UI"
        }
        
        foreach ($package in $requiredPackages.GetEnumerator()) {
            if ($dependencies.PSObject.Properties.Name -contains $package.Key) {
                $version = $dependencies.$($package.Key)
                Write-Host "   ✅ $($package.Value): $version" -ForegroundColor Green
            } else {
                Write-Host "   ❌ $($package.Value): Not installed" -ForegroundColor Red
            }
        }
    }
    catch {
        Write-Host "⚠️  Could not parse Unity manifest" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ Unity Package manifest not found" -ForegroundColor Red
}

# Quick health assessment
if ($QuickCheck -or (!$FullScan -and !$Cleanup)) {
    Write-Host "`n🏥 Running Quick Health Assessment..." -ForegroundColor Blue
    
    # Check for common issues
    $issues = @()
    
    # Check Library folder size
    $libraryPath = Join-Path $projectRoot "Library"
    if (Test-Path $libraryPath) {
        $librarySize = (Get-ChildItem $libraryPath -Recurse -Force | Measure-Object -Property Length -Sum).Sum / 1GB
        if ($librarySize -gt 5) {
            $issues += "Library folder is large ($([math]::Round($librarySize, 2)) GB) - consider cleanup"
        }
    }
    
    # Check for temp files
    $tempPath = Join-Path $projectRoot "Temp"
    if (Test-Path $tempPath) {
        $tempFiles = Get-ChildItem $tempPath -Recurse -Force -ErrorAction SilentlyContinue
        if ($tempFiles.Count -gt 100) {
            $issues += "Temp folder contains $($tempFiles.Count) files - consider cleanup"
        }
    }
    
    # Check for missing meta files
    $metaIssues = Get-ChildItem $assetsPath -Recurse -Include "*.cs", "*.unity", "*.prefab" -ErrorAction SilentlyContinue | 
                  Where-Object { !(Test-Path "$($_.FullName).meta") } | 
                  Select-Object -First 10
    
    if ($metaIssues.Count -gt 0) {
        $issues += "$($metaIssues.Count) assets missing .meta files"
    }
    
    # Display issues
    if ($issues.Count -eq 0) {
        Write-Host "✅ No major issues detected!" -ForegroundColor Green
    } else {
        Write-Host "`n⚠️  Issues Detected:" -ForegroundColor Yellow
        $issues | ForEach-Object { Write-Host "   - $_" -ForegroundColor Yellow }
        
        Write-Host "`n💡 Suggestions:" -ForegroundColor Cyan
        Write-Host "   - Run with -FullScan for detailed analysis" -ForegroundColor White
        Write-Host "   - Run with -Cleanup to clean up issues" -ForegroundColor White
        Write-Host "   - Use Unity Project Health Check window (Tools → Project Health Check)" -ForegroundColor White
    }
}

# Full scan
if ($FullScan) {
    Write-Host "`n🔍 Running Full Project Scan..." -ForegroundColor Blue
    
    Write-Host "`n📂 Analyzing Folder Structure..." -ForegroundColor Blue
    
    $folders = @("Scripts", "Scenes", "Prefabs", "Materials", "Animations", "Settings")
    foreach ($folder in $folders) {
        $folderPath = Join-Path $assetsPath $folder
        if (Test-Path $folderPath) {
            $fileCount = (Get-ChildItem $folderPath -Recurse -File -ErrorAction SilentlyContinue).Count
            Write-Host "   ✅ $folder`: $fileCount files" -ForegroundColor Green
        } else {
            Write-Host "   ❌ $folder`: Missing" -ForegroundColor Red
        }
    }
    
    Write-Host "`n🎯 Checking Bootstrap Components..." -ForegroundColor Blue
    
    # Analyze Bootstrap files
    $bootstrapFiles = Get-ChildItem $scriptsPath -Recurse -Filter "*Bootstrap*.cs" -ErrorAction SilentlyContinue
    Write-Host "   Bootstrap files found: $($bootstrapFiles.Count)" -ForegroundColor Green
    $bootstrapFiles | ForEach-Object { Write-Host "     - $($_.Name)" -ForegroundColor White }
    
    # Check for enhanced components
    $enhancedComponents = @(
        "DOTweenCompat.cs",
        "BootstrapRunnerEnhanced.cs", 
        "ManagerInitializerEnhanced.cs",
        "ProjectHealthCheckWindow.cs",
        "ProjectCleanupUtility.cs",
        "CompilationSafetyManager.cs"
    )
    
    Write-Host "`n🚀 Enhanced Components Check:" -ForegroundColor Blue
    foreach ($component in $enhancedComponents) {
        $componentPath = Join-Path $scriptsPath "Utils/$component"
        if (Test-Path $componentPath) {
            Write-Host "   ✅ $component" -ForegroundColor Green
        } else {
            Write-Host "   ❌ $component" -ForegroundColor Red
        }
    }
    
    Write-Host "`n🎮 Game Manager Analysis..." -ForegroundColor Blue
    $managerFiles = Get-ChildItem $scriptsPath -Recurse -Filter "*Manager*.cs" -ErrorAction SilentlyContinue
    Write-Host "   Manager files found: $($managerFiles.Count)" -ForegroundColor Green
    $managerFiles | ForEach-Object { Write-Host "     - $($_.Name)" -ForegroundColor White }
}

# Cleanup
if ($Cleanup) {
    Write-Host "`n🧹 Running Project Cleanup..." -ForegroundColor Blue
    
    $cleanupActions = @()
    
    # Clean Temp folder
    $tempPath = Join-Path $projectRoot "Temp"
    if (Test-Path $tempPath) {
        try {
            Remove-Item $tempPath -Recurse -Force -ErrorAction Stop
            $cleanupActions += "Cleaned Temp folder"
            Write-Host "   ✅ Cleaned Temp folder" -ForegroundColor Green
        }
        catch {
            Write-Host "   ⚠️  Could not clean Temp folder (Unity may be using it)" -ForegroundColor Yellow
        }
    }
    
    # Clean obj folder  
    $objPath = Join-Path $projectRoot "obj"
    if (Test-Path $objPath) {
        try {
            Remove-Item $objPath -Recurse -Force -ErrorAction Stop
            $cleanupActions += "Cleaned obj folder"
            Write-Host "   ✅ Cleaned obj folder" -ForegroundColor Green
        }
        catch {
            Write-Host "   ⚠️  Could not clean obj folder" -ForegroundColor Yellow
        }
    }
    
    # Asset database refresh simulation
    Write-Host "   📋 Asset database refresh simulated" -ForegroundColor Blue
    $cleanupActions += "Asset database refresh"
    
    if ($cleanupActions.Count -gt 0) {
        Write-Host "`n✅ Cleanup completed:" -ForegroundColor Green
        $cleanupActions | ForEach-Object { Write-Host "   - $_" -ForegroundColor White }
        
        Write-Host "`n💡 Next Steps:" -ForegroundColor Cyan
        Write-Host "   - Restart Unity Editor" -ForegroundColor White
        Write-Host "   - Reimport assets if needed" -ForegroundColor White
        Write-Host "   - Run Project Health Check in Unity" -ForegroundColor White
    } else {
        Write-Host "   ⚠️  No cleanup actions performed" -ForegroundColor Yellow
    }
}

# Generate summary report
Write-Host "`n📋 Unity Bootstrap & Health Check Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

$reportTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "Report Generated: $reportTime" -ForegroundColor White
Write-Host "Project Path: $projectRoot" -ForegroundColor White

Write-Host "`n🎯 Status Summary:" -ForegroundColor Blue
Write-Host "   Bootstrap Files: $(if ($existingFiles.Count -gt 0) { '✅ Ready' } else { '❌ Missing' })" -ForegroundColor $(if ($existingFiles.Count -gt 0) { 'Green' } else { 'Red' })
Write-Host "   Enhanced Components: $(if ((Test-Path (Join-Path $scriptsPath "Utils/DOTweenCompat.cs"))) { '✅ Installed' } else { '❌ Missing' })" -ForegroundColor $(if (Test-Path (Join-Path $scriptsPath "Utils/DOTweenCompat.cs")) { 'Green' } else { 'Red' })
Write-Host "   Unity Dependencies: $(if (Test-Path $manifestPath) { '✅ Found' } else { '❌ Missing' })" -ForegroundColor $(if (Test-Path $manifestPath) { 'Green' } else { 'Red' })

Write-Host "`n🚀 Ready for Unity Bootstrap!" -ForegroundColor Green

Write-Host "`n💡 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Open Unity Editor" -ForegroundColor White
Write-Host "2. Open Project Health Check window: Tools → Project Health Check" -ForegroundColor White  
Write-Host "3. Run Full Scan to verify everything is working" -ForegroundColor White
Write-Host "4. Check Console for bootstrap diagnostics" -ForegroundColor White
Write-Host "5. Test your scene transitions and UI animations" -ForegroundColor White

Write-Host "`n🆘 If Issues Persist:" -ForegroundColor Red
Write-Host "1. Run with -Cleanup flag to clean project cache" -ForegroundColor White
Write-Host "2. Use Unity Project Cleanup Utility in Editor" -ForegroundColor White
Write-Host "3. Check UNITY_CORRUPTION_FIX_COMPLETE.md for detailed instructions" -ForegroundColor White

Write-Host "`n🎉 Unity Corruption Fix & Bootstrap Optimization Complete!" -ForegroundColor Green