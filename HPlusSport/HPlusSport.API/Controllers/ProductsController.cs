﻿using HPlusSport.API.Models;
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

        #region Public methods

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
