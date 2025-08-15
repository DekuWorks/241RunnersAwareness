# 241 Runners Awareness Backend Deployment Script
# Supports multiple deployment targets

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("local", "staging", "production", "docker")]
    [string]$Environment = "local",
    
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipMigrations
)

Write-Host "🚀 241 Runners Awareness Backend Deployment" -ForegroundColor Green
Write-Host "Environment: $Environment" -ForegroundColor Yellow

# Configuration
$ProjectName = "241RunnersAwareness.BackendAPI"
$SolutionFile = "241RunnersAwareness.BackendAPI.sln"
$BuildConfiguration = "Release"

# Colors for output
$SuccessColor = "Green"
$ErrorColor = "Red"
$InfoColor = "Cyan"

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $SuccessColor
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $ErrorColor
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $InfoColor
}

# Step 1: Clean and Restore
Write-Info "Step 1: Cleaning and restoring packages..."
try {
    dotnet clean $SolutionFile
    dotnet restore $SolutionFile
    Write-Success "Packages restored successfully"
} catch {
    Write-Error "Failed to restore packages: $($_.Exception.Message)"
    exit 1
}

# Step 2: Build
Write-Info "Step 2: Building solution..."
try {
    dotnet build $SolutionFile -c $BuildConfiguration --no-restore
    Write-Success "Solution built successfully"
} catch {
    Write-Error "Build failed: $($_.Exception.Message)"
    exit 1
}

# Step 3: Run Tests (if not skipped)
if (-not $SkipTests) {
    Write-Info "Step 3: Running tests..."
    try {
        dotnet test $SolutionFile -c $BuildConfiguration --no-build --verbosity normal
        Write-Success "Tests passed successfully"
    } catch {
        Write-Error "Tests failed: $($_.Exception.Message)"
        exit 1
    }
} else {
    Write-Info "Step 3: Skipping tests (--SkipTests specified)"
}

# Step 4: Publish
Write-Info "Step 4: Publishing application..."
try {
    $PublishPath = "./publish"
    if (Test-Path $PublishPath) {
        Remove-Item $PublishPath -Recurse -Force
    }
    
    dotnet publish $SolutionFile -c $BuildConfiguration -o $PublishPath --no-build
    Write-Success "Application published to $PublishPath"
} catch {
    Write-Error "Publish failed: $($_.Exception.Message)"
    exit 1
}

# Step 5: Environment-specific deployment
switch ($Environment) {
    "local" {
        Write-Info "Step 5: Starting local development server..."
        try {
            dotnet run --project $ProjectName
        } catch {
            Write-Error "Failed to start local server: $($_.Exception.Message)"
            exit 1
        }
    }
    
    "docker" {
        Write-Info "Step 5: Building and running Docker containers..."
        try {
            docker-compose up --build -d
            Write-Success "Docker containers started successfully"
            Write-Info "API available at: http://localhost:8080"
            Write-Info "Health check: http://localhost:8080/health"
        } catch {
            Write-Error "Docker deployment failed: $($_.Exception.Message)"
            exit 1
        }
    }
    
    "staging" {
        Write-Info "Step 5: Deploying to staging environment..."
        # Add staging deployment logic here
        Write-Info "Staging deployment would go here"
        Write-Info "Consider using Azure DevOps pipeline or manual deployment"
    }
    
    "production" {
        Write-Info "Step 5: Deploying to production environment..."
        # Add production deployment logic here
        Write-Info "Production deployment would go here"
        Write-Info "Consider using Azure DevOps pipeline or manual deployment"
    }
}

# Step 6: Database Migrations (if not skipped)
if (-not $SkipMigrations) {
    Write-Info "Step 6: Running database migrations..."
    try {
        dotnet ef database update --project $ProjectName
        Write-Success "Database migrations completed successfully"
    } catch {
        Write-Error "Database migrations failed: $($_.Exception.Message)"
        Write-Info "You may need to run migrations manually"
    }
} else {
    Write-Info "Step 6: Skipping database migrations (--SkipMigrations specified)"
}

Write-Success "Deployment completed successfully!"
Write-Info "Next steps:"
Write-Info "  - Test the API endpoints"
Write-Info "  - Check health endpoint: /health"
Write-Info "  - Verify database connectivity"
Write-Info "  - Monitor application logs"
