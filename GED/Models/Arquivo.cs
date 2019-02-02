using GED.Models.Interfaces;
using GED.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GED.Models
{
    public class Arquivo : IHistoricoModificacaoArquivo
    {
        public Guid ArquivoId { get; set; }
        public long Tamanho { get; set; }
        public string NomeArquivo { get; set; }
        public string NomeFisicoReal { get; set; }
        public string Diretorio { get; set; }
        public DateTime? DataUpload { get; set; }
        public int Versao { get; set; }
        //FK
        public string Extensao { get; set; }
        public virtual TipoMime TipoMime { get; set; }

        public virtual ICollection<ArquivoModificacao> ArquivoModificacoes { get; set; }

        public static explicit operator Arquivo(ArquivoViewModel arquivo)
        {
            return new Arquivo
            {
                ArquivoId = arquivo.ArquivoId,
                Tamanho = arquivo.Tamanho,
                NomeArquivo = arquivo.NomeArquivo,
                DataUpload = arquivo.DataUpload,
                Extensao = arquivo.Extensao,
                Versao = arquivo.Versao
            };
        }
    }
}