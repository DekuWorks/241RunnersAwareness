#!/bin/bash
# 241 Runners Awareness - Build Script for Hashed Assets

set -e

echo "🔨 Building 241 Runners Awareness..."

# Get current commit hash
COMMIT_HASH=$(git rev-parse --short HEAD 2>/dev/null || echo "unknown")
BUILD_TIME=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

echo "📝 Updating version.json..."
cat > version.json << VERSION_EOF
{
  "version": "1.0.0",
  "commit": "$COMMIT_HASH",
  "builtAt": "$BUILD_TIME",
  "environment": "production"
}
VERSION_EOF

echo "✅ Build completed successfully!"
echo "   Commit: $COMMIT_HASH"
echo "   Built: $BUILD_TIME"
