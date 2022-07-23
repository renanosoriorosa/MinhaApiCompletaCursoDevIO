using AutoMapper;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Route("api/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(
            IProdutoRepository produtoRepository,
            IProdutoService produtoService,
            IMapper mapper,
            INotificador notificador) : base(notificador)
        {
            _produtoRepository = produtoRepository; 
            _produtoService = produtoService;   
            _mapper = mapper;   
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produto = await ObterProduto(id);

            if (produto == null) return NotFound();

            return produto;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

            if (!UploadArquivo(produtoViewModel.ImagemUpload, produtoViewModel.Imagem))
            {
                return CustomResponse(produtoViewModel);
            }

            produtoViewModel.Imagem = imagemNome;
            await _produtoRepository.Adicionar(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpPost("Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo
            (
            [ModelBinder(BinderType = typeof(ProdutoModelBinder))]
            ProdutoImagemViewModel produtoImagemViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefixo = Guid.NewGuid() + "_";

            if (!await UploadArquivoAlternativo(produtoImagemViewModel.ImagemUpload, imgPrefixo))
            {
                return CustomResponse(produtoImagemViewModel);
            }

            produtoImagemViewModel.Imagem = imgPrefixo + produtoImagemViewModel.ImagemUpload.FileName;
            await _produtoRepository.Adicionar(_mapper.Map<Produto>(produtoImagemViewModel));

            return CustomResponse(produtoImagemViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produtoViewModel = await ObterProduto(id);

            if (produtoViewModel == null) return NotFound();

            await _produtoRepository.Remover(id);

            return CustomResponse(produtoViewModel);
        }

        [RequestSizeLimit(40000000)]//limite de 40mb da imagem
        [HttpPost("imagem")]
        private async Task<ActionResult> AdicionarImagem(IFormFile file)
        {
            return Ok(file);
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (String.IsNullOrEmpty(arquivo))
            {
                NotificarErro("Forneça uma imagem para esse Produto!");
                return false;
            }

            var  imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificarErro("Já existe um arquivo com esse nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                NotificarErro("Forneça uma imagem para esse Produto!");
                return false;
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(path))
            {
                NotificarErro("Já existe um arquivo com esse nome!");
                return false;
            }

            using (var stream =  new FileStream(path, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }


        public async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
        }
    }
}
