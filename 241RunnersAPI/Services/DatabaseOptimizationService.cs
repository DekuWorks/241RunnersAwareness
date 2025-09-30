using Microsoft.EntityFrameworkCore;
using _241RunnersAPI.Data;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Database optimization service for performance monitoring and optimization
    /// Provides database health checks, query optimization, and performance metrics
    /// </summary>
    public class DatabaseOptimizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseOptimizationService> _logger;
        private readonly TelemetryClient _telemetryClient;

        public DatabaseOptimizationService(ApplicationDbContext context, ILogger<DatabaseOptimizationService> logger, TelemetryClient telemetryClient)
        {
            _context = context;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        /// <summary>
        /// Get database health status
        /// </summary>
        /// <returns>Database health information</returns>
        public async Task<DatabaseHealthStatus> GetDatabaseHealthAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return new DatabaseHealthStatus
                    {
                        IsHealthy = false,
                        Status = "Unhealthy",
                        Message = "Cannot connect to database",
                        ResponseTime = stopwatch.ElapsedMilliseconds
                    };
                }

                // Test basic query performance
                var userCount = await _context.Users.CountAsync();
                var caseCount = await _context.Runners.CountAsync();
                
                stopwatch.Stop();

                var healthStatus = new DatabaseHealthStatus
                {
                    IsHealthy = true,
                    Status = "Healthy",
                    Message = "Database is responding normally",
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    UserCount = userCount,
                    CaseCount = caseCount,
                    Timestamp = DateTimeOffset.UtcNow
                };

                // Track performance metrics
                _telemetryClient.TrackMetric("Database.HealthCheck.Duration", stopwatch.ElapsedMilliseconds);
                _telemetryClient.TrackMetric("Database.HealthCheck.UserCount", userCount);
                _telemetryClient.TrackMetric("Database.HealthCheck.CaseCount", caseCount);

                return healthStatus;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database health check failed");
                
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackMetric("Database.HealthCheck.Failed", 1);

                return new DatabaseHealthStatus
                {
                    IsHealthy = false,
                    Status = "Unhealthy",
                    Message = $"Database health check failed: {ex.Message}",
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Timestamp = DateTimeOffset.UtcNow
                };
            }
        }

        /// <summary>
        /// Get database performance metrics
        /// </summary>
        /// <returns>Database performance information</returns>
        public async Task<DatabasePerformanceMetrics> GetPerformanceMetricsAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Get connection info
                var connectionString = _context.Database.GetConnectionString();
                var isConnected = await _context.Database.CanConnectAsync();
                
                // Get table statistics
                var tableStats = await GetTableStatisticsAsync();
                
                // Get query performance metrics
                var queryMetrics = await GetQueryPerformanceMetricsAsync();
                
                stopwatch.Stop();

                var metrics = new DatabasePerformanceMetrics
                {
                    IsConnected = isConnected,
                    ConnectionString = MaskConnectionString(connectionString),
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    TableStatistics = tableStats,
                    QueryMetrics = queryMetrics,
                    Timestamp = DateTimeOffset.UtcNow
                };

                // Track performance metrics
                _telemetryClient.TrackMetric("Database.Performance.Duration", stopwatch.ElapsedMilliseconds);
                _telemetryClient.TrackMetric("Database.Performance.TableCount", tableStats.Count);

                return metrics;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Failed to get database performance metrics");
                
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackMetric("Database.Performance.Failed", 1);

                return new DatabasePerformanceMetrics
                {
                    IsConnected = false,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message,
                    Timestamp = DateTimeOffset.UtcNow
                };
            }
        }

        /// <summary>
        /// Optimize database queries
        /// </summary>
        /// <returns>Optimization results</returns>
        public async Task<DatabaseOptimizationResult> OptimizeDatabaseAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var results = new List<string>();

            try
            {
                // Check for missing indexes
                var missingIndexes = await CheckMissingIndexesAsync();
                if (missingIndexes.Any())
                {
                    results.AddRange(missingIndexes);
                }

                // Check for unused indexes
                var unusedIndexes = await CheckUnusedIndexesAsync();
                if (unusedIndexes.Any())
                {
                    results.AddRange(unusedIndexes);
                }

                // Check for table fragmentation
                var fragmentationInfo = await CheckTableFragmentationAsync();
                if (fragmentationInfo.Any())
                {
                    results.AddRange(fragmentationInfo);
                }

                // Check for long-running queries
                var longRunningQueries = await CheckLongRunningQueriesAsync();
                if (longRunningQueries.Any())
                {
                    results.AddRange(longRunningQueries);
                }

                stopwatch.Stop();

                var optimizationResult = new DatabaseOptimizationResult
                {
                    Success = true,
                    Duration = stopwatch.ElapsedMilliseconds,
                    Recommendations = results,
                    Timestamp = DateTimeOffset.UtcNow
                };

                // Track optimization metrics
                _telemetryClient.TrackMetric("Database.Optimization.Duration", stopwatch.ElapsedMilliseconds);
                _telemetryClient.TrackMetric("Database.Optimization.Recommendations", results.Count);

                return optimizationResult;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Database optimization failed");
                
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackMetric("Database.Optimization.Failed", 1);

                return new DatabaseOptimizationResult
                {
                    Success = false,
                    Duration = stopwatch.ElapsedMilliseconds,
                    Error = ex.Message,
                    Timestamp = DateTimeOffset.UtcNow
                };
            }
        }

        private async Task<List<TableStatistics>> GetTableStatisticsAsync()
        {
            var statistics = new List<TableStatistics>();

            try
            {
                // Get Users table stats
                var userCount = await _context.Users.CountAsync();
                statistics.Add(new TableStatistics { TableName = "Users", RowCount = userCount });

                // Get Runners table stats
                var runnerCount = await _context.Runners.CountAsync();
                statistics.Add(new TableStatistics { TableName = "Runners", RowCount = runnerCount });

                // Get Cases table stats
                var caseCount = await _context.Cases.CountAsync();
                statistics.Add(new TableStatistics { TableName = "Cases", RowCount = caseCount });

                // Get Alerts table stats
                var alertCount = await _context.Alerts.CountAsync();
                statistics.Add(new TableStatistics { TableName = "Alerts", RowCount = alertCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get table statistics");
            }

            return statistics;
        }

        private async Task<List<QueryPerformanceMetric>> GetQueryPerformanceMetricsAsync()
        {
            var metrics = new List<QueryPerformanceMetric>();

            try
            {
                // Test common query performance
                var stopwatch = Stopwatch.StartNew();
                
                // Test user query
                stopwatch.Restart();
                var users = await _context.Users.Take(10).ToListAsync();
                metrics.Add(new QueryPerformanceMetric 
                { 
                    QueryName = "GetUsers", 
                    Duration = stopwatch.ElapsedMilliseconds,
                    RowCount = users.Count
                });

                // Test runner query
                stopwatch.Restart();
                var runners = await _context.Runners.Take(10).ToListAsync();
                metrics.Add(new QueryPerformanceMetric 
                { 
                    QueryName = "GetRunners", 
                    Duration = stopwatch.ElapsedMilliseconds,
                    RowCount = runners.Count
                });

                // Test case query
                stopwatch.Restart();
                var cases = await _context.Cases.Take(10).ToListAsync();
                metrics.Add(new QueryPerformanceMetric 
                { 
                    QueryName = "GetCases", 
                    Duration = stopwatch.ElapsedMilliseconds,
                    RowCount = cases.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get query performance metrics");
            }

            return metrics;
        }

        private async Task<List<string>> CheckMissingIndexesAsync()
        {
            var recommendations = new List<string>();

            try
            {
                // Check for common missing indexes
                var userEmailCount = await _context.Users.CountAsync(u => u.Email != null);
                if (userEmailCount > 100)
                {
                    recommendations.Add("Consider adding index on Users.Email for better query performance");
                }

                var runnerStatusCount = await _context.Runners.CountAsync(r => r.Status != null);
                if (runnerStatusCount > 100)
                {
                    recommendations.Add("Consider adding index on Runners.Status for better filtering performance");
                }

                var caseDateCount = await _context.Cases.CountAsync(c => c.DateReported != null);
                if (caseDateCount > 100)
                {
                    recommendations.Add("Consider adding index on Cases.DateReported for better date range queries");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for missing indexes");
            }

            return recommendations;
        }

        private async Task<List<string>> CheckUnusedIndexesAsync()
        {
            var recommendations = new List<string>();

            try
            {
                // This would require SQL Server specific queries
                // For now, return generic recommendations
                recommendations.Add("Consider reviewing index usage statistics to identify unused indexes");
                recommendations.Add("Monitor index usage over time to identify optimization opportunities");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check for unused indexes");
            }

            return recommendations;
        }

        private async Task<List<string>> CheckTableFragmentationAsync()
        {
            var recommendations = new List<string>();

            try
            {
                // This would require SQL Server specific queries
                // For now, return generic recommendations
                recommendations.Add("Consider running DBCC SHOWCONTIG to check for table fragmentation");
                recommendations.Add("Schedule regular maintenance windows for index rebuilding if needed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check table fragmentation");
            }

            return recommendations;
        }

        private async Task<List<string>> CheckLongRunningQueriesAsync()
        {
            var recommendations = new List<string>();

            try
            {
                // Test query performance
                var stopwatch = Stopwatch.StartNew();
                await _context.Users.CountAsync();
                var duration = stopwatch.ElapsedMilliseconds;

                if (duration > 1000) // More than 1 second
                {
                    recommendations.Add($"User count query took {duration}ms - consider optimization");
                }

                stopwatch.Restart();
                await _context.Runners.CountAsync();
                duration = stopwatch.ElapsedMilliseconds;

                if (duration > 1000)
                {
                    recommendations.Add($"Runner count query took {duration}ms - consider optimization");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check long-running queries");
            }

            return recommendations;
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "Not available";

            // Mask sensitive information
            return connectionString
                .Replace(Regex.Match(connectionString, @"Password=([^;]+)").Groups[1].Value, "***")
                .Replace(Regex.Match(connectionString, @"User ID=([^;]+)").Groups[1].Value, "***");
        }
    }

    public class DatabaseHealthStatus
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public int UserCount { get; set; }
        public int CaseCount { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public class DatabasePerformanceMetrics
    {
        public bool IsConnected { get; set; }
        public string ConnectionString { get; set; } = string.Empty;
        public long ResponseTime { get; set; }
        public List<TableStatistics> TableStatistics { get; set; } = new();
        public List<QueryPerformanceMetric> QueryMetrics { get; set; } = new();
        public string? Error { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    public class TableStatistics
    {
        public string TableName { get; set; } = string.Empty;
        public int RowCount { get; set; }
    }

    public class QueryPerformanceMetric
    {
        public string QueryName { get; set; } = string.Empty;
        public long Duration { get; set; }
        public int RowCount { get; set; }
    }

    public class DatabaseOptimizationResult
    {
        public bool Success { get; set; }
        public long Duration { get; set; }
        public List<string> Recommendations { get; set; } = new();
        public string? Error { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
