using BluecoreApi.Data;
using BluecoreApi.DTOs;
using BluecoreApi.Enums;
using BluecoreApi.Models;
using BluecoreApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BluecoreApi.Services.Implementations;

public class CreditRequestService : ICreditRequestService
{
    private readonly AppDbContext _context;

    public CreditRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CreditRequest> CreateAsync(CreateCreditRequestDto dto, CancellationToken cancellationToken = default)
    {
        var creditRequest = new CreditRequest
        {
            ApplicantId = dto.ApplicantId,
            Amount = dto.Amount,
            TermMonths = dto.TermMonths,
            Status = CreditStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CreditRequests.Add(creditRequest);

        await _context.SaveChangesAsync(cancellationToken);

        return creditRequest;
    }

    public async Task<IEnumerable<CreditRequest>> GetAllAsync(string? status, CancellationToken cancellationToken = default)
    {
        var query = _context.CreditRequests
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<CreditStatus>(
                    status,
                    true,
                    out var parsedStatus) || !Enum.IsDefined(parsedStatus))
            {
                throw new ArgumentException(
                    "El estado enviado no es válido.");
            }

            query = query.Where(x => x.Status == parsedStatus);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.CreditRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<CreditRequest?> UpdateStatusAsync(
        int id,
        UpdateCreditStatusDto dto, CancellationToken cancellationToken = default)
    {
        var creditRequest =
            await _context.CreditRequests
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (creditRequest is null)
        {
            return null;
        }

        if (!Enum.TryParse<CreditStatus>(
                dto.Status,
                true,
                out var newStatus) || !Enum.IsDefined(newStatus))
        {
            throw new ArgumentException(
                "El estado enviado no es válido.");
        }

        if (newStatus == CreditStatus.Pending)
        {
            throw new ArgumentException(
                "Solo se permite aprobar o rechazar una solicitud.");
        }

        creditRequest.Status = newStatus;
        creditRequest.Comment = dto.Comment;
        creditRequest.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return creditRequest;
    }
}
