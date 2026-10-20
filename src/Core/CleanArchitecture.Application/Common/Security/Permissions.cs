namespace CleanArchitecture.Application.Common.Security;

/// <summary>
/// Konstanta hak akses (Permissions) granular di dalam sistem
/// </summary>
public static class Permissions
{
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string ManageRoles = "Permissions.Users.ManageRoles";
        public const string ManagePermissions = "Permissions.Users.ManagePermissions";
    }

    public static class AuditLogs
    {
        public const string View = "Permissions.AuditLogs.View";
    }

    public static class AppSettings
    {
        public const string View = "Permissions.AppSettings.View";
        public const string Edit = "Permissions.AppSettings.Edit";
    }

    public static class Notifications
    {
        public const string View = "Permissions.Notifications.View";
        public const string Manage = "Permissions.Notifications.Manage";
    }

    /// <summary>
    /// Mengembalikan seluruh permission yang terdaftar dalam sistem
    /// </summary>
    public static IReadOnlyList<string> GetAll()
    {
        return new List<string>
        {
            Products.View,
            Products.Create,
            Products.Edit,
            Products.Delete,
            Users.View,
            Users.ManageRoles,
            Users.ManagePermissions,
            AuditLogs.View,
            AppSettings.View,
            AppSettings.Edit,
            Notifications.View,
            Notifications.Manage
        };
    }

    /// <summary>
    /// Mengembalikan seluruh permission yang dikelompokkan berdasarkan kategori / modul
    /// </summary>
    public static IReadOnlyDictionary<string, List<string>> GetGrouped()
    {
        return new Dictionary<string, List<string>>
        {
            ["Products"] = new() { Products.View, Products.Create, Products.Edit, Products.Delete },
            ["Users"] = new() { Users.View, Users.ManageRoles, Users.ManagePermissions },
            ["Audit Logs"] = new() { AuditLogs.View },
            ["App Settings"] = new() { AppSettings.View, AppSettings.Edit },
            ["Notifications"] = new() { Notifications.View, Notifications.Manage }
        };
    }
}
