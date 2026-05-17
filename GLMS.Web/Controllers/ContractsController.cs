using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.Enums;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContractsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Contracts
        public async Task<IActionResult> Index(
            DateTime? startDate,
            DateTime? endDate,
            ContractStatus? status)
        {
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (startDate.HasValue)
            {
                contracts = contracts.Where(c =>
                    c.StartDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                contracts = contracts.Where(c =>
                    c.EndDate <= endDate.Value);
            }

            if (status.HasValue)
            {
                contracts = contracts.Where(c =>
                    c.Status == status.Value);
            }

            ViewData["StartDate"] =
                startDate?.ToString("yyyy-MM-dd");

            ViewData["EndDate"] =
                endDate?.ToString("yyyy-MM-dd");

            ViewData["Status"] = status;

            return View(await contracts.ToListAsync());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] =
                new SelectList(_context.Clients, "Id", "Name");

            return View();
        }

        // POST: Contracts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath,SignedAgreementFile")]
            Contract contract)
        {
            if (contract.SignedAgreementFile != null)
            {
                var extension =
                    Path.GetExtension(contract.SignedAgreementFile.FileName);

                if (extension.ToLower() != ".pdf")
                {
                    ModelState.AddModelError(
                        "SignedAgreementFile",
                        "Only PDF files are allowed.");

                    ViewData["ClientId"] =
                        new SelectList(
                            _context.Clients,
                            "Id",
                            "Name",
                            contract.ClientId);

                    return View(contract);
                }

                var fileName = Guid.NewGuid() + extension;

                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/contracts");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath =
                    Path.Combine(uploadPath, fileName);

                using (var stream =
                       new FileStream(filePath, FileMode.Create))
                {
                    await contract.SignedAgreementFile
                        .CopyToAsync(stream);
                }

                contract.SignedAgreementPath =
                    "/uploads/contracts/" + fileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(contract);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] =
                new SelectList(
                    _context.Clients,
                    "Id",
                    "Name",
                    contract.ClientId);

            return View(contract);
        }

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract =
                await _context.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            ViewData["ClientId"] =
                new SelectList(
                    _context.Clients,
                    "Id",
                    "Name",
                    contract.ClientId);

            return View(contract);
        }

        // POST: Contracts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath,SignedAgreementFile")]
            Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (contract.SignedAgreementFile != null)
            {
                var extension =
                    Path.GetExtension(contract.SignedAgreementFile.FileName);

                if (extension.ToLower() != ".pdf")
                {
                    ModelState.AddModelError(
                        "SignedAgreementFile",
                        "Only PDF files are allowed.");

                    ViewData["ClientId"] =
                        new SelectList(
                            _context.Clients,
                            "Id",
                            "Name",
                            contract.ClientId);

                    return View(contract);
                }

                var fileName = Guid.NewGuid() + extension;

                var uploadPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads/contracts");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var filePath =
                    Path.Combine(uploadPath, fileName);

                using (var stream =
                       new FileStream(filePath, FileMode.Create))
                {
                    await contract.SignedAgreementFile
                        .CopyToAsync(stream);
                }

                contract.SignedAgreementPath =
                    "/uploads/contracts/" + fileName;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] =
                new SelectList(
                    _context.Clients,
                    "Id",
                    "Name",
                    contract.ClientId);

            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract =
                await _context.Contracts.FindAsync(id);

            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
    }
}