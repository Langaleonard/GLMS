using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web.Models;
using GLMS.Web.Models.Enums;

namespace GLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly HttpClient _apiClient;

        public ContractsController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("GLMSApi");
        }

        public async Task<IActionResult> Index(
            DateTime? startDate,
            DateTime? endDate,
            ContractStatus? status)
        {
            var query = "api/Contracts";

            var parameters = new List<string>();

            if (startDate.HasValue)
            {
                parameters.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            }

            if (endDate.HasValue)
            {
                parameters.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            }

            if (status.HasValue)
            {
                parameters.Add($"status={status.Value}");
            }

            if (parameters.Any())
            {
                query += "?" + string.Join("&", parameters);
            }

            var contracts = await _apiClient.GetFromJsonAsync<List<Contract>>(query);

            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["Status"] = status;

            return View(contracts ?? new List<Contract>());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/Contracts/{id}");

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            await LoadClientsDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath,SignedAgreementFile")]
            Contract contract)
        {
            if (contract.SignedAgreementFile != null)
            {
                var extension = Path.GetExtension(contract.SignedAgreementFile.FileName);

                if (extension.ToLower() != ".pdf")
                {
                    ModelState.AddModelError("SignedAgreementFile", "Only PDF files are allowed.");
                    await LoadClientsDropdownAsync(contract.ClientId);
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

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await contract.SignedAgreementFile.CopyToAsync(stream);
                }

                contract.SignedAgreementPath = "/uploads/contracts/" + fileName;
            }

            if (!ModelState.IsValid)
            {
                await LoadClientsDropdownAsync(contract.ClientId);
                return View(contract);
            }

            var response = await _apiClient.PostAsJsonAsync("api/Contracts", contract);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to create contract through the API.");
            await LoadClientsDropdownAsync(contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/Contracts/{id}");

            if (contract == null)
            {
                return NotFound();
            }

            await LoadClientsDropdownAsync(contract.ClientId);
            return View(contract);
        }

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
                var extension = Path.GetExtension(contract.SignedAgreementFile.FileName);

                if (extension.ToLower() != ".pdf")
                {
                    ModelState.AddModelError("SignedAgreementFile", "Only PDF files are allowed.");
                    await LoadClientsDropdownAsync(contract.ClientId);
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

                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await contract.SignedAgreementFile.CopyToAsync(stream);
                }

                contract.SignedAgreementPath = "/uploads/contracts/" + fileName;
            }

            if (!ModelState.IsValid)
            {
                await LoadClientsDropdownAsync(contract.ClientId);
                return View(contract);
            }

            var response = await _apiClient.PutAsJsonAsync($"api/Contracts/{id}", contract);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to update contract through the API.");
            await LoadClientsDropdownAsync(contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/Contracts/{id}");

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/Contracts/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to delete contract through the API.");

            var contract = await _apiClient.GetFromJsonAsync<Contract>($"api/Contracts/{id}");
            return View("Delete", contract);
        }

        private async Task LoadClientsDropdownAsync(int? selectedClientId = null)
        {
            var clients = await _apiClient.GetFromJsonAsync<List<Client>>("api/Clients");

            ViewData["ClientId"] = new SelectList(
                clients ?? new List<Client>(),
                "Id",
                "Name",
                selectedClientId);
        }
    }
}