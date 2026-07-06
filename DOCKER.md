# Docker Deployment Guide for SqlOptimizer API

## Overview
This guide covers Docker deployment for the SqlOptimizer API with production-ready configuration including health checks, logging, and environment variable management.

## Prerequisites

- Docker 20.10+
- Docker Compose 2.0+
- OpenAI API Key
- 4GB+ RAM recommended
- 10GB+ disk space

## Quick Start

### 1. Clone and Configure

```bash
# Clone the repository
git clone https://github.com/your-org/sqloptimizer.git
cd sqloptimizer

# Copy environment template
cp .env.example .env

# Edit .env and set your OpenAI API key
nano .env
# Set: OPENAI_API_KEY=sk-your-actual-openai-key
```

### 2. Build and Run (Development)

```bash
# Build and start all services
docker-compose -f docker-compose.dev.yml up -d

# View logs
docker-compose -f docker-compose.dev.yml logs -f

# Check health
curl http://localhost:5000/health
```

### 3. Access the API

- **Swagger UI**: http://localhost:5000
- **API**: http://localhost:5000/api
- **Health Check**: http://localhost:5000/health

### 4. Stop Services

```bash
docker-compose -f docker-compose.dev.yml down
```

## Production Deployment

### Build Production Image

```bash
# Build the API image
docker build -t sqloptimizer-api:1.0.0 -f Dockerfile .

# Tag for registry
docker tag sqloptimizer-api:1.0.0 your-registry.com/sqloptimizer-api:1.0.0

# Push to registry
docker push your-registry.com/sqloptimizer-api:1.0.0
```

### Run Production Stack

```bash
# Start production services
docker-compose up -d

# Check status
docker-compose ps

# View logs
docker-compose logs -f sqloptimizer-api
```

## Configuration

### Environment Variables

#### Required

| Variable | Description | Example |
|----------|-------------|---------|
| `OPENAI_API_KEY` | OpenAI API key for AI analysis | `sk-proj-xxx...` |
| `Database__ConnectionString` | SQL Server connection string | `Server=sqlserver;Database=SqlOptimizerDb;...` |

#### Optional

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment |
| `Database__CommandTimeout` | `30` | SQL command timeout (seconds) |
| `OpenAI__Model` | `gpt-4` | OpenAI model to use |
| `OpenAI__MaxTokens` | `2000` | Maximum tokens per request |
| `OpenAI__Temperature` | `0.7` | AI response randomness (0.0-1.0) |
| `OpenAI__TimeoutSeconds` | `60` | OpenAI request timeout |
| `OpenAI__MaxRetryAttempts` | `3` | Number of retry attempts |
| `Serilog__MinimumLevel__Default` | `Information` | Minimum log level |

### Using Docker Secrets (Recommended for Production)

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  sqloptimizer-api:
    image: sqloptimizer-api:latest
    secrets:
      - openai_api_key
      - db_connection_string
    environment:
      - OpenAI__ApiKey=/run/secrets/openai_api_key
      - Database__ConnectionString=/run/secrets/db_connection_string

secrets:
  openai_api_key:
    external: true
  db_connection_string:
    external: true
```

Create secrets:
```bash
echo "sk-your-openai-key" | docker secret create openai_api_key -
echo "Server=..." | docker secret create db_connection_string -
```

## Docker Compose Files

### docker-compose.yml (Production)

- Production-ready configuration
- External SQL Server connection
- Information-level logging
- Health checks enabled
- No volume mounts for source code

### docker-compose.dev.yml (Development)

- Development configuration
- Includes SQL Server container
- Debug-level logging
- Source code volume mounts
- Hot reload enabled

## Health Checks

### API Health Check

The API includes a health check endpoint at `/health`:

```bash
# Check API health
curl http://localhost:5000/health

# Response (healthy):
# Healthy

