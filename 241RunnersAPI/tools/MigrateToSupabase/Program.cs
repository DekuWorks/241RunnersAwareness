using _241RunnersAPI.Data;
using _241RunnersAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace _241RunnersAPI.Tools.MigrateToSupabase;

internal static class Program
{
  private static readonly string[] SequenceTables =
  [
    "Users",
    "Runners",
    "Cases",
    "Devices",
    "TopicSubscriptions",
    "Notifications",
    "DataDeletionRequests",
    "AccountDeletionRequests"
  ];

  public static async Task<int> Main(string[] args)
  {
    var dryRun = args.Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
    var skipConfirm = args.Contains("--yes", StringComparer.OrdinalIgnoreCase);

    var config = new ConfigurationBuilder()
      .AddEnvironmentVariables()
      .AddJsonFile("appsettings.Migration.local.json", optional: true)
      .Build();

    var azureCs = config["AZURE_SQL_CONNECTION_STRING"]
      ?? config.GetConnectionString("AzureSql")
      ?? Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING");

    var supabaseCs = config["SUPABASE_CONNECTION_STRING"]
      ?? config.GetConnectionString("Supabase")
      ?? config.GetConnectionString("DefaultConnection")
      ?? Environment.GetEnvironmentVariable("SUPABASE_CONNECTION_STRING");

    if (string.IsNullOrWhiteSpace(azureCs))
    {
      Console.Error.WriteLine("Missing AZURE_SQL_CONNECTION_STRING (env or appsettings.Migration.local.json).");
      return 1;
    }

    if (string.IsNullOrWhiteSpace(supabaseCs))
    {
      Console.Error.WriteLine("Missing SUPABASE_CONNECTION_STRING (env or appsettings.Migration.local.json).");
      return 1;
    }

    await using var source = CreateContext(azureCs, useSqlServer: true);
    await using var target = CreateContext(supabaseCs, useSqlServer: false);

    Console.WriteLine("=== 241 Runners: Azure SQL → Supabase migration ===");
    Console.WriteLine();

    if (!await source.Database.CanConnectAsync())
    {
      try
      {
        await source.Database.OpenConnectionAsync();
      }
      catch (Exception ex)
      {
        Console.Error.WriteLine($"Cannot connect to Azure SQL source: {ex.Message}");
        return 1;
      }
    }

    if (!await target.Database.CanConnectAsync())
    {
      Console.Error.WriteLine("Cannot connect to Supabase target.");
      return 1;
    }

    var sourceCounts = await ReadSourceCounts(source);
    PrintCounts("Azure SQL (source)", sourceCounts);

    if (!dryRun && !skipConfirm)
    {
      Console.WriteLine();
      Console.Write("This will REPLACE all data in Supabase. Continue? [y/N] ");
      var answer = Console.ReadLine()?.Trim().ToLowerInvariant();
      if (answer is not "y" and not "yes")
      {
        Console.WriteLine("Aborted.");
        return 0;
      }
    }

    if (dryRun)
    {
      Console.WriteLine();
      Console.WriteLine("Dry run complete — no data written.");
      return 0;
    }

    Console.WriteLine();
    Console.WriteLine("Preparing Supabase schema...");
    await target.Database.EnsureCreatedAsync();
    await ClearTargetTables(target);

    Console.WriteLine("Copying data...");
    var copied = new List<(string Table, int Count)>();

    copied.Add(await CopyTable(source.Users, target.Users, target));
    copied.Add(await CopyTable(source.Runners, target.Runners, target));
    copied.Add(await CopyTable(source.Cases, target.Cases, target));
    copied.Add(await CopyTable(source.Devices, target.Devices, target));
    copied.Add(await CopyTable(source.TopicSubscriptions, target.TopicSubscriptions, target));
    copied.Add(await CopyTable(source.Notifications, target.Notifications, target));
    copied.Add(await CopyTable(source.DataDeletionRequests, target.DataDeletionRequests, target));
    copied.Add(await CopyTable(source.AccountDeletionRequests, target.AccountDeletionRequests, target));

    await ResetSequences(target);

    Console.WriteLine();
    PrintCounts("Supabase (target)", copied);
    Console.WriteLine();
    Console.WriteLine("Migration completed successfully.");
    return 0;
  }

  private static ApplicationDbContext CreateContext(string connectionString, bool useSqlServer)
  {
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    if (useSqlServer)
      optionsBuilder.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3));
    else
      optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(3));

    return new ApplicationDbContext(optionsBuilder.Options);
  }

  private static async Task<Dictionary<string, int>> ReadSourceCounts(ApplicationDbContext source)
  {
    return new Dictionary<string, int>
    {
      ["Users"] = await source.Users.CountAsync(),
      ["Runners"] = await source.Runners.CountAsync(),
      ["Cases"] = await source.Cases.CountAsync(),
      ["Devices"] = await source.Devices.CountAsync(),
      ["TopicSubscriptions"] = await source.TopicSubscriptions.CountAsync(),
      ["Notifications"] = await source.Notifications.CountAsync(),
      ["DataDeletionRequests"] = await source.DataDeletionRequests.CountAsync(),
      ["AccountDeletionRequests"] = await source.AccountDeletionRequests.CountAsync(),
    };
  }

  private static void PrintCounts(string label, IEnumerable<(string Table, int Count)> counts)
  {
    Console.WriteLine(label + ":");
    foreach (var (table, count) in counts)
      Console.WriteLine($"  {table,-26} {count,6}");
  }

  private static void PrintCounts(string label, Dictionary<string, int> counts)
  {
    PrintCounts(label, counts.Select(kv => (kv.Key, kv.Value)));
  }

  private static async Task ClearTargetTables(ApplicationDbContext target)
  {
    await target.Database.ExecuteSqlRawAsync("""
      TRUNCATE TABLE
        "AccountDeletionRequests",
        "DataDeletionRequests",
        "Notifications",
        "TopicSubscriptions",
        "Devices",
        "Cases",
        "Runners",
        "Users"
      RESTART IDENTITY CASCADE
      """);
  }

  private static async Task<(string Table, int Count)> CopyTable<T>(
    IQueryable<T> sourceQuery,
    DbSet<T> targetSet,
    ApplicationDbContext target) where T : class
  {
    var tableName = target.Model.FindEntityType(typeof(T))?.GetTableName() ?? typeof(T).Name;
    var rows = await sourceQuery.AsNoTracking().ToListAsync();
    if (rows.Count == 0)
      return (tableName, 0);

    await targetSet.AddRangeAsync(rows);
    await target.SaveChangesAsync();
    target.ChangeTracker.Clear();
    Console.WriteLine($"  {tableName}: {rows.Count} rows");
    return (tableName, rows.Count);
  }

  private static async Task ResetSequences(ApplicationDbContext target)
  {
    foreach (var table in SequenceTables)
    {
      // table names are from a fixed allow-list
      var sql = $"""
        SELECT setval(
          pg_get_serial_sequence('"{table}"', 'Id'),
          COALESCE((SELECT MAX("Id") FROM "{table}"), 1)
        )
        """;
      await target.Database.ExecuteSqlRawAsync(sql);
    }
  }
}
