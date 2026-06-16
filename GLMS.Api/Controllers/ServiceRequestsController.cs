using GLMS.Api.Data;
using GLMS.Api.Models;
using GLMS.Api.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ServiceRequestsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetServiceRequests()
    {
        var serviceRequests = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .ThenInclude(c => c.Client)
            .ToListAsync();

        return Ok(serviceRequests);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetServiceRequest(int id)
    {
        var serviceRequest = await _context.ServiceRequests
            .Include(sr => sr.Contract)
            .ThenInclude(c => c.Client)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        return Ok(serviceRequest);
    }

    [HttpPost]
    public async Task<IActionResult> CreateServiceRequest(ServiceRequest serviceRequest)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == serviceRequest.ContractId);

        if (contract == null)
        {
            return BadRequest("Selected contract does not exist.");
        }

        if (contract.Status == ContractStatus.Expired ||
            contract.Status == ContractStatus.OnHold)
        {
            return BadRequest("Service requests can only be created for Active contracts.");
        }

        _context.ServiceRequests.Add(serviceRequest);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetServiceRequest),
            new { id = serviceRequest.Id },
            serviceRequest);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServiceRequest(int id, ServiceRequest serviceRequest)
    {
        if (id != serviceRequest.Id)
        {
            return BadRequest();
        }

        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == serviceRequest.ContractId);

        if (contract == null)
        {
            return BadRequest("Selected contract does not exist.");
        }

        if (contract.Status == ContractStatus.Expired ||
            contract.Status == ContractStatus.OnHold)
        {
            return BadRequest("Service requests can only be linked to Active contracts.");
        }

        _context.Entry(serviceRequest).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceRequest(int id)
    {
        var serviceRequest = await _context.ServiceRequests.FindAsync(id);

        if (serviceRequest == null)
        {
            return NotFound();
        }

        _context.ServiceRequests.Remove(serviceRequest);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}