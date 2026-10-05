using BluecoreApi.DTOs;
using BluecoreApi.Models;
namespace BluecoreApi.Services.Interfaces;

public interface ICreditRequestService
{
    Task<CreditRequest> CreateAsync(CreateCreditRequestDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditRequest>> GetAllAsync(string? status, CancellationToken cancellationToken = default);
    Task<CreditRequest?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CreditRequest?> UpdateStatusAsync(int id, UpdateCreditStatusDto dto, CancellationToken cancellationToken = default);
}
