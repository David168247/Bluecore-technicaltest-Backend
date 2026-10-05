using System.ComponentModel.DataAnnotations;
using BluecoreApi.Data;
using BluecoreApi.DTOs;
using BluecoreApi.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BluecoreApi.Tests;

public sealed class CreditStatusServiceTests
{
    [Fact]
    public async Task UpdateStatusAsync_RejectsMissingComment()
    {
        using var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused").Options);
        var service = new CreditRequestService(context);

        foreach (var status in new[] { "Approved", "Rejected" })
        {
            foreach (var comment in new string?[] { null, string.Empty, " ", "\t\n" })
            {
                var request = new UpdateCreditStatusDto
                {
                    Status = status,
                    Comment = comment!
                };

                var exception = await Assert.ThrowsAsync<ValidationException>(
                    () => service.UpdateStatusAsync(1, request));

                Assert.Equal("El comentario es obligatorio.", exception.Message);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }
    }
}
