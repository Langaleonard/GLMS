using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using GLMS.Web.Models;

namespace GLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly HttpClient _apiClient;

        public ClientsController(IHttpClientFactory httpClientFactory)
        {
            _apiClient = httpClientFactory.CreateClient("GLMSApi");
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _apiClient.GetFromJsonAsync<List<Client>>("api/Clients");

            return View(clients ?? new List<Client>());
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/Clients/{id}");

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ContactDetails,Region")] Client client)
        {
            if (!ModelState.IsValid)
            {
                return View(client);
            }

            var response = await _apiClient.PostAsJsonAsync("api/Clients", client);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to create client through the API.");
            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/Clients/{id}");

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ContactDetails,Region")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            var response = await _apiClient.PutAsJsonAsync($"api/Clients/{id}", client);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to update client through the API.");
            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/Clients/{id}");

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var response = await _apiClient.DeleteAsync($"api/Clients/{id}");

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Unable to delete client through the API.");

            var client = await _apiClient.GetFromJsonAsync<Client>($"api/Clients/{id}");
            return View("Delete", client);
        }
    }
} 
