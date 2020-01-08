using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers {
  [Route ("api/[controller]")]
  public class FornecedoresController : MainController {
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly IFornecedorService _fornecedorService;
    private readonly IMapper _mapper;
    private readonly IEnderecoRepository _enderecoRepository;

    public FornecedoresController (
      IFornecedorRepository fornecedorRepository,
      IMapper mapper,
      IFornecedorService fornecedorService,
      INotificador notificador,
      IEnderecoRepository enderecoRepository) : base (notificador) {

      _fornecedorRepository = fornecedorRepository;
      _mapper = mapper;
      _fornecedorService = fornecedorService;
      _enderecoRepository = enderecoRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FornecedorViewModel>>> ObterTodos () {
      var fornecedor = _mapper.Map<IEnumerable<FornecedorViewModel>> (await _fornecedorRepository.ObterTodos ());

      return Ok (fornecedor);
    }

    [HttpGet ("{id:guid}")]
    public async Task<ActionResult<FornecedorViewModel>> ObterPorId (Guid id) {
      var fornecedor = await ObterFornecedorProdutosEndereco (id);

      if (fornecedor == null) return NotFound ();

      return Ok (fornecedor);
    }

    [HttpPost]
    public async Task<ActionResult<FornecedorViewModel>> Adicionar ([FromBody] FornecedorViewModel fornecedorViewModel) {
      if (!ModelState.IsValid) return CustomResponse (ModelState);

      await _fornecedorService.Adicionar (_mapper.Map<Fornecedor> (fornecedorViewModel));

      return CustomResponse (fornecedorViewModel);

      // if (!result) return BadRequest ();

      // return Ok (fornecedor);
    }

    [HttpPut ("{id:guid}")]
    public async Task<ActionResult<FornecedorViewModel>> Atualizar (Guid id, [FromBody] FornecedorViewModel fornecedorViewModel) {
      if (id != fornecedorViewModel.Id) {
        NotificarErro ("O id informado não é o mesmo que foi passado na query.");
        return CustomResponse (fornecedorViewModel);
      }

      if (!ModelState.IsValid) return CustomResponse (fornecedorViewModel);

      await _fornecedorService.Atualizar (_mapper.Map<Fornecedor> (fornecedorViewModel));

      return CustomResponse (fornecedorViewModel);
    }

    [HttpDelete ("{id:guid}")]
    public async Task<ActionResult<FornecedorViewModel>> Excluir (Guid id) {
      var fornecedorViewModel = await ObterFornecedorEndereco (id);

      if (fornecedorViewModel == null) return NotFound ();

      await _fornecedorService.Remover (id);

      return CustomResponse (fornecedorViewModel);
    }

    [HttpGet ("obter-endereco/{id:guid}")]
    public async Task<EnderecoViewModel> ObterEnderecoPorId (Guid id) {
      var enderecoViewModel = _mapper.Map<EnderecoViewModel> (await _enderecoRepository.ObterPorId (id));
      return enderecoViewModel;
    }

    [HttpPut ("atualizar-endereco/{id:guid}")]
    public async Task<IActionResult> AtualizarEndereco (Guid id, EnderecoViewModel enderecoViewModel) {
      if (id != enderecoViewModel.Id) {
        NotificarErro ("O id informado não é o mesmo que foi passado na query.");
        return CustomResponse (enderecoViewModel);
      }

      if (!ModelState.IsValid) return CustomResponse (ModelState);

      await _fornecedorService.AtualizarEndereco (_mapper.Map<Endereco> (enderecoViewModel));

      return CustomResponse (enderecoViewModel);
    }

    private async Task<FornecedorViewModel> ObterFornecedorProdutosEndereco (Guid id) {
      return _mapper.Map<FornecedorViewModel> (await _fornecedorRepository.ObterFornecedorProdutosEndereco (id));
    }

    private async Task<FornecedorViewModel> ObterFornecedorEndereco (Guid id) {
      return _mapper.Map<FornecedorViewModel> (await _fornecedorRepository.ObterFornecedorEndereco (id));
    }
  }
}