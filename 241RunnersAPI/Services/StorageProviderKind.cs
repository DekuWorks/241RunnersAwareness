namespace _241RunnersAPI.Services
{
    /// <summary>
    /// Image storage backend: Supabase (default) or Azure Blob (legacy rollback).
    /// </summary>
    public static class StorageProviderKind
    {
        public const string Supabase = "Supabase";
        public const string Azure = "Azure";

        public static string Current =>
            Environment.GetEnvironmentVariable("STORAGE_PROVIDER")?.Trim() ?? Supabase;

        public static bool IsSupabase =>
            !string.Equals(Current, Azure, StringComparison.OrdinalIgnoreCase);
    }
}
