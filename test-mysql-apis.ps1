#!/usr/bin/env pwsh
# Comprehensive MySQL API Testing Script for SqlOptimizer

$baseUrl = 'http://localhost:5119'
$connectionInfo = @{
    databaseType = 1  # 1 = MySQL, 0 = SQL Server
    server = 'localhost'
    database = 'CompanyDB'
    username = 'root'
    password = 'test123'
}

$testResults = @()

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = 'POST',
        [hashtable]$Body
    )
    
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Testing: $Name" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    
    try {
        $jsonBody = $Body | ConvertTo-Json -Depth 10
        $response = Invoke-RestMethod -Uri $Url -Method $Method -Headers @{'Content-Type'='application/json'} -Body $jsonBody -ErrorAction Stop
        
        Write-Host "[PASS] SUCCESS" -ForegroundColor Green
        Write-Host "Response:" -ForegroundColor Gray
        $response | ConvertTo-Json -Depth 5 | Write-Host
        
        $script:testResults += [PSCustomObject]@{
            Test = $Name
            Status = 'PASS'
            Message = 'Success'
        }
        
        return $response
    }
    catch {
        Write-Host "[FAIL] FAILED" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        
        $script:testResults += [PSCustomObject]@{
            Test = $Name
            Status = 'FAIL'
            Message = $_.Exception.Message
        }
        
        return $null
    }
}

Write-Host @"
╔══════════════════════════════════════════════════════════════╗
║     MySQL API Testing Suite - SqlOptimizer                  ║
║     Testing against: localhost/CompanyDB                     ║
╚══════════════════════════════════════════════════════════════╝
"@ -ForegroundColor Yellow

# Test 1: Database Connection
$result1 = Test-Endpoint -Name "1. Database Connection Test" `
    -Url "$baseUrl/api/Database/connect" `
    -Body $connectionInfo

if (-not $result1 -or -not $result1.success) {
    Write-Host "`n[WARNING] Connection test failed. Cannot proceed with other tests." -ForegroundColor Yellow
    Write-Host "Please ensure MySQL is running and credentials are correct." -ForegroundColor Yellow
    exit 1
}

# Test 2: List Databases
Test-Endpoint -Name "2. List Databases" `
    -Url "$baseUrl/api/Metadata/databases" `
    -Body $connectionInfo

# Test 3: List Tables
$tables = Test-Endpoint -Name "3. List Tables" `
    -Url "$baseUrl/api/Metadata/tables" `
    -Body $connectionInfo

# Test 4: List Views
Test-Endpoint -Name "4. List Views" `
    -Url "$baseUrl/api/Metadata/views" `
    -Body $connectionInfo

# Test 5: List Stored Procedures
$procedures = Test-Endpoint -Name "5. List Stored Procedures" `
    -Url "$baseUrl/api/Metadata/stored-procedures" `
    -Body $connectionInfo

# Test 6: List Functions
Test-Endpoint -Name "6. List Functions" `
    -Url "$baseUrl/api/Metadata/functions" `
    -Body $connectionInfo

# Test 7: List Indexes
Test-Endpoint -Name "7. List Indexes" `
    -Url "$baseUrl/api/Metadata/indexes" `
    -Body $connectionInfo

# Test 8: List Foreign Keys
Test-Endpoint -Name "8. List Foreign Keys" `
    -Url "$baseUrl/api/Metadata/foreign-keys" `
    -Body $connectionInfo

# Test 9: Stored Procedure Detail (if procedures exist)
if ($procedures -and $procedures.Count -gt 0) {
    $firstProc = $procedures[0]
    $procName = if ($firstProc.ProcedureName) { $firstProc.ProcedureName } else { $firstProc.procedureName }
    
    if ($procName) {
        $procRequest = $connectionInfo.Clone()
        $procRequest.procedureName = $procName
        
        Test-Endpoint -Name "9. Get Stored Procedure Detail ($procName)" `
            -Url "$baseUrl/api/StoredProcedure/detail" `
            -Body $procRequest
            
        # Test 10: Get Execution Plan
        $execPlanRequest = $connectionInfo.Clone()
        $execPlanRequest.storedProcedureName = $procName
        
        Test-Endpoint -Name "10. Get Execution Plan ($procName)" `
            -Url "$baseUrl/api/ExecutionPlan" `
            -Body $execPlanRequest
    }
    else {
        Write-Host "`n[WARNING] Skipping procedure detail tests - no procedure name found" -ForegroundColor Yellow
    }
}
else {
    Write-Host "`n[WARNING] Skipping procedure detail tests - no stored procedures found" -ForegroundColor Yellow
}

# Test 11: Dashboard Overview
$dashRequest = @{
    databaseType = 1
    serverName = 'localhost'
    databaseName = 'CompanyDB'
    useWindowsAuth = $false
    username = 'root'
    password = 'test123'
}

Test-Endpoint -Name "11. Dashboard Overview" `
    -Url "$baseUrl/api/Dashboard/overview" `
    -Body $dashRequest

# Display Summary
Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║                     TEST SUMMARY                             ║" -ForegroundColor Yellow
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

$passCount = ($testResults | Where-Object { $_.Status -eq 'PASS' }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq 'FAIL' }).Count
$totalCount = $testResults.Count

Write-Host "`nTotal Tests: $totalCount" -ForegroundColor Cyan
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red

Write-Host "`nDetailed Results:" -ForegroundColor Cyan
$testResults | Format-Table -AutoSize

if ($failCount -eq 0) {
    Write-Host "`n[SUCCESS] All tests passed! MySQL support is working correctly." -ForegroundColor Green
}
else {
    Write-Host "`n[WARNING] Some tests failed. Review the errors above." -ForegroundColor Yellow
}

Write-Host "`n[OK] Testing complete!" -ForegroundColor Cyan
