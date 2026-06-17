using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GLMS.Web.Models;
using GLMS.Web.Services;

namespace GLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly HttpClient _apiClient;
        private readonly CurrencyService _currencyService;

        public ServiceRequestsController(
            IHttpClientFactory httpClientFactory,
            CurrencyService currencyService)
        {
            _apiClient = httpClientFactory.CreateClient("GLMSApi");
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index()
        {
            var serviceRequests =
                await _apiClient.GetFromJsonAsync<List<ServiceRequest>>("api/ServiceRequests");

            return View(serviceRequests ?? new List<ServiceRequest>());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/ServiceRequests/{id}");

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        public async Task<IActionResult> Create()
        {
            await LoadContractsDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,ContractId,Description,CostUsd,ExchangeRate,CostZar,Status")]
            ServiceRequest serviceRequest)
        {
            await ApplyCurrencyConversionAsync(serviceRequest);

            if (!ModelState.IsValid)
            {
                await LoadContractsDropdownAsync(serviceRequest.ContractId);
                return View(serviceRequest);
            }

            var response =
                await _apiClient.PostAsJsonAsync("api/ServiceRequests", serviceRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to create service request through the API."
                    : errorMessage);

            await LoadContractsDropdownAsync(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/ServiceRequests/{id}");

            if (serviceRequest == null)
            {
                return NotFound();
            }

            await LoadContractsDropdownAsync(serviceRequest.ContractId);
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

            await ApplyCurrencyConversionAsync(serviceRequest);

            if (!ModelState.IsValid)
            {
                await LoadContractsDropdownAsync(serviceRequest.ContractId);
                return View(serviceRequest);
            }

            var response =
                await _apiClient.PutAsJsonAsync($"api/ServiceRequests/{id}", serviceRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var errorMessage = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                "",
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Unable to update service request through the API."
                    : errorMessage);

            await LoadContractsDropdownAsync(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/ServiceRequests/{id}");

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
            var response =
                await _apiClient.DeleteAsync($"api/ServiceRequests/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to delete service request through the API.");

            var serviceRequest =
                await _apiClient.GetFromJsonAsync<ServiceRequest>($"api/ServiceRequests/{id}");

            return View("Delete", serviceRequest);
        }

        private async Task ApplyCurrencyConversionAsync(ServiceRequest serviceRequest)
        {
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
        }

        private async Task LoadContractsDropdownAsync(int? selectedContractId = null)
        {
            var contracts =
                await _apiClient.GetFromJsonAsync<List<Contract>>("api/Contracts");

            var contractItems = (contracts ?? new List<Contract>())
                .Select(c => new
                {
                    c.Id,
                    DisplayText = c.Client != null
                        ? c.Client.Name + " - " + c.ServiceLevel + " (" + c.Status + ")"
                        : c.ServiceLevel + " (" + c.Status + ")"
                })
                .ToList();

            ViewData["ContractId"] = new SelectList(
                contractItems,
                "Id",
                "DisplayText",
                selectedContractId);
        }
    }
}