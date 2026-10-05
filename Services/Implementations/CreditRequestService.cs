using System.ComponentModel.DataAnnotations;
using BluecoreApi.Data;
using BluecoreApi.DTOs;
using BluecoreApi.Enums;
using BluecoreApi.Models;
using BluecoreApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace BluecoreApi.Services.Implementations;

public sealed class CreditRequestService(AppDbContext context) : ICreditRequestService
{
    public async Task<CreditRequest> CreateAsync(CreateCreditRequestDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var now = DateTime.UtcNow;
        var creditRequest = new CreditRequest
        {
            ApplicantId = dto.ApplicantId.Trim(),
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = CreditStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.CreditRequests.Add(creditRequest);
        await context.SaveChangesAsync(cancellationToken);
        return creditRequest;
    }

    public async Task<IEnumerable<CreditRequest>> GetAllAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = context.CreditRequests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status))
        {
            var parsedStatus = ParseStatus(status);
            query = query.Where(x => x.Status == parsedStatus);
        }
        return await query.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.CreditRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<CreditRequest?> UpdateStatusAsync(int id, UpdateCreditStatusDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var newStatus = ParseStatus(dto.Status);
        if (newStatus == CreditStatus.Pending)
            throw new ValidationException("Solo se permite aprobar o rechazar una solicitud.");

        var creditRequest = await context.CreditRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (creditRequest is null) return null;
        creditRequest.Status = newStatus;
        creditRequest.Comment = dto.Comment.Trim();
        creditRequest.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return creditRequest;
    }

    private static CreditStatus ParseStatus(string status)
    {
        var normalizedStatus = status.Trim();
        if (!Enum.TryParse<CreditStatus>(normalizedStatus, true, out var parsedStatus)
            || !Enum.IsDefined(parsedStatus)
            || !string.Equals(Enum.GetName(parsedStatus), normalizedStatus, StringComparison.OrdinalIgnoreCase))
            throw new ValidationException("El estado enviado no es válido.");
        return parsedStatus;
    }
}
