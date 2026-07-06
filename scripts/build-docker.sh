#!/bin/bash

# SqlOptimizer API - Docker Build Script

set -e

echo "======================================"
echo "Building SqlOptimizer API Docker Image"
echo "======================================"

# Get version from argument or use default
VERSION=${1:-latest}
REGISTRY=${2:-sqloptimizer}

echo "Version: $VERSION"
echo "Registry: $REGISTRY"

# Build the image
echo ""
echo "Building Docker image..."
docker build \
  -t ${REGISTRY}/sqloptimizer-api:${VERSION} \
  -t ${REGISTRY}/sqloptimizer-api:latest \
  -f Dockerfile \
  .

echo ""
echo "✅ Build completed successfully!"
echo ""
echo "Images created:"
docker images | grep sqloptimizer-api | head -2

echo ""
echo "To run the image:"
echo "  docker-compose up -d"
echo ""
echo "To push to registry:"
echo "  docker push ${REGISTRY}/sqloptimizer-api:${VERSION}"
echo "  docker push ${REGISTRY}/sqloptimizer-api:latest"
