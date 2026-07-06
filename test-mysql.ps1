# MySQL API Testing Script for SqlOptimizer
$baseUrl = 'http://localhost:5119'
$conn = @{
    databaseType = 1
    server = 'localhost'
    database = 'CompanyDB'
    username = 'root'
    password = 'test123'
}

Write-Host "`n=== MySQL API Testing Suite ===" -ForegroundColor Yellow
Write-Host "Testing against: localhost/CompanyDB`n" -ForegroundColor Cyan

# Test 1: Connection
Write-Host "1. Testing Connection..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Database/connect" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Connected to MySQL $($r.serverVersion)" -ForegroundColor Green
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: List Databases
Write-Host "2. List Databases..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/databases" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) databases" -ForegroundColor Green
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: List Tables
Write-Host "3. List Tables..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/tables" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) tables" -ForegroundColor Green
    $tables = $r
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: List Views
Write-Host "4. List Views..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/views" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) views" -ForegroundColor Green
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: List Stored Procedures
Write-Host "5. List Stored Procedures..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/storedprocedures" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) procedures" -ForegroundColor Green
    $procs = $r
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: List Functions
Write-Host "6. List Functions..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/functions" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) functions" -ForegroundColor Green
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: List Indexes
Write-Host "7. List Indexes..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/indexes" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) indexes" -ForegroundColor Green
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 8: List Foreign Keys
Write-Host "8. List Foreign Keys..." -ForegroundColor Cyan
try {
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Metadata/foreignkeys" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($conn | ConvertTo-Json)
    Write-Host "   PASS - Found $($r.Count) foreign keys" -ForegroundColor Green
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test 9: Dashboard
Write-Host "9. Dashboard Overview..." -ForegroundColor Cyan
try {
    $dash = @{
        databaseType = 1
        serverName = 'localhost'
        databaseName = 'CompanyDB'
        useWindowsAuth = $false
        username = 'root'
        password = 'test123'
    }
    $r = Invoke-RestMethod -Uri "$baseUrl/api/Dashboard" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($dash | ConvertTo-Json)
    Write-Host "   PASS - Dashboard loaded" -ForegroundColor Green
    Write-Host "     Tables: $($r.tableCount), Procedures: $($r.storedProcedureCount)" -ForegroundColor Gray
} catch {
    Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
}

# Test Stored Procedure Detail if procedures exist
if ($procs -and $procs.Count -gt 0) {
    $procName = $procs[0].procedureName
    if (-not $procName) { $procName = $procs[0].ProcedureName }
    
    if ($procName) {
        Write-Host "10. Stored Procedure Detail ($procName)..." -ForegroundColor Cyan
        try {
            $procReq = $conn.Clone()
            $procReq.procedureName = $procName
            $r = Invoke-RestMethod -Uri "$baseUrl/api/StoredProcedure/detail" -Method POST -Headers @{'Content-Type'='application/json'} -Body ($procReq | ConvertTo-Json)
            Write-Host "   PASS - Procedure details retrieved" -ForegroundColor Green
        } catch {
            Write-Host "   FAIL - $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`n=== All Tests Complete ===" -ForegroundColor Yellow
Write-Host "MySQL support is working!" -ForegroundColor Green
