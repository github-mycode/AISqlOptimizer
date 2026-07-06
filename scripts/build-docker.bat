@echo off
REM SqlOptimizer API - Docker Build Script (Windows)

echo ======================================
echo Building SqlOptimizer API Docker Image
echo ======================================

SET VERSION=%1
IF "%VERSION%"=="" SET VERSION=latest

SET REGISTRY=%2
IF "%REGISTRY%"=="" SET REGISTRY=sqloptimizer

echo Version: %VERSION%
echo Registry: %REGISTRY%

echo.
echo Building Docker image...
docker build ^
  -t %REGISTRY%/sqloptimizer-api:%VERSION% ^
  -t %REGISTRY%/sqloptimizer-api:latest ^
  -f Dockerfile ^
  .

IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ Build failed!
    exit /b 1
)

echo.
echo ✅ Build completed successfully!
echo.
echo Images created:
docker images | findstr sqloptimizer-api

echo.
echo To run the image:
echo   docker-compose up -d
echo.
echo To push to registry:
echo   docker push %REGISTRY%/sqloptimizer-api:%VERSION%
echo   docker push %REGISTRY%/sqloptimizer-api:latest
