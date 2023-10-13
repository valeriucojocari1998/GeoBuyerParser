using GeoBuyerParser.Managers;
using GeoBuyerParser.Parsers;
using GeoBuyerParser.Repositories;
using GeoBuyerParser.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class ApiController : ControllerBase
{
    public BiedronkaService _biedronkaService;
    public KauflandService _kauflandService;
    public LidlService _lidlService;
    public SparService _sparService;
    public GazetkiService _gazetkiService;
    public Repository _repository;

    public ApiController(BiedronkaService biedronkaService, KauflandService kauflandService, LidlService lidlService, SparService sparService, GazetkiService gazetkiService, Repository repository) {
        _biedronkaService = biedronkaService;
        _kauflandService = kauflandService;
        _lidlService = lidlService;
        _sparService = sparService;
        _gazetkiService = gazetkiService;
        _repository = repository;
    }

    [HttpGet("api/ParseProducts")]
    public async Task<IActionResult> ParseProducts()
    {

        var bProducts = await _biedronkaService.GetProducts();
        var kProducts = await _kauflandService.GetProducts();
        var lProducts = await _lidlService.GetProducts();
        var sProducts = await _sparService.GetProducts();
        var gProducts = await _gazetkiService.GetProducts();
        var products = bProducts + gProducts + kProducts + lProducts + sProducts;
        return Ok(products);
    }

    [HttpGet("api/ParseProductsGazetki")]
    public async Task<IActionResult> ParseProductsGazetki()
    {
        var products = await _gazetkiService.GetProducts();
        return Ok(products);
    }

    [HttpGet("api/CleanNewspapers")]
    public IActionResult CleanNewspapers()
    {
        _gazetkiService.CleanNewspapersAddPages();
        return Ok();
    }

    [HttpGet("api/GetProducts")]
    public IActionResult GetProducts()
    {
        var products = _repository.GetProducts();
        return Ok(products);
    }

    [HttpGet("api/GetSpots")]
    public IActionResult GetSpots()
    {
        var spots = _repository.GetAllSpots();
        return Ok(spots);
    }

    [HttpGet("api/GetCategories")]
    public IActionResult GetCategories()
    {
        var categories = _repository.GetCategories();
        return Ok(categories);
    }

    [HttpGet("api/TestGetDefaultSpots")]
    public IActionResult GetTestSpots()
    {
        var testSpots = RepositoryConfig.Spots;
        return Ok(testSpots);
    }

    [HttpGet("api/TestNewMethod")]
    public async Task<IActionResult> TestNewMethod()
    {
        var html = await HtmlSourceManager.DownloadHtmlSourceCode("https://www.gazetki.pl/przejrzyj/oferty/biedronka-gazetka-1818137#page=1");
        var xxxx = _gazetkiService.GetNewspapersAndProducts(html, RepositoryConfig.Spots.FirstOrDefault(), "123");
        return Ok(xxxx);
    }
}
