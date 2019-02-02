using GED.Helper;
using GED.Models;
using GED.Models.Enums;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace GED.Controllers
{
    /// <summary>
    /// Controller para strem de medias (músicas e vídeos).
    /// </summary>
    /// <remarks>https://www.codeproject.com/Articles/820146/HTTP-Partial-Content-In-ASP-NET-Web-API-Video</remarks>
    public class MediaController : ApiController
    {
        #region Fields

        /// <summary>
        /// Usado para copiar a entrada do stream para a saida do stream.
        /// </summary>
        public const int ReadStreamBufferSize = 1024 * 1024;

        /// <summary>
        /// Contexto da aplicação.
        /// </summary>
        ApplicationDbContext context = new ApplicationDbContext();

        #endregion

        #region Actions

        /// <summary>
        /// Executa o stream e responde com o <seealso cref="HttpStatusCode.PartialContent"/> até o final da execução.
        /// Apenas arquivos de musíca ou vídeo são consultados e caso não encontrados retorna o <seealso cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        /// <param name="id">Id do arquivo a ser consultado.</param>
        /// <returns>HttpResponseMessage conteúdo parcial (Stream vídeo e musíca).</returns>
        /// <remarks>Para mais detalhes do funcionamento do stream verificar: https://www.codeproject.com/Articles/820146/HTTP-Partial-Content-In-ASP-NET-Web-API-Video</remarks>
        [HttpGet]
        [Route("api/Media/Play/{id}")]
        public HttpResponseMessage Play(Guid id)
        {
            var tipoVideo = Util.GetEnumDescription(FileTypes.Video);
            var tipoMusica = Util.GetEnumDescription(FileTypes.Musica);
            //Busca apenas arquivos que sejam do tipo Música ou Vídeo
            var dadosArquivo = context.Arquivos
                .Where(a => a.ArquivoId == id && (a.TipoMime.TipoArquivo.NomeTipo == tipoVideo || a.TipoMime.TipoArquivo.NomeTipo == tipoMusica))
                .SingleOrDefault();

            if (dadosArquivo == null)
            {//Caso o Id não seja localizado no BD retorna 404
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            string caminhoArquivo = dadosArquivo.Diretorio + "\\" + dadosArquivo.NomeFisicoReal;

            // This can prevent some unnecessary accesses. 
            // These kind of file names won't be existing at all. 
            if (string.IsNullOrWhiteSpace(dadosArquivo.NomeFisicoReal) || Util.AnyInvalidFileNameChars(dadosArquivo.NomeFisicoReal))
                throw new HttpResponseException(HttpStatusCode.NotFound);

            FileInfo fileInfo = new FileInfo(Path.Combine(dadosArquivo.Diretorio, caminhoArquivo));

            if (!fileInfo.Exists)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            long totalLength = fileInfo.Length;

            RangeHeaderValue rangeHeader = base.Request.Headers.Range;
            HttpResponseMessage response = new HttpResponseMessage();

            response.Headers.AcceptRanges.Add("bytes");

            // The request will be treated as normal request if there is no Range header.
            if (rangeHeader == null || !rangeHeader.Ranges.Any())
            {
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new PushStreamContent((outputStream, httpContent, transpContext)
                =>
                {
                    using (outputStream) // Copy the file to output stream straightforward. 
                    using (Stream inputStream = fileInfo.OpenRead())
                    {
                        try
                        {
                            inputStream.CopyTo(outputStream, ReadStreamBufferSize);
                        }
                        catch (Exception error)
                        {
                            Debug.WriteLine(error);
                        }
                    }
                }, Util.GetMimeNameFromExt(fileInfo.Extension));

                response.Content.Headers.ContentLength = totalLength;
                return response;
            }

            long start = 0, end = 0;

            // 1. If the unit is not 'bytes'.
            // 2. If there are multiple ranges in header value.
            // 3. If start or end position is greater than file length.
            if (rangeHeader.Unit != "bytes" || rangeHeader.Ranges.Count > 1 ||
                !TryReadRangeItem(rangeHeader.Ranges.First(), totalLength, out start, out end))
            {
                response.StatusCode = HttpStatusCode.RequestedRangeNotSatisfiable;
                response.Content = new StreamContent(Stream.Null);  // No content for this status.
                response.Content.Headers.ContentRange = new ContentRangeHeaderValue(totalLength);
                response.Content.Headers.ContentType = Util.GetMimeNameFromExt(fileInfo.Extension);

                return response;
            }

            var contentRange = new ContentRangeHeaderValue(start, end, totalLength);

            // We are now ready to produce partial content.
            response.StatusCode = HttpStatusCode.PartialContent;
            response.Content = new PushStreamContent((outputStream, httpContent, transpContext)
            =>
            {
                using (outputStream) // Copy the file to output stream in indicated range.
                using (Stream inputStream = fileInfo.OpenRead())
                    CreatePartialContent(inputStream, outputStream, start, end);

            }, Util.GetMimeNameFromExt(fileInfo.Extension));

            response.Content.Headers.ContentLength = end - start + 1;
            response.Content.Headers.ContentRange = contentRange;

            return response;
        }

        #endregion

        #region Others
        /// <summary>
        /// Faz a leitura do range enviado no stream e devolve o inicio e fim.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="contentLength"></param>
        /// <param name="start">Saída: Inicio do range da leitura do arquivo de stream.</param>
        /// <param name="end">Saída: Final do range da leitura do arquivo de stream.</param>
        /// <returns>Retorna true se a leitura ainda não chegou ao fim do arquivo.</returns>
        private static bool TryReadRangeItem(RangeItemHeaderValue range, long contentLength,
            out long start, out long end)
        {
            if (range.From != null)
            {
                start = range.From.Value;
                if (range.To != null)
                    end = range.To.Value;
                else
                    end = contentLength - 1;
            }
            else
            {
                end = contentLength - 1;
                if (range.To != null)
                    start = contentLength - range.To.Value;
                else
                    start = 0;
            }
            return (start < contentLength && end < contentLength);
        }

        /// <summary>
        /// Cria um conteúdo parcial. "Pedaços" do arquivo passados por Stream.
        /// </summary>
        /// <param name="inputStream">Stream de entrada.</param>
        /// <param name="outputStream">Stream de saída.</param>
        /// <param name="start">Saída: Inicio do range da leitura do arquivo de stream.</param>
        /// <param name="end">Saída: Final do range da leitura do arquivo de stream.</param>
        private static void CreatePartialContent(Stream inputStream, Stream outputStream,
            long start, long end)
        {
            int count = 0;
            long remainingBytes = end - start + 1;
            long position = start;
            byte[] buffer = new byte[ReadStreamBufferSize];

            inputStream.Position = start;
            do
            {
                try
                {
                    if (remainingBytes > ReadStreamBufferSize)
                        count = inputStream.Read(buffer, 0, ReadStreamBufferSize);
                    else
                        count = inputStream.Read(buffer, 0, (int)remainingBytes);
                    outputStream.Write(buffer, 0, count);
                }
                catch (Exception error)
                {
                    Debug.WriteLine(error);
                    break;
                }
                position = inputStream.Position;
                remainingBytes = end - position + 1;
            } while (position <= end);
        }

        #endregion
    }
}