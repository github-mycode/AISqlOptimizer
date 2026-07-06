# Quick Start Guide - SqlOptimizer API

## Prerequisites
- Docker 20.10+
- Docker Compose 2.0+
- OpenAI API Key

## 🚀 Quick Start (5 minutes)

### 1. Clone and Setup
```bash
git clone https://github.com/your-org/sqloptimizer.git
cd sqloptimizer
cp .env.example .env
```

### 2. Configure Environment
Edit `.env` and set your OpenAI API key:
```bash
OPENAI_API_KEY=sk-your-actual-openai-key-here
```

### 3. Run with Docker

**Option A - Development (with SQL Server)**
```bash
docker-compose -f docker-compose.dev.yml up -d
```

**Option B - Production (external SQL Server)**
```bash
# Edit docker-compose.yml with your SQL Server connection
docker-compose up -d
```

**Option C - Using Scripts (Windows)**
```batch
cd scripts
deploy-docker.bat dev
```

**Option D - Using Scripts (Linux/Mac)**
```bash
cd scripts
chmod +x deploy-docker.sh
./deploy-docker.sh dev
```

### 4. Verify Deployment
```bash
# Check health
curl http://localhost:5000/health

# Open Swagger UI
# Visit: http://localhost:5000
```

### 5. Test the API

Open Swagger UI at http://localhost:5000 and try:

**Test Connection:**
```json
POST /api/database/connect
{
  "serverName": "sqlserver",
  "databaseName": "master",
  "username": "sa",
  "password": "YourStrong@Password123",
  "trustServerCertificate": true
}
```

**Get Dashboard:**
```json
POST /api/dashboard
{
  "serverName": "sqlserver",
  "databaseName": "MyDatabase",
  "useWindowsAuth": false,
  "username": "sa",
  "password": "YourStrong@Password123"
}
```

## 📊 What's Included

- ✅ SqlOptimizer API (ASP.NET Core 8)
- ✅ SQL Server 2022 (Development mode)
- ✅ Swagger UI (Interactive API docs)
- ✅ Health Checks
- ✅ Logging (Console + Files)
- ✅ Auto-restart on failure

## 🔍 Monitoring

### View Logs
```bash
# All services
docker-compose logs -f

# API only
docker-compose logs -f sqloptimizer-api

# SQL Server only
docker-compose logs -f sqlserver
```

### Check Status
```bash
# Container status
docker-compose ps

# Health status
docker inspect sqloptimizer-api | grep -A 10 Health
```

### View Metrics
```bash
# Resource usage
docker stats sqloptimizer-api

# Container details
docker inspect sqloptimizer-api
```

## 🛠️ Common Operations

### Restart Services
```bash
docker-compose restart
```

### Stop Services
```bash
docker-compose down
```

### Update Images
```bash
docker-compose pull
docker-compose up -d
```

### Clean Up Everything
```bash
# Stop and remove containers, volumes
docker-compose down -v

# Remove images
docker rmi sqloptimizer-api:latest
```

## 🔧 Troubleshooting

### Container Won't Start
```bash
# Check logs
docker-compose logs sqloptimizer-api

# Check events
docker events --filter container=sqloptimizer-api
```

### Health Check Failing
```bash
# Test manually
docker exec sqloptimizer-api curl http://localhost:8080/health

# Check SQL Server connection
docker exec sqloptimizer-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P 'YourStrong@Password123' -Q "SELECT 1"
```

### Port Already in Use
```bash
# Change port in docker-compose.yml
ports:
  - "5001:8080"  # Use 5001 instead of 5000
```

### OpenAI API Errors
```bash
# Check API key is set
docker exec sqloptimizer-api printenv | grep OPENAI

# Test OpenAI connection
curl -H "Authorization: Bearer $OPENAI_API_KEY" \
  https://api.openai.com/v1/models
```

## 📦 File Structure

```
sqloptimizer/
├── docker-compose.yml          # Production configuration
├── docker-compose.dev.yml      # Development configuration
├── Dockerfile                  # Multi-stage build
├── .env.example               # Environment template
├── .dockerignore              # Docker build excludes
├── scripts/
│   ├── build-docker.sh        # Build script (Linux/Mac)
│   ├── build-docker.bat       # Build script (Windows)
│   ├── deploy-docker.sh       # Deploy script (Linux/Mac)
│   └── deploy-docker.bat      # Deploy script (Windows)
├── kubernetes/
│   └── deployment.yaml        # Kubernetes manifests
└── logs/                      # Application logs (volume mount)
```

## 🌐 Endpoints

| Endpoint | Description |
|----------|-------------|
| `/` | Swagger UI |
| `/health` | Health check |
| `/api/database/connect` | Test database connection |
| `/api/dashboard` | Dashboard overview |
| `/api/metadata/*` | Database metadata |
| `/api/analysis/*` | AI-powered analysis |
| `/api/report/*` | Report generation |

## 🔐 Security Notes

⚠️ **Important for Production:**

1. **Change default passwords** in docker-compose.yml
2. **Use Docker secrets** for sensitive data
3. **Enable HTTPS/TLS** with reverse proxy
4. **Restrict network access** with firewall rules
5. **Update base images** regularly
6. **Disable Swagger** in production (set `EnableSwagger=false`)

## 🎯 Next Steps

1. **Analyze a database:**
   ```bash
   POST /api/analysis/database
   ```

2. **View dashboard:**
   ```bash
   POST /api/dashboard
   ```

3. **Generate report:**
   ```bash
   POST /api/report/pdf
   ```

## 📚 Documentation

- [Full Docker Guide](DOCKER.md)
- [API Documentation](README.md)
- [Dashboard API](DASHBOARD_API.md)
- [Logging Guide](LOGGING.md)

## 💡 Tips

- Use `docker-compose.dev.yml` for local development
- Use `docker-compose.yml` for production
- Check logs with `docker-compose logs -f`
- Monitor health with `/health` endpoint
- Access Swagger UI at http://localhost:5000

## 🆘 Support

- GitHub Issues: https://github.com/your-org/sqloptimizer/issues
- Documentation: https://github.com/your-org/sqloptimizer/wiki
- Email: support@sqloptimizer.com
