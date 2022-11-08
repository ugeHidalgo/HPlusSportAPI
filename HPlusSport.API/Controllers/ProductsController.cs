using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc;

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
        public ActionResult<IEnumerable<Product>> GetAllProducts()
        {
            IEnumerable<Product> products = _context.Products.ToArray();
            return Ok(products);
        }

        [HttpGet]
        [Route("{productId}")]
        public ActionResult<Product> GetProduct(int productId)
        {
            Product? product = _context.Products.Find(productId);
            if (product != null)
            {
                return Ok(product);
            }
            return NotFound($"Product with Id {productId} not found.");
        }
    }
}
