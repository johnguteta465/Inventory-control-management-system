namespace InventoryManagementSystem
{
    public static class Global
    {
        public static int UserID { get; set; }
        public static string UserName { get; set; }
        public static string FullName { get; set; }
        public static string UserRole { get; set; }

        static Global()
        {
            UserName = "";
            FullName = "";
            UserRole = "";
        }
    }
}