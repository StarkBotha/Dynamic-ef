using DynamicEFprototype.Data;
using Microsoft.AspNetCore.Mvc;

namespace DynamicEFprototype.Controllers;

[ApiController]
[Route("[controller]")]
public class FurnitureController : ControllerBase
{
    private ProtoContext _context;

    public FurnitureController(ProtoContext context)
    {
        _context = context;
    }

    // GET
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var products = _context.Products;

        return Ok(products);
    }

    [HttpGet("dynamic-search")]
    public async Task<IActionResult> DynamicSearch(List<DynamicSearchRequest> request)
    {
        var dbservice = new DataService(_context);

        var results = await dbservice.ProductDynamicSearchAsync(request);

        return Ok(results);
    }
}