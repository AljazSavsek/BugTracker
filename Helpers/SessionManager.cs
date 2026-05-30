namespace BugTracker.Helpers
{
    public static class SessionManager
    {
        public static int    UserId   { get; set; }
        public static string Username { get; set; }
        public static string Vloga    { get; set; }
        public static bool   IsAdmin  => Vloga == "Admin";

        public static void Clear()
        {
            UserId   = 0;
            Username = null;
            Vloga    = null;
        }
    }
}
