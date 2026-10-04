using BluecoreApi.DTOs;
using BluecoreApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BluecoreApi.Controllers;

[ApiController]
[Route("api/credit-requests")]
public class CreditRequestsController : ControllerBase
{
    private readonly ICreditRequestService _service;

    public CreditRequestsController(ICreditRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create( [FromBody] CreateCreditRequestDto dto )
    {
        var creditRequest = await _service.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetById),
            new { id = creditRequest.Id },
            creditRequest);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll( [FromQuery] string? status)
    {
        try
        {
            var creditRequests = await _service.GetAllAsync(status);
            return Ok(creditRequests);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var creditRequest = await _service.GetByIdAsync(id);

        if (creditRequest is null)
        {
            return NotFound(new
            {
                message = "Solicitud de crédito no encontrada."
            });
        }

        return Ok(creditRequest);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateCreditStatusDto dto)
    {
        try
        {
            var creditRequest = await _service.UpdateStatusAsync(id, dto);

            if (creditRequest is null)
            {
                return NotFound(new
                {
                    message = "Solicitud de crédito no encontrada."
                });
            }

            return Ok(creditRequest);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}