using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;
using OrdersAPI.Models;
using System.Net.Http.Json;

namespace OrdersAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(OrdersDbContext context, HttpClient httpClient) : ControllerBase
{
    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts() =>
        Ok(await context.Products.AsNoTracking().ToListAsync());

    [HttpGet("products/{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await context.Products.FindAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost("sync-products")]
    public async Task<ActionResult<string>> SyncProducts()
    {
        try
        {
            var products = await httpClient.GetFromJsonAsync<List<Product>>("http://localhost:5001/api/Products");
            if (products == null) return BadRequest("Impossible de récupérer les produits depuis ProductsAPI");

            foreach (var product in products)
            {
                var existing = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
                if (existing is null)
                {
                    context.Products.Add(product);
                }
                else
                {
                    existing.Name = product.Name;
                    existing.Price = product.Price;
                    existing.Stock = product.Stock;
                    existing.Description = product.Description;
                }
            }

            await context.SaveChangesAsync();
            return Ok($"Synchronisation réussie : {products.Count} produit(s) synchronisé(s)");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erreur de synchronisation : {ex.Message}");
        }
    }
}
