using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace GED.Helper
{
    /// <summary>
    /// Métodos uteis para o projeto em geral.
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Propriedade de somente leitura para mapear alguns formatos de Mime.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> MimeNames;
        /// <summary>
        /// Caracteres inválidos em nomes de arquivos.
        /// </summary>
        public static readonly IReadOnlyCollection<char> InvalidFileNameChars;

        /// <summary>
        /// Construtor.
        /// Carrega alguns tipos de Mime no contrutor que não são reconhecidos. <seealso cref="MimeNames"/>.
        /// </summary>
        static Util()
        {
            var mimeNames = new Dictionary<string, string>();

            mimeNames.Add(".mp3", "audio/mpeg"); 
            mimeNames.Add(".mp4", "video/mp4");
            mimeNames.Add(".ogg", "application/ogg");
            mimeNames.Add(".ogv", "video/ogg");
            mimeNames.Add(".oga", "audio/ogg");
            mimeNames.Add(".wav", "audio/x-wav");
            mimeNames.Add(".webm", "video/webm");
            mimeNames.Add(".csv", "text/csv");

            MimeNames = new ReadOnlyDictionary<string, string>(mimeNames);

            InvalidFileNameChars = Array.AsReadOnly(Path.GetInvalidFileNameChars());
        }

        /// <summary>
        /// Pegar o tipo de Mime de acordo com a extenção infomada.
        /// </summary>
        /// <param name="ext">extensão do arquivo. Exemplos: .txt, .ogg, .docx</param>
        /// <returns>Retorna um <see cref="MediaTypeHeaderValue"/>. Utilize o ToString() para pegar o Mime em string.</returns>
        public static MediaTypeHeaderValue GetMimeNameFromExt(string ext)
        {
            string value = MimeMapping.GetMimeMapping(ext);

            if(value != MediaTypeNames.Application.Octet)
            {
                return new MediaTypeHeaderValue(value);
            }
            else if (MimeNames.TryGetValue(ext.ToLowerInvariant(), out value))
            {
                return new MediaTypeHeaderValue(value);
            }
            else
            {
                return new MediaTypeHeaderValue(MediaTypeNames.Application.Octet);
            }
        }

        /// <summary>
        /// Verifica se o nome de algum arquivo possui caracteres inválidos.
        /// </summary>
        /// <param name="fileName">Nome do arquivo.</param>
        /// <returns>Retorna true se possuir algum caractere invalido e false caso contrário.</returns>
        public static bool AnyInvalidFileNameChars(string fileName)
        {
            return InvalidFileNameChars.Intersect(fileName).Any();
        }

        /// <summary>
        /// Retorna uma IEnumerable<SelectListItem> de um determinado tipo de <see cref="Enum"/>.
        /// </summary>
        /// <typeparam name="T">Tipo de enum.</typeparam>
        /// <returns>IEnumerable<SelectListItem> com descrição e enumeração do Enum.</returns>
        public static IEnumerable<SelectListItem> GetEnumSelectListItem<T>()
        {
            var itensEnum = GetItens(typeof(T));

            return (from item in itensEnum
                    select new SelectListItem()
                    {
                        Value = item.Key.ToString(),
                        Text = item.Value
                    }).ToList();
        }

        /// <summary>
        /// Chama o <see cref="GetItens(Type)"/> passando o tipo do enum.
        /// </summary>
        /// <typeparam name="T">Tipo struct e IConvertible</typeparam>
        /// <returns>Retorna um <see cref="Dictionary{int, string}"/> com as informações do enum.</returns>
        private static Dictionary<int, string> GetItens<T>() where T : struct, IConvertible
        {
            return GetItens(typeof(T));
        }

        /// <summary>
        /// Pega cada item de um <see cref="Enum"/> para um <see cref="Dictionary{int, string}"/>.
        /// </summary>
        /// <param name="enumType">Tipo do enum.</param>
        /// <returns>Retorna um <see cref="Dictionary{int, string}"/> com as informações do enum.</returns>
        private static Dictionary<int, string> GetItens(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type.");
            }

            Dictionary<int, string> dic = new Dictionary<int, string>();
            string[] names = Enum.GetNames(enumType);

            foreach (string item in names)
            {
                Enum e = (Enum)Enum.Parse(enumType, item);
                dic.Add(Convert.ToInt32(e), GetEnumDescription(e));
            }
            return dic;
        }

        /// <summary>
        /// Retornar a descrição de um <see cref="Enum"/>.
        /// </summary>
        /// <param name="value">Valor do enum a ser lido.</param>
        /// <returns>Retorna a string com descrição do enum.</returns>
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
        }

        /// <summary>
        /// Mapeia o caminho de determinado arquivo.
        /// Utilizado para recuperar o diretório da aplicação no método seed do EF.
        /// </summary>
        /// <param name="seedFile">Arquivo do projeto a ser mapeado.</param>
        /// <returns>Retorna o diretório do arquivo.</returns>
        public static string MapPath(string seedFile)
        {
            if (HttpContext.Current != null)
                return HostingEnvironment.MapPath(seedFile);

            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; //was AbsolutePath but didn't work with spaces according to comments
            var directoryName = Path.GetDirectoryName(absolutePath);
            var path = Path.Combine(directoryName, ".." + seedFile.TrimStart('~').Replace('/', '\\'));

            return path;
        }
    }
}