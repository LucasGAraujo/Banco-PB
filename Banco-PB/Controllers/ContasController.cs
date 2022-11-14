using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Banco_PB.Data;
using Banco_PB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using System.Text;

namespace Banco_PB.Controllers
{
    [Authorize]
    public class ContasController : Controller
    {
        private readonly string URL = "https://localhost:7051/api/contasapi";
        private readonly ApplicationDbContext _context;

        public ContasController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        // GET: Contas
        public async Task<IActionResult> Index()
        {
              List<Conta> conta = new List<Conta>();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(URL))
                {
                    string apiReponse = await response.Content.ReadAsStringAsync();
                    conta = JsonConvert.DeserializeObject<List<Conta>>(apiReponse);
                }
            }
            return View(conta);
        }

        // GET: Contas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            Conta conta = new Conta();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(URL + "/" + id))
                {
                    string apiReponse = await response.Content.ReadAsStringAsync();
                    conta = JsonConvert.DeserializeObject<Conta>(apiReponse);
                }
            }
            return View(conta);
        }
        
        // GET: Contas/Create
        public IActionResult Create()
        {
            
            return View();
        }
        [Authorize(Roles = "Administrador")]

        // POST: Contas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conta conta, IFormFile Foto)
        {
            if (ModelState.IsValid)
            {
                conta.Foto = UploadImage(Foto);
                using (var httpClient = new HttpClient())
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(conta),
                        Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(URL, content))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        conta = JsonConvert.DeserializeObject<Conta>(apiResponse);
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(conta);
        }

        private static string UploadImage(IFormFile imageFile)
        {
            string connectionString = @"DefaultEndpointsProtocol=https;AccountName=xxxresourcer;AccountKey=19LwhIgh1nJiU6BTH6idmZ4W285jkSS3xkowQ3D1vPOQjXe8kWE6ULNG6vrBFgQt+wzqf1f0mAYG+AStBR1/Zw==;EndpointSuffix=core.windows.net";
            string containerName = "imagem";
            var reader = imageFile.OpenReadStream();
            var cloundStorageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = cloundStorageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            container.CreateIfNotExistsAsync();
            CloudBlockBlob blob = container.GetBlockBlobReference(imageFile.FileName);
            Thread.Sleep(1000);
            blob.UploadFromStreamAsync(reader);
            return blob.Uri.ToString();

        }

        // GET: Contas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Conta == null)
            {
                return NotFound();
            }

            var conta = await _context.Conta.FindAsync(id);
            if (conta == null)
            {
                return NotFound();
            }
            return View(conta);
        }

        // POST: Contas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Conta conta, IFormFile Foto)
        {
            if (id != conta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    conta.Foto = UploadImage(Foto);
                    _context.Update(conta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContaExists(conta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(conta);
        }

        // GET: Contas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            Conta conta = new Conta();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(URL + "/" + id))
                {
                    string apiReponse = await response.Content.ReadAsStringAsync();
                    conta = JsonConvert.DeserializeObject<Conta>(apiReponse);
                }
            }
            return View(conta);
        }
    
        [Authorize(Roles = "Administrador")]
        // POST: Contas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Conta conta)
        {
            if (conta != null)
            {
                DeleteFile(conta.Foto);
                using (var httpCliente = new HttpClient())
                {
                    using (var response = await httpCliente.DeleteAsync(URL + "/" + conta.Id))
                    {
                        await  response.Content.ReadAsStringAsync();
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }
        private static void DeleteFile(string foto)
        {
            string connectionString = @"DefaultEndpointsProtocol=https;AccountName=xxxresourcer;AccountKey=19LwhIgh1nJiU6BTH6idmZ4W285jkSS3xkowQ3D1vPOQjXe8kWE6ULNG6vrBFgQt+wzqf1f0mAYG+AStBR1/Zw==;EndpointSuffix=core.windows.net";
            string containerName = "imagem";
            var blobServiceClient = new BlobServiceClient(connectionString);
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);
            string arquivo = foto.Substring(foto.LastIndexOf('/') + 1);
            var blobClient = blobContainerClient.GetBlobClient(arquivo);
            blobClient.DeleteIfExists();
        }
        private bool ContaExists(int id)
        {
          return _context.Conta.Any(e => e.Id == id);
        }
    }
}
