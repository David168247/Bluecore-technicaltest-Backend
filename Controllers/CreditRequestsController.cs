using BluecoreApi.DTOs;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace BluecoreApi.Controllers;

[ApiController]
[Route("api/credit-requests")]
[Authorize]
public sealed class CreditRequestsController(ICreditRequestService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateCreditRequestDto dto, CancellationToken cancellationToken)
    {
        var creditRequest = await service.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = creditRequest.Id }, creditRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(status, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var creditRequest = await service.GetByIdAsync(id, cancellationToken);
        return creditRequest is null ? NotFound() : Ok(creditRequest);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateCreditStatusDto dto, CancellationToken cancellationToken)
    {
        var creditRequest = await service.UpdateStatusAsync(id, dto, cancellationToken);
        return creditRequest is null ? NotFound() : Ok(creditRequest);
    }
}
