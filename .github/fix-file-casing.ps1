# Fix file casing in .github folder
# Windows workaround for case-insensitive filesystem

Write-Host "?? Fixing file casing in .github folder..." -ForegroundColor Cyan

# Navigate to repository root
cd C:\github\xavier\FunctionalDDD

# Check if we're in a git repository
if (-not (Test-Path ".git")) {
    Write-Error "Not in a git repository!"
    exit 1
}

# Check for uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Warning "You have uncommitted changes. Please commit or stash them first."
    git status
    exit 1
}

Write-Host "? Repository is clean, proceeding with renames..." -ForegroundColor Green

# Define file renames (old name -> new name)
$renames = @{
    ".github/PROJECT_SPEC_TEMPLATE.md" = "project-spec-template.md"
    ".github/DEMO_GUIDE.md" = "demo-guide.md"
    ".github/FEATURE_TEMPLATE.md" = "feature-template.md"
    ".github/ITERATIVE_DEMO.md" = "iterative-demo.md"
    ".github/PRESENTER_GUIDE.md" = "presenter-guide.md"
}

# Step 1: Rename to temporary names
Write-Host "`n?? Step 1: Renaming to temporary names..." -ForegroundColor Yellow
foreach ($oldName in $renames.Keys) {
    $newName = $renames[$oldName]
    $tempName = "$newName.tmp"
    
    if (Test-Path $oldName) {
        Write-Host "  $oldName -> .github/$tempName" -ForegroundColor Gray
        git mv $oldName ".github/$tempName"
    } else {
        Write-Warning "  File not found: $oldName"
    }
}

# Commit temporary renames
git commit -m "chore: rename files to temp (step 1 of case fix)"

# Step 2: Rename to final names
Write-Host "`n?? Step 2: Renaming to final lowercase names..." -ForegroundColor Yellow
foreach ($oldName in $renames.Keys) {
    $newName = $renames[$oldName]
    $tempName = ".github/$newName.tmp"
    $finalName = ".github/$newName"
    
    if (Test-Path $tempName) {
        Write-Host "  $tempName -> $finalName" -ForegroundColor Gray
        git mv $tempName $finalName
    } else {
        Write-Warning "  Temp file not found: $tempName"
    }
}

# Commit final renames
git commit -m "chore: fix file casing to lowercase-kebab"

Write-Host "`n? File casing fixed successfully!" -ForegroundColor Green
Write-Host "`nFiles renamed:" -ForegroundColor Cyan
foreach ($oldName in $renames.Keys) {
    Write-Host "  ? $oldName -> .github/$($renames[$oldName])" -ForegroundColor Green
}

Write-Host "`n?? Ready to push. Run: git push origin main" -ForegroundColor Yellow

# Show git log to confirm
Write-Host "`nRecent commits:" -ForegroundColor Cyan
git log --oneline -3
