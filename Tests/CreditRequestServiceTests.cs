using System.ComponentModel.DataAnnotations;
using BluecoreApi.Data;
using BluecoreApi.DTOs;
using BluecoreApi.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BluecoreApi.Tests;

public sealed class CreditRequestServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsAmountsOutsideAllowedRange()
    {
        using var context = CreateContext();
        var service = new CreditRequestService(context);

        foreach (var amount in new[] { 499.99m, 50000.01m })
        {
            var request = new CreateCreditRequestDto
            {
                ApplicantId = "8-123-456",
                Amount = amount,
                TermMonths = 12
            };

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        foreach (var amount in new[] { 500m, 50000m })
        {
            var request = new CreateCreditRequestDto
            {
                ApplicantId = "8-123-456",
                Amount = amount,
                TermMonths = 12
            };
            Assert.True(Validator.TryValidateObject(request, new ValidationContext(request), [], true));
        }
    }

    [Fact]
    public async Task CreateAsync_RejectsTermsOutsideAllowedRange()
    {
        using var context = CreateContext();
        var service = new CreditRequestService(context);

        foreach (var termMonths in new[] { 5, 61 })
        {
            var request = new CreateCreditRequestDto
            {
                ApplicantId = "8-123-456",
                Amount = 1000m,
                TermMonths = termMonths
            };

            await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));
            Assert.Empty(context.ChangeTracker.Entries());
        }

        foreach (var termMonths in new[] { 6, 60 })
        {
            var request = new CreateCreditRequestDto
            {
                ApplicantId = "8-123-456",
                Amount = 1000m,
                TermMonths = termMonths
            };
            Assert.True(Validator.TryValidateObject(request, new ValidationContext(request), [], true));
        }
    }

    private static AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused").Options);
}