# Response (unhealthy):
# Unhealthy
```

### Docker Health Checks

Health checks are configured in both Dockerfile and docker-compose:

**Dockerfile:**
```dockerfile
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1
```

**docker-compose.yml:**
```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 40s
```

### Check Container Health

```bash
# Check container health status
docker inspect sqloptimizer-api | grep -A 10 Health

# View health check logs
docker inspect sqloptimizer-api --format='{{json .State.Health}}' | jq
```

## Logging

### View Logs

```bash
# All services
docker-compose logs

# Specific service
docker-compose logs sqloptimizer-api

# Follow logs
docker-compose logs -f sqloptimizer-api

# Last 100 lines
docker-compose logs --tail=100 sqloptimizer-api

# Since timestamp
docker-compose logs --since 2026-07-04T10:00:00 sqloptimizer-api
```

### Log Files

Logs are persisted to the host filesystem:

```bash
# View log files
ls -la logs/

# View all logs
tail -f logs/log-20260704.txt

# View errors only
tail -f logs/errors-20260704.txt
```

### Log Rotation

The application uses Serilog with automatic log rotation:
- Daily rolling logs
- 10MB file size limit
- 30-day retention (general logs)
- 90-day retention (error logs)

## Volumes

### Production Volumes

```yaml
volumes:
  sqlserver-data:  # SQL Server data persistence
```

### Development Volumes

```yaml
volumes:
  sqlserver-dev-data:  # SQL Server data
  ./logs:/app/logs     # Application logs
  ./SqlOptimizer.Api:/app/src  # Source code (hot reload)
```

## Networking

### Network Configuration

```yaml
networks:
  sqloptimizer-network:
    driver: bridge
```

### Service Communication

Services communicate via service names:
- API → SQL Server: `Server=sqlserver;...`
- Health checks: `http://localhost:8080/health`

### Port Mapping

| Service | Container Port | Host Port | Protocol |
|---------|---------------|-----------|----------|
| API | 8080 | 5000 | HTTP |
| SQL Server | 1433 | 1433 | TCP |

## Database Setup

### Initialize Database

The SQL Server container automatically creates the database on first run.

To initialize schema:

```bash
# Connect to SQL Server
docker exec -it sqloptimizer-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password123'

# Run initialization script
docker exec -i sqloptimizer-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password123' \
  < SqlOptimizer.Infrastructure/Scripts/CreateTables.sql
```

### Backup Database

```bash
# Create backup
docker exec sqloptimizer-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password123' \
  -Q "BACKUP DATABASE SqlOptimizerDb TO DISK='/var/opt/mssql/backup/sqloptimizer.bak'"

# Copy backup to host
docker cp sqloptimizer-sqlserver:/var/opt/mssql/backup/sqloptimizer.bak ./backup/
```

### Restore Database

```bash
# Copy backup to container
docker cp ./backup/sqloptimizer.bak sqloptimizer-sqlserver:/var/opt/mssql/backup/

# Restore database
docker exec sqloptimizer-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password123' \
  -Q "RESTORE DATABASE SqlOptimizerDb FROM DISK='/var/opt/mssql/backup/sqloptimizer.bak' WITH REPLACE"
```

## Security Best Practices

### 1. Use Non-Root User

The Dockerfile creates a non-root user:
```dockerfile
RUN useradd -m -u 1000 appuser
USER appuser
```

### 2. Secure Secrets

**Don't:**
- Hardcode secrets in docker-compose.yml
- Commit .env files to git
- Use default passwords in production

**Do:**
- Use Docker secrets
- Use environment variables
- Use a secrets manager (Azure Key Vault, AWS Secrets Manager)
- Rotate credentials regularly

### 3. Network Isolation

```yaml
networks:
  sqloptimizer-network:
    internal: true  # No external access
  
  frontend-network:
    # Only API exposed to frontend
```

### 4. Read-Only Filesystem

```yaml
services:
  sqloptimizer-api:
    read_only: true
    tmpfs:
      - /tmp
      - /app/logs
```

