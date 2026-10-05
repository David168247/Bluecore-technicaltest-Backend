using BluecoreApi.Data;
using BluecoreApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class DatabaseMappingTests
{
    [Fact]
    public void AllApplicationTablesAndColumns_UseOnlyEsquemaCAndSnakeCase()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused").Options);
        foreach (var entity in context.Model.GetEntityTypes())
        {
            Assert.Equal("esquema_c", entity.GetSchema());
            Assert.Matches("^[a-z][a-z0-9_]*$", entity.GetTableName()!);
            var table = StoreObjectIdentifier.Table(entity.GetTableName()!, entity.GetSchema());
            foreach (var property in entity.GetProperties())
                Assert.Matches("^[a-z][a-z0-9_]*$", property.GetColumnName(table)!);
        }
        var user = context.Model.FindEntityType(typeof(UserAccount))!;
        Assert.Equal(2, user.GetIndexes().Count(index => index.IsUnique));
    }
}
