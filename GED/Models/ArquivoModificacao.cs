using GED.ViewModels;
using System;
namespace GED.Models
{
    public class ArquivoModificacao
    {
        public Guid ModificacaoId { get; set; }
        public long Tamanho { get; set; }
        public string NomeArquivo { get; set; }
        public string NomeFisicoReal { get; set; }
        public string Diretorio { get; set; }
        public int Versao { get; set; }
        public DateTime? DataModificacao { get; set; }

        //FK
        public Guid ArquivoId { get; set; }
        public virtual Arquivo Arquivo { get; set; }

        public static explicit operator ArquivoModificacao(ArquivoViewModel arquivo)
        {
            return new ArquivoModificacao
            {
                ArquivoId = arquivo.ArquivoId,
                Tamanho = arquivo.Tamanho,
                NomeArquivo = arquivo.NomeArquivo,
                DataModificacao = arquivo.DataUpload,
                Versao = arquivo.Versao
            };
        }
    }
}