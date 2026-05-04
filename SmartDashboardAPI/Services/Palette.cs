namespace SmartDashboardAPI.Services
{
    public static class Palette
    {
        public static readonly string[] Colors =
 [
     "#3b82f6","#10b981","#f59e0b","#8b5cf6",
        "#06b6d4","#ec4899","#34d399","#fbbf24",
        "#f87171","#a78bfa"
 ];
        public static string Get(int i) => Colors[i % Colors.Length];
    }
}
