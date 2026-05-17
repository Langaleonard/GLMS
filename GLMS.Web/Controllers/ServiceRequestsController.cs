using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GLMS.Web.Data;
using GLMS.Web.Models;
using GLMS.Web.Models.Enums;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CurrencyService _currencyService;

        public ServiceRequestsController(
            ApplicationDbContext context,
            CurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client);

            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        public IActionResult Create()
        {
            LoadContractsDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,ContractId,Description,CostUsd,ExchangeRate,CostZar,Status")]
            ServiceRequest serviceRequest)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("", "Selected contract does not exist.");
            }
            else if (contract.Status == ContractStatus.Expired ||
                     contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("ContractId",
                    "Service requests can only be created for Active contracts. This contract is not active.");
            }

            try
            {
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();

                serviceRequest.ExchangeRate = exchangeRate;
                serviceRequest.CostZar = serviceRequest.CostUsd * exchangeRate;

                ModelState.Remove("ExchangeRate");
                ModelState.Remove("CostZar");
            }
            catch
            {
                ModelState.AddModelError("", "Currency conversion failed. Please try again later.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(serviceRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadContractsDropdown(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            LoadContractsDropdown(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,ContractId,Description,CostUsd,ExchangeRate,CostZar,Status")]
            ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.Id)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .FirstOrDefaultAsync(c => c.Id == serviceRequest.ContractId);

            if (contract == null)
            {
                ModelState.AddModelError("", "Selected contract does not exist.");
            }
            else if (contract.Status == ContractStatus.Expired ||
                     contract.Status == ContractStatus.OnHold)
            {
                ModelState.AddModelError("ContractId",
                    "Service requests can only be linked to Active contracts.");
            }

            try
            {
                var exchangeRate = await _currencyService.GetUsdToZarRateAsync();

                serviceRequest.ExchangeRate = exchangeRate;
                serviceRequest.CostZar = serviceRequest.CostUsd * exchangeRate;

                ModelState.Remove("ExchangeRate");
                ModelState.Remove("CostZar");
            }
            catch
            {
                ModelState.AddModelError("", "Currency conversion failed. Please try again later.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceRequestExists(serviceRequest.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            LoadContractsDropdown(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);

            if (serviceRequest != null)
            {
                _context.ServiceRequests.Remove(serviceRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadContractsDropdown(int? selectedContractId = null)
        {
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .Select(c => new
                {
                    c.Id,
                    DisplayText = c.Client!.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
                })
                .ToList();

            ViewData["ContractId"] = new SelectList(
                contracts,
                "Id",
                "DisplayText",
                selectedContractId);
        }

        private bool ServiceRequestExists(int id)
        {
            return _context.ServiceRequests.Any(e => e.Id == id);
        }
    }
}