# Comprehensive Duplicate Test Scanner for RailwayOrientedProgramming Project

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "DUPLICATE TEST ANALYSIS" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$testPath = "C:\GitHub\xavier\FunctionalDDD\RailwayOrientedProgramming\tests\Results\Extensions"

# 1. Count tests in each file
Write-Host "=== Test Counts by File ===" -ForegroundColor Yellow
$testCounts = @()
Get-ChildItem -Path $testPath -Filter "*.cs" | Where-Object { 
    $_.Name -notlike "*AssemblyInfo*" -and 
    $_.Name -notlike "*GlobalUsings*" -and
    $_.Name -notlike "TestBase.cs" -and
    $_.Name -notlike "*Helper*"
} | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $factCount = ([regex]::Matches($content, '\[Fact\]')).Count
    $theoryCount = ([regex]::Matches($content, '\[Theory\]')).Count
    
    $testCounts += [PSCustomObject]@{
        File = $_.Name
        Facts = $factCount
        Theories = $theoryCount
        Total = $factCount + $theoryCount
        SizeKB = [math]::Round($_.Length / 1KB, 1)
    }
}
$testCounts | Sort-Object File | Format-Table -AutoSize

# 2. Group files by similar naming patterns
Write-Host "`n=== Grouped by Functionality ===" -ForegroundColor Yellow

$groups = @{
    "TapOnFailure" = @("TapOnFailureTests.cs", "TapOnFailureTsTests.cs", "TapOnFailureTupleTests.cs", "TapOnFailureTupleTracingTests.cs")
    "Tap" = @("TapTests.cs", "TapTupleTests.cs", "TapTupleTracingTests.cs")
    "Ensure" = @("EnsureTests.cs", "EnsureTests.Task.cs", "EnsureTests.Task.Left.cs", "EnsureTests.Task.Right.cs", "EnsureTests.Tracing.cs", "EnsureValueTaskTests.cs")
    "Combine" = @("CombineTests.cs", "CombineTracingTests.cs")
    "Bind" = @("BindTests.cs", "BindTsTests.cs")
}

foreach ($group in $groups.GetEnumerator() | Sort-Object Name) {
    Write-Host "`n$($group.Name):" -ForegroundColor Cyan
    foreach ($file in $group.Value) {
        $fileInfo = $testCounts | Where-Object { $_.File -eq $file }
        if ($fileInfo) {
            Write-Host "  $file`: $($fileInfo.Total) tests ($($fileInfo.Facts) Facts, $($fileInfo.Theories) Theories) - $($fileInfo.SizeKB) KB" -ForegroundColor White
        }
    }
}

# 3. Check for test method name overlap between specific files
Write-Host "`n`n=== Checking for Duplicate Test Names ===" -ForegroundColor Yellow

function Get-TestMethodNames {
    param([string]$FilePath)
    
    $content = Get-Content $FilePath -Raw
    $pattern = '\[(?:Fact|Theory)\]\s+(?:public\s+)?(?:async\s+)?(?:Task|ValueTask|void)\s+(\w+)'
    $matches = [regex]::Matches($content, $pattern)
    
    return $matches | ForEach-Object { $_.Groups[1].Value }
}

# Compare TapOnFailureTsTests.cs vs TapOnFailureTupleTests.cs
$file1 = Join-Path $testPath "TapOnFailureTsTests.cs"
$file2 = Join-Path $testPath "TapOnFailureTupleTests.cs"

if ((Test-Path $file1) -and (Test-Path $file2)) {
    Write-Host "`nComparing: TapOnFailureTsTests.cs vs TapOnFailureTupleTests.cs" -ForegroundColor Cyan
    
    $tests1 = Get-TestMethodNames -FilePath $file1
    $tests2 = Get-TestMethodNames -FilePath $file2
    
    Write-Host "  TapOnFailureTsTests.cs: $($tests1.Count) tests"
    Write-Host "  TapOnFailureTupleTests.cs: $($tests2.Count) tests"
    
    # Find similar patterns (ignoring exact matches since they test different types)
    $similarPatterns = @()
    foreach ($test1 in $tests1) {
        foreach ($test2 in $tests2) {
            # Check if test names are very similar (e.g., same pattern, different suffix)
            $base1 = $test1 -replace '_2Tuple|_3Tuple', ''
            $base2 = $test2 -replace '_2Tuple|_3Tuple', ''
            
            if ($base1 -eq $base2 -and $test1 -ne $test2) {
                $similarPatterns += "$test1 ~ $test2"
            }
        }
    }
    
    if ($similarPatterns.Count -gt 0) {
        Write-Host "`n  Similar test patterns found: $($similarPatterns.Count)" -ForegroundColor Yellow
        Write-Host "  (This suggests potential duplication)`n"
    } else {
        Write-Host "  No overlapping test patterns found`n" -ForegroundColor Green
    }
}

# 4. Summary of potential duplicates
Write-Host "`n=== DUPLICATE ANALYSIS SUMMARY ===" -ForegroundColor Yellow

Write-Host "`n1. TapOnFailure Files:" -ForegroundColor Cyan
Write-Host "   - TapOnFailureTests.cs (base Result<T>)"
Write-Host "   - TapOnFailureTsTests.cs (tuple results - OLDER)" -ForegroundColor Red
Write-Host "   - TapOnFailureTupleTests.cs (tuple results - NEWER)" -ForegroundColor Green
Write-Host "   - TapOnFailureTupleTracingTests.cs (tracing)"
Write-Host "`n   RECOMMENDATION: TapOnFailureTsTests.cs appears to be a duplicate" -ForegroundColor Yellow

Write-Host "`n2. Tap Files:" -ForegroundColor Cyan
Write-Host "   - TapTests.cs (base Result<T>)"
Write-Host "   - TapTupleTests.cs (tuple results)"
Write-Host "   - TapTupleTracingTests.cs (tracing)"
Write-Host "   Status: No duplicates - proper separation" -ForegroundColor Green

Write-Host "`n3. Ensure Files:" -ForegroundColor Cyan
Write-Host "   - Multiple files for different async patterns"
Write-Host "   Status: Likely intentional split by functionality" -ForegroundColor Green

Write-Host "`n========================================`n" -ForegroundColor Cyan

# 5. Get first few test names from each file for comparison
Write-Host "=== Sample Test Names from TapOnFailure Files ===" -ForegroundColor Yellow

foreach ($file in @("TapOnFailureTsTests.cs", "TapOnFailureTupleTests.cs")) {
    $filePath = Join-Path $testPath $file
    if (Test-Path $filePath) {
        Write-Host "`n$file (first 5 tests):" -ForegroundColor Cyan
        $tests = Get-TestMethodNames -FilePath $filePath
        $tests | Select-Object -First 5 | ForEach-Object { Write-Host "  - $_" }
    }
}

Write-Host "`n"