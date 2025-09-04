using _241RunnersAwarenessAPI.Models;

namespace _241RunnersAwarenessAPI.Services
{
    public interface INamusDataService
    {
        /// <summary>
        /// Fetch missing persons data from NamUs API for Houston area
        /// </summary>
        Task<NamUsImportResult> FetchNamUsDataAsync(string state = "TX", string city = "Houston", int maxResults = 100);
        
        /// <summary>
        /// Import NamUs data from CSV file
        /// </summary>
        Task<NamUsImportResult> ImportFromCsvAsync(IFormFile csvFile);
        
        /// <summary>
        /// Import NamUs data from JSON data
        /// </summary>
        Task<NamUsImportResult> ImportFromJsonAsync(string jsonData);
        
        /// <summary>
        /// Sync existing cases with NamUs for updates
        /// </summary>
        Task<NamUsSyncResult> SyncExistingCasesAsync();
        
        /// <summary>
        /// Get import statistics
        /// </summary>
        Task<NamUsStats> GetImportStatsAsync();
    }

    public class NamUsImportResult
    {
        public bool Success { get; set; }
        public int Imported { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class NamUsSyncResult
    {
        public bool Success { get; set; }
        public int CasesChecked { get; set; }
        public int CasesUpdated { get; set; }
        public int CasesResolved { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class NamUsStats
    {
        public int TotalCases { get; set; }
        public int ActiveCases { get; set; }
        public int ResolvedCases { get; set; }
        public int HoustonAreaCases { get; set; }
        public DateTime LastImportDate { get; set; }
        public DateTime LastSyncDate { get; set; }
    }
} 