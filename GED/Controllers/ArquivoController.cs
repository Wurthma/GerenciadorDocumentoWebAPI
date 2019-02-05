using GED.Helper;
using GED.Models;
using GED.Models.Enums;
using GED.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace GED.Controllers
{
    public class ArquivoController : ApiController
    {
        /// <summary>
        /// Contexto da aplicação.
        /// </summary>
        ApplicationDbContext context = new ApplicationDbContext();

        /// <summary>
        /// Lista de arquivos. Apenas a versão mais atual de cada arquivo.
        /// Para detalhes de alterações de cada arquivo verificar <see cref="Get(Guid)"/>.
        /// </summary>
        /// <returns>Retorna uma lista com os dados dos arquivos em Json ou XML.</returns>
        /// <remarks>Rota: GET api/arquivo</remarks>
        public IEnumerable<ArquivoViewModel> Get()
        {
            var arquivos = context.Arquivos.ToList();
            return arquivos.Select(arq => (ArquivoViewModel)arq).ToList();
        }

        /// <summary>
        /// Informações de um arquivos específico com a lista de todas suas versões e modificações.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Retorna uma lista com os dados dos arquivos em Json ou XML.</returns>
        /// <remarks>Rota: GET api/arquivo/id</remarks>
        public IEnumerable<ArquivoViewModel> Get(Guid id)
        {
            var listVersoesArquivos = context.ArquivoModificacoes.Where(a => a.ArquivoId == id).ToList();
            if (listVersoesArquivos == null)
            {
                return null;
            }
            return listVersoesArquivos.Select(arq => (ArquivoViewModel)arq).ToList();
        }

        /// <summary>
        /// Faz o Upload de arquivo(s).
        /// Se formato não suportado será retornado <see cref="HttpStatusCode.UnsupportedMediaType"/>.
        /// Caso Upload realizado retornará <see cref="HttpStatusCode.Created"/>.
        /// </summary>
        /// <returns>Retorna <see cref="HttpResponseMessage"/> com resposta da requisição.</returns>
        /// /// <remarks>Rota: GET api/Arquivo/Upload</remarks>
        [Route("api/Arquivo/Upload")]
        public HttpResponseMessage Upload()
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    HttpPostedFile postedFile = httpRequest.Files[file];
                    string ext = Path.GetExtension(postedFile.FileName);

                    //Localiza o tipo do arquivo
                    var tipoMime = context.TipoMimes.Where(tm => tm.Extensao == ext).FirstOrDefault();

                    if (tipoMime == null)
                    {//Caso o tipo do arquivo não seja encontrado é por que este não é suportado
                        return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, ext);
                    }

                    string diretorio = HttpContext.Current.Server.MapPath("~/Files/" + tipoMime.TipoArquivo.NomeTipo + "/");
                    var idArquivo = Guid.NewGuid();
                    int versao = 1;
                    string nomeFisico = idArquivo.ToString() + "_v" + versao + ext;
                    string filePath = diretorio + nomeFisico;
                    DateTime? dataUpload = DateTime.Now;
                    //Gravar arquivo no diretório
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);

                    bool arquivoGravado = File.Exists(filePath);

                    if (!arquivoGravado)
                    {//Se ocorrer alguma falha ao salvar arquivo no diretório
                        result = Request.CreateResponse(HttpStatusCode.PreconditionFailed, docfiles);
                    }
                    else
                    {//Arquivo gravado com sucesso: gravar no BD
                        context.Arquivos.Add(new Arquivo
                        {
                            ArquivoId = idArquivo,
                            Tamanho = postedFile.ContentLength,
                            NomeArquivo = postedFile.FileName,
                            NomeFisicoReal = nomeFisico,
                            Diretorio = diretorio,
                            DataUpload = dataUpload,
                            Versao = versao,
                            Extensao = tipoMime.Extensao,
                        });

                        context.SaveChanges();
                    }
                }
                result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }

        /// <summary>
        /// Faz o download de determinado arquivo.
        /// </summary>
        /// <param name="id">Id do arquivo a ser realizado o download.</param>
        /// <returns>Retorna o arquivo utilizando <see cref="MediaTypeHeaderValue"/>.</returns>
        /// <remarks>
        /// Rota: GET api/Arquivo/Download/{id}
        /// Exemplo de URI: http://localhost:55709/api/Arquivo/Download/29666740-da8d-43f4-94ce-1d27e5de08d0
        /// </remarks>
        [HttpGet]
        [Route("api/Arquivo/Download/{id}")]
        //
        public HttpResponseMessage Download(Guid id)
        {
            var stream = new MemoryStream();
            var dadosArquivo = context.Arquivos.Where(a => a.ArquivoId == id).SingleOrDefault();
            string pathFile = dadosArquivo.Diretorio + "\\" + dadosArquivo.NomeFisicoReal;

            if (!File.Exists(pathFile))
            {//Se arquivo não existir retornar NotFound
                return Request.CreateResponse(HttpStatusCode.NotFound, id);
            }

            File.OpenRead(pathFile).CopyTo(stream);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(dadosArquivo.TipoMime.Mime);

            return result;
        }

        /// <summary>
        /// Faz o download de determinada versão do arquivo.
        /// </summary>
        /// <param name="id">Id do arquivo a ser consultado.</param>
        /// <param name="versao">Versão do arquivo.</param>
        /// <returns>Retorna o arquivo utilizando <see cref="MediaTypeHeaderValue"/>.</returns>
        /// <remarks>
        /// Rota: GET api/Arquivo/Download/{id}/{versao}
        /// Exemplo de URI: http://localhost:55709/api/arquivo/download/556c5bd9-70c6-4048-b4d0-5e278544ffa7/1
        /// </remarks>
        [HttpGet]
        [Route("api/Arquivo/Download/{id}/{versao}")]
        public HttpResponseMessage Download(Guid id, int versao)
        {
            var stream = new MemoryStream();
            var dadosArquivo = context.ArquivoModificacoes.Where(a => a.ArquivoId == id && a.Versao == versao).SingleOrDefault();
            string pathFile = dadosArquivo.Diretorio + "\\" + dadosArquivo.NomeFisicoReal;

            if (!File.Exists(pathFile))
            {//Se arquivo não existir retornar NotFound
                return Request.CreateResponse(HttpStatusCode.NotFound, id);
            }

            File.OpenRead(pathFile).CopyTo(stream);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(stream.ToArray())
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(dadosArquivo.Arquivo.TipoMime.Mime);

            return result;
        }

        /// <summary>
        /// Faz um upload substituindo um arquivo (versão é atualizada e posterior é mantida).
        /// Para download de versões anteriores verificar <see cref="Download(Guid, int)"/>.
        /// </summary>
        /// <param name="id">Id do arquivo a ser editado.</param>
        /// <returns>Retorna 200 (ok) caso edição realizada com sucesso.</returns>
        [Route("api/Arquivo/Edit/{id}")]
        public HttpResponseMessage Edit(Guid id)
        {
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;

            var dadosArquivo = context.Arquivos.Where(a => a.ArquivoId == id).SingleOrDefault();

            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    HttpPostedFile postedFile = httpRequest.Files[file];
                    string ext = Path.GetExtension(postedFile.FileName);

                    //Localiza o tipo do arquivo
                    var tipoMime = context.TipoMimes.Where(tm => tm.Extensao == ext).FirstOrDefault();

                    if (tipoMime == null)
                    {//Caso o tipo do arquivo não seja encontrado é por que este não é suportado
                        return Request.CreateResponse(HttpStatusCode.UnsupportedMediaType, ext);
                    }

                    string diretorio = HttpContext.Current.Server.MapPath("~/Files/" + tipoMime.TipoArquivo.NomeTipo + "/");
                    var idArquivo = dadosArquivo.ArquivoId;
                    int versao = ++dadosArquivo.Versao;
                    string nomeFisico = idArquivo.ToString() + "_v" + versao + ext;
                    string filePath = diretorio + nomeFisico;
                    DateTime? dataUpload = DateTime.Now;
                    //Gravar arquivo no diretório
                    postedFile.SaveAs(filePath);
                    docfiles.Add(filePath);

                    bool arquivoGravado = File.Exists(filePath);

                    if (!arquivoGravado)
                    {//Se ocorrer alguma falha ao salvar arquivo no diretório
                        result = Request.CreateResponse(HttpStatusCode.PreconditionFailed, docfiles);
                    }
                    else
                    {//Arquivo gravado com sucesso: gravar no BD

                        dadosArquivo.Tamanho = postedFile.ContentLength;
                        dadosArquivo.NomeArquivo = postedFile.FileName;
                        dadosArquivo.NomeFisicoReal = nomeFisico;
                        dadosArquivo.Diretorio = diretorio;
                        dadosArquivo.DataUpload = dataUpload;
                        dadosArquivo.Versao = versao;
                        dadosArquivo.Extensao = tipoMime.Extensao;

                        context.SaveChanges();
                    }
                }
                result = Request.CreateResponse(HttpStatusCode.Created, docfiles);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return result;
        }
    }
}
