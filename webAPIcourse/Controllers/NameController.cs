using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NameController : ControllerBase
{
    private readonly NameService _nameService;

    public NameController(NameService nameService)
    {
        _nameService = nameService;
    }

    [HttpGet("RndName")]
    public async Task<IActionResult> GetRandomName(int? count = 1)
    {
        var result = await _nameService.GetRandomNameAsync(count.Value);
        return Content(result, "text/html");
    }
}