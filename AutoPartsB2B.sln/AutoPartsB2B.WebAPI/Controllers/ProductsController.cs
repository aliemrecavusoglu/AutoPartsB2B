using AutoPartsB2B.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoPartsB2B.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            //Sadece stoğu 0'dan büyük olan ürünleri listeleyelim
            var products = await _context.Products
                                         .Where(p => p.StockQuantity > 0)
                                         .ToListAsync();
            return Ok(products);
        }
    }
}