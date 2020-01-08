using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers {

    [Route ("api/{controller}")]
    public class ProdutosController : MainController {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController (
            INotificador notificador,
            IProdutoRepository produtoRepository,
            IProdutoService produtoService,
            IMapper mapper) : base (notificador) {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos () {
            return _mapper.Map<IEnumerable<ProdutoViewModel>> (await _produtoRepository.ObterProdutosFornecedores ());
        }

        [HttpGet ("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId (Guid id) {
            var produtoViewModel = await ObterProduto (id);

            if (produtoViewModel == null) return NotFound ();

            return produtoViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar (ProdutoViewModel produtoViewModel) {
            if (!ModelState.IsValid) return CustomResponse (ModelState);

            var imagemNome = Guid.NewGuid () + "_" + produtoViewModel.Imagem;
            if (!UploadArquivo (produtoViewModel.ImagemUpload, imagemNome)) {
                return CustomResponse (produtoViewModel);
            }

            produtoViewModel.Imagem = imagemNome;
            await _produtoService.Adicionar (_mapper.Map<Produto> (produtoViewModel));

            return CustomResponse (produtoViewModel);
        }

        [HttpDelete ("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir (Guid id) {
            var produtoViewModel = await ObterProduto (id);

            if (produtoViewModel == null) return NotFound ();

            await _produtoService.Remover (id);

            return CustomResponse (produtoViewModel);
        }

        private bool UploadArquivo (string arquivo, string imgNome) {
            // converte a string para Base64
            var imageDataByteArray = Convert.FromBase64String (arquivo);

            // verifica se o arquivo é nulo ou menor que zero
            if (string.IsNullOrEmpty (arquivo)) {
                // adiciona uma msg de erro no modelState
                // ModelState.AddModelError (string.Empty, "Forneça uma imagem para este produto!");
                NotificarErro ("Forneça uma imagem para este produto!");
                return false;
            }

            // cria o path + nome do arquivo
            var filePath = Path.Combine (Directory.GetCurrentDirectory (), "wwwroot/images", imgNome);

            // verifica se existe o arquivo no path indicado
            if (System.IO.File.Exists (filePath)) {
                // adiciona uma msg de erro no modelState
                // ModelState.AddModelError (string.Empty, "Já existe um arquivo com este nome!");
                NotificarErro ("Já existe um arquivo com este nome!");
                return false;
            }

            // salva o arquivo no path indicado
            System.IO.File.WriteAllBytes (filePath, imageDataByteArray);

            return true;
        }

        private async Task<ProdutoViewModel> ObterProduto (Guid id) {
            return _mapper.Map<ProdutoViewModel> (await _produtoRepository.ObterProdutoFornecedor (id));
        }
    }
}