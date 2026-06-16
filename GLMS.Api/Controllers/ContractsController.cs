using GLMS.Api.Data;
using GLMS.Api.Models;
using GLMS.Api.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContractsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetContracts(
        DateTime? startDate,
        DateTime? endDate,
        ContractStatus? status)
    {
        var contracts = _context.Contracts
            .Include(c => c.Client)
            .AsQueryable();

        if (startDate.HasValue)
        {
            contracts = contracts.Where(c => c.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            contracts = contracts.Where(c => c.EndDate <= endDate.Value);
        }

        if (status.HasValue)
        {
            contracts = contracts.Where(c => c.Status == status.Value);
        }

        return Ok(await contracts.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetContract(int id)
    {
        var contract = await _context.Contracts
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (contract == null)
        {
            return NotFound();
        }

        return Ok(contract);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContract(Contract contract)
    {
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetContract),
            new { id = contract.Id },
            contract);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContract(int id, Contract contract)
    {
        if (id != contract.Id)
        {
            return BadRequest();
        }

        _context.Entry(contract).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateContractStatus(int id, ContractStatus status)
    {
        var contract = await _context.Contracts.FindAsync(id);

        if (contract == null)
        {
            return NotFound();
        }

        contract.Status = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(int id)
    {
        var contract = await _context.Contracts.FindAsync(id);

        if (contract == null)
        {
            return NotFound();
        }

        _context.Contracts.Remove(contract);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}