using BluecoreApi.DTOs;
using BluecoreApi.Models;

namespace BluecoreApi.Services.Interfaces
{
    public interface ICreditRequestService
    {

        Task<CreditRequest> CreateAsync(CreateCreditRequestDto dto);

        Task<IEnumerable<CreditRequest>> GetAllAsync(string? status);

        Task<CreditRequest?> GetByIdAsync(int id);

        Task<CreditRequest?> UpdateStatusAsync( int id, UpdateCreditStatusDto dto);
    }
}
