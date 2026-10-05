using System.ComponentModel.DataAnnotations;
using BluecoreApi.Data;
using BluecoreApi.DTOs;
using BluecoreApi.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace BluecoreApi.Tests;

public sealed class CreditRequestServiceValidationTests
{
    private static AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql("Host=localhost;Database=unused;Username=unused").Options);

    [Theory]
    [InlineData("999")]
    [InlineData("-1")]
    [InlineData("not-a-status")]
    public async Task List_RejectsUndefinedStatusBeforeDatabaseAccess(string status)
    {
        using var context = Context();
        await Assert.ThrowsAsync<ValidationException>(() => new CreditRequestService(context).GetAllAsync(status));
    }

    [Theory]
    [InlineData("999", "comment")]
    [InlineData("Pending", "comment")]
    [InlineData("Approved", " ")]
    public async Task Update_RejectsInvalidStatusAndBlankCommentBeforeDatabaseAccess(string status, string comment)
    {
        using var context = Context();
        await Assert.ThrowsAsync<ValidationException>(() => new CreditRequestService(context).UpdateStatusAsync(
            1, new() { Status = status, Comment = comment }));
    }

    [Fact]
    public async Task Create_RejectsInvalidDataBeforeDatabaseAccess()
    {
        using var context = Context();
        await Assert.ThrowsAsync<ValidationException>(() => new CreditRequestService(context).CreateAsync(new CreateCreditRequestDto()));
        Assert.Empty(context.ChangeTracker.Entries());
    }
}
