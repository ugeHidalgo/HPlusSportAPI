using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            IEnumerable<Product> products = await _context.Products.ToArrayAsync();
            return Ok(products);
        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<ActionResult<Product>> GetProduct(int productId)
        {
            Product? product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                return Ok(product);
            }
            return NotFound($"Product with Id {productId} not found.");
        }

        [HttpPost]        
        public async Task<ActionResult<Product>> CreateProduct(Product productToBeCreated)
        {
            Category? category = await _context.Categories.FindAsync(productToBeCreated.CategoryId); 
            if (category == null)
            {
                return BadRequest($"Category {productToBeCreated.CategoryId} is not a valid category.");
            }

            productToBeCreated.Category = category;
            _context.Products.Add(productToBeCreated);

            var result = await _context.SaveChangesAsync();
            return CreatedAtAction( "GetProduct", new { productId = productToBeCreated.Id}, productToBeCreated);
        }
    }
}
