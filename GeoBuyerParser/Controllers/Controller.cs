﻿using GeoBuyerParser.Repositories;
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
        var products = bProducts.Concat(gProducts).Concat(kProducts).Concat(lProducts).Concat(sProducts).ToList();
        return Ok(products);
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
}