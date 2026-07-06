@echo off
REM SqlOptimizer API - Docker Deployment Script (Windows)

echo ========================================
echo Deploying SqlOptimizer API with Docker
echo ========================================

REM Check if .env file exists
IF NOT EXIST .env (
    echo ❌ Error: .env file not found!
    echo Creating .env from template...
    copy .env.example .env
    echo ✅ Please edit .env and set your OPENAI_API_KEY
    exit /b 1
)

REM Check if OPENAI_API_KEY is set
findstr /C:"your-openai-api-key-here" .env >nul
IF %ERRORLEVEL% EQU 0 (
    echo ⚠️  Warning: OPENAI_API_KEY is still set to placeholder value
    echo Please update .env with your actual OpenAI API key
    set /p CONTINUE=Continue anyway? (y/N): 
    IF /I NOT "%CONTINUE%"=="y" exit /b 1
)

REM Choose environment
SET ENV=%1
IF "%ENV%"=="" SET ENV=dev

IF "%ENV%"=="prod" (
    SET COMPOSE_FILE=docker-compose.yml
    echo Environment: Production
) ELSE (
    SET COMPOSE_FILE=docker-compose.dev.yml
    echo Environment: Development
)

echo.
echo Starting services with %COMPOSE_FILE%...
docker-compose -f %COMPOSE_FILE% up -d

echo.
echo Waiting for services to be healthy...
timeout /t 10 /nobreak >nul

REM Check health
echo.
echo Checking API health...
curl -f http://localhost:5000/health
IF %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ API is healthy!
) ELSE (
    echo.
    echo ⚠️  API health check failed, but services may still be starting...
)

echo.
echo.
echo ======================================
echo Deployment Summary
echo ======================================
docker-compose -f %COMPOSE_FILE% ps

echo.
echo Access points:
echo   Swagger UI:  http://localhost:5000
echo   API:         http://localhost:5000/api
echo   Health:      http://localhost:5000/health
echo.
echo View logs:
echo   docker-compose -f %COMPOSE_FILE% logs -f
echo.
echo Stop services:
echo   docker-compose -f %COMPOSE_FILE% down
