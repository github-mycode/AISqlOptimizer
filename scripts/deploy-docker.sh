#!/bin/bash

# SqlOptimizer API - Docker Deployment Script

set -e

echo "========================================"
echo "Deploying SqlOptimizer API with Docker"
echo "========================================"

# Check if .env file exists
if [ ! -f .env ]; then
    echo "❌ Error: .env file not found!"
    echo "Creating .env from template..."
    cp .env.example .env
    echo "✅ Please edit .env and set your OPENAI_API_KEY"
    exit 1
fi

# Check if OPENAI_API_KEY is set
if grep -q "your-openai-api-key-here" .env; then
    echo "⚠️  Warning: OPENAI_API_KEY is still set to placeholder value"
    echo "Please update .env with your actual OpenAI API key"
    read -p "Continue anyway? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

# Choose environment
ENV=${1:-dev}

if [ "$ENV" = "prod" ]; then
    COMPOSE_FILE="docker-compose.yml"
    echo "Environment: Production"
else
    COMPOSE_FILE="docker-compose.dev.yml"
    echo "Environment: Development"
fi

echo ""
echo "Starting services with $COMPOSE_FILE..."
docker-compose -f $COMPOSE_FILE up -d

echo ""
echo "Waiting for services to be healthy..."
sleep 5

# Check health
echo ""
echo "Checking API health..."
for i in {1..30}; do
    if curl -f http://localhost:5000/health 2>/dev/null; then
        echo ""
        echo "✅ API is healthy!"
        break
    fi
    echo -n "."
    sleep 2
done

echo ""
echo ""
echo "======================================"
echo "Deployment Summary"
echo "======================================"
docker-compose -f $COMPOSE_FILE ps

echo ""
echo "Access points:"
echo "  Swagger UI:  http://localhost:5000"
echo "  API:         http://localhost:5000/api"
echo "  Health:      http://localhost:5000/health"
echo ""
echo "View logs:"
echo "  docker-compose -f $COMPOSE_FILE logs -f"
echo ""
echo "Stop services:"
echo "  docker-compose -f $COMPOSE_FILE down"
