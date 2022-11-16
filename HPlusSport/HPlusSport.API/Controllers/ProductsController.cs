using HPlusSport.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HPlusSport.API.Controllers
{
    [ApiVersion("1.0")]
    //[Route("api/[controller]")]
    [Route("products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ShopContext _context;

        public ProductsController(ShopContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        #region Public methods

        //Use cases:
        //  api/Products
        //  api/Products?size=10
        //  api/products?size=25&page=2
        //  api/products?size=25&page=2&MinPrice=5
        //  api/products?size=25&page=2&MinPrice=5&MaxPrice=10
        //  api/products?MinPrice=5&MaxPrice=10
        //  api/products?MinPrice=5
        //  api/products?Name=jeans
        //  api/products?SortBy=Price&sortOrder=desc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts([FromQuery]ProductQueryParameters queryParameters)
        {
            IQueryable<Product> products = _context.Products;

            if (queryParameters.MinPrice != null)
            {
                products = products.Where(x => x.Price >= queryParameters.MinPrice);
            }

            if (queryParameters.MaxPrice != null)
            {
                products = products.Where(x => x.Price <= queryParameters.MaxPrice);
            }

            if (!string.IsNullOrEmpty(queryParameters.Name))
            {
                products = products.Where(x => x.Name.ToLower().Contains(queryParameters.Name.ToLower()));
            }

            if (!string.IsNullOrEmpty(queryParameters.Sku))
            {
                products = products.Where(x => x.Sku == queryParameters.Sku);
            }

            if (!string.IsNullOrEmpty(queryParameters.SortBy) && typeof(Product).GetProperty(queryParameters.SortBy) != null)
            {
                products = products.OrderByCustom(queryParameters.SortBy, queryParameters.SortOrder);
            }

            products = products
                .Skip(queryParameters.Size * (queryParameters.Page - 1))
                .Take(queryParameters.Size);           
            
            return Ok(await products.ToArrayAsync());
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

            await _context.SaveChangesAsync();
            return CreatedAtAction( "GetProduct", new { productId = productToBeCreated.Id}, productToBeCreated);
        }

        [HttpPut]
        [Route("{productId}")]
        public async Task<ActionResult<Product>> UpdateProduct(int productId, [FromBody] Product productToBeUpdated)
        {
            if (productId != productToBeUpdated.Id)
            {
                return BadRequest($"ProductId: {productId} and id on the product: {productToBeUpdated.Id} are not equal.");
            }

            Category? category = await _context.Categories.FindAsync(productToBeUpdated.CategoryId);
            if (category == null)
            {
                return BadRequest($"Category {productToBeUpdated.CategoryId} is not a valid category.");
            }
            productToBeUpdated.Category = category;

            bool IsProductOnDb = _context.Products.Any(x => x.Id == productId);
            if (!IsProductOnDb)
            {
                return await CreateProduct(productToBeUpdated);
            }
            else
            {
                return await UpdateProduct(productToBeUpdated);
            }                 
        }

        [HttpDelete]
        [Route("{productId}")]
        public async Task<ActionResult<Product>> DeleteProduct(int productId)
        {
            Product? product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return new ActionResult<Product>(product);
            }
            return NotFound($"Product with Id {productId} not found.");
        }

        [HttpPost]
        [Route("Delete")] //use products/delete?productIds=1&productIds=2
        public async Task<ActionResult> DeleteProducts([FromQuery] int[] productIds)
        {
            IList<Product> products = new List<Product>();
            foreach (int productId in productIds)
            {
                Product? product = await _context.Products.FindAsync(productId);
                if (product == null)
                {                    
                    return NotFound($"Product with Id {productId} not found.");
                }
                products.Add(product);
            }
            
            _context.Products.RemoveRange(products);
            await _context.SaveChangesAsync();                
            
            return Ok(products);
        }

        #endregion

        #region Private methods

        private async Task<ActionResult<Product>> UpdateProduct(Product productToBeUpdated)
        {
            try
            {
                _context.Products.Update(productToBeUpdated);
                _context.Entry(productToBeUpdated).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return new ActionResult<Product>(productToBeUpdated);
            }
            catch (DbUpdateConcurrencyException)
            {
                bool IsProductOnDb = _context.Products.Any(x => x.Id == productToBeUpdated.Id);
                if (!IsProductOnDb)
                {
                    return NotFound("Product was removed.");
                }
                else
                {
                    throw;
                }
            }
        }

        #endregion
    }
}