### 5. Resource Limits

```yaml
services:
  sqloptimizer-api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

## Monitoring

### Container Stats

```bash
# Real-time stats
docker stats sqloptimizer-api

# One-time stats
docker stats --no-stream
```

### Health Monitoring

```bash
# Check health status
docker ps --filter name=sqloptimizer-api --format "table {{.Names}}\t{{.Status}}"

# Continuous health monitoring
watch -n 5 'docker ps --filter name=sqloptimizer --format "table {{.Names}}\t{{.Status}}"'
```

### Log Monitoring

```bash
# Watch for errors
docker-compose logs -f | grep -i error

# Count log levels
docker-compose logs | grep -oP '\[(INF|WRN|ERR|DBG)\]' | sort | uniq -c
```

## Troubleshooting

### Container Won't Start

```bash
# Check logs
docker-compose logs sqloptimizer-api

# Inspect container
docker inspect sqloptimizer-api

# Check events
docker events --filter container=sqloptimizer-api
```

### Health Check Failing

```bash
# Test health endpoint manually
docker exec sqloptimizer-api curl -f http://localhost:8080/health

# Check health check logs
docker inspect sqloptimizer-api --format='{{json .State.Health}}' | jq
```

### SQL Server Connection Issues

```bash
# Test SQL Server connectivity from API container
docker exec sqloptimizer-api nc -zv sqlserver 1433

# Test SQL Server from host
docker exec sqloptimizer-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password123' -Q "SELECT @@VERSION"
```

### Performance Issues

```bash
# Check resource usage
docker stats sqloptimizer-api

# Check container processes
docker top sqloptimizer-api

# Check disk usage
docker system df
```

## Scaling

### Horizontal Scaling

```bash
# Scale API to 3 instances
docker-compose up -d --scale sqloptimizer-api=3

# Use with load balancer (nginx, traefik, etc.)
```

### Load Balancing with Nginx

```yaml
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - sqloptimizer-api
```

## CI/CD Integration

### GitHub Actions

```yaml
name: Build and Push Docker Image

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Build Docker image
        run: docker build -t sqloptimizer-api:${{ github.sha }} .
      
      - name: Push to registry
        run: |
          echo ${{ secrets.DOCKER_PASSWORD }} | docker login -u ${{ secrets.DOCKER_USERNAME }} --password-stdin
          docker push sqloptimizer-api:${{ github.sha }}
```

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
  - task: Docker@2
    inputs:
      command: 'buildAndPush'
      repository: 'sqloptimizer-api'
      dockerfile: '**/Dockerfile'
      tags: '$(Build.BuildId)'
```

## Production Checklist

- [ ] Set strong SQL Server password
- [ ] Configure OpenAI API key
- [ ] Enable HTTPS/TLS
- [ ] Configure firewall rules
- [ ] Set up log aggregation (ELK, Splunk, etc.)
- [ ] Configure monitoring (Prometheus, Grafana)
- [ ] Set resource limits
- [ ] Enable automatic restarts
- [ ] Configure backup strategy
- [ ] Set up alerting
- [ ] Review security settings
- [ ] Test health checks
- [ ] Configure reverse proxy
- [ ] Set up SSL certificates
- [ ] Enable rate limiting
- [ ] Configure CORS properly

## Useful Commands

```bash
# Build
docker build -t sqloptimizer-api .

# Run
docker-compose up -d

# Stop
docker-compose down

# Restart
docker-compose restart

# View logs
docker-compose logs -f

# Execute command in container
docker exec -it sqloptimizer-api bash

# Clean up
docker-compose down -v  # Remove volumes
docker system prune -a  # Clean all unused resources

# Update
docker-compose pull
docker-compose up -d
```

## Support

For issues and questions:
- GitHub Issues: https://github.com/your-org/sqloptimizer/issues
- Documentation: https://github.com/your-org/sqloptimizer/wiki
- Email: support@sqloptimizer.com
