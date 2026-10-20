using CleanArchitecture.Application.Common.Security;
using Shouldly;

namespace CleanArchitecture.Application.UnitTests.Common.Security;

public class PermissionsTests
{
    [Fact]
    public void GetAll_ReturnsAllPermissionsWithoutDuplicates()
    {
        var all = Permissions.GetAll();

        all.ShouldNotBeEmpty();
        all.Count.ShouldBe(all.Distinct().Count());
        all.ShouldContain(Permissions.Products.View);
        all.ShouldContain(Permissions.Products.Create);
        all.ShouldContain(Permissions.Products.Edit);
        all.ShouldContain(Permissions.Products.Delete);
        all.ShouldContain(Permissions.Users.View);
        all.ShouldContain(Permissions.Users.ManageRoles);
        all.ShouldContain(Permissions.Users.ManagePermissions);
        all.ShouldContain(Permissions.AuditLogs.View);
        all.ShouldContain(Permissions.AppSettings.View);
        all.ShouldContain(Permissions.AppSettings.Edit);
        all.ShouldContain(Permissions.Notifications.View);
        all.ShouldContain(Permissions.Notifications.Manage);
    }

    [Fact]
    public void GetGrouped_ReturnsAllGroupsWithCorrectPermissions()
    {
        var grouped = Permissions.GetGrouped();

        grouped.ShouldNotBeNull();
        grouped.Keys.ShouldContain("Products");
        grouped.Keys.ShouldContain("Users");
        grouped.Keys.ShouldContain("Audit Logs");
        grouped.Keys.ShouldContain("App Settings");
        grouped.Keys.ShouldContain("Notifications");

        grouped["Products"].ShouldContain(Permissions.Products.View);
        grouped["Users"].ShouldContain(Permissions.Users.ManagePermissions);
        grouped["Audit Logs"].ShouldContain(Permissions.AuditLogs.View);
    }
}
