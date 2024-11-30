using EcoSync.Models.DTO;
using EcoSync.Services;
using EcoSync.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EcoSync.Controllers;
[ApiController]
[Route("[action]")]
public class DadosController : ControllerBase
{
    private readonly IDadosService _dadosService;
    public DadosController(IDadosService dadosService)
    {
        _dadosService = dadosService;
    }
    
    
    [HttpGet("{cep}")]
    public async Task<ActionResult<RetornoPontuacoesPorBairro>> ObterPontuacoesPorCep(int cep)
    {
        try
        {
            var resposta = await _dadosService.ObterPontuacoesPorCep(cep);
            return Ok(resposta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Erro ao obter dados do bairro");
        }
    }
}