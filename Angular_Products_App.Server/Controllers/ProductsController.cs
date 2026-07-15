using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyApp.Dtos;
using MyApp.Services;

namespace MyApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService productSvc;

        public ProductsController(IProductService svc)
        {
            productSvc = svc;
        }

        // Supports pagination, filtering, and onlyInactive flag:
        // GET /api/products?pageNumber=1&pageSize=20&nameFilter=foo&includeInactive=false&onlyInactive=false
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? nameFilter = null, [FromQuery] bool includeInactive = false, [FromQuery] bool onlyInactive = false)
        {
            // If pageSize is 0, return all (no paging)
            if (pageSize == 0)
            {
                var all = await productSvc.GetAllAsync();
                return Ok(all);
            }

            var paged = await productSvc.GetPagedAsync(pageNumber, pageSize, nameFilter, includeInactive, onlyInactive);
            return Ok(paged);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var item = await productSvc.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await productSvc.CreateAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var ok = await productSvc.UpdateAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ok = await productSvc.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}