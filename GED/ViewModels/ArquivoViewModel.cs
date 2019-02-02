using GED.Models;
using System;

namespace GED.ViewModels
{
    public class ArquivoViewModel
    {
        public Guid ArquivoId { get; set; }
        public long Tamanho { get; set; }
        public string NomeArquivo { get; set; }
        public DateTime? DataUpload { get; set; }
        public int Versao { get; set; }
        public string Extensao { get; set; }

        public static explicit operator ArquivoViewModel(Arquivo arquivo)
        {
            return new ArquivoViewModel
            {
                ArquivoId = arquivo.ArquivoId,
                Tamanho = arquivo.Tamanho,
                NomeArquivo = arquivo.NomeArquivo,
                DataUpload = arquivo.DataUpload,
                Extensao = arquivo.Extensao,
                Versao = arquivo.Versao
            };
        }

        public static explicit operator ArquivoViewModel(ArquivoModificacao arquivoModificacao)
        {
            return new ArquivoViewModel
            {
                ArquivoId = arquivoModificacao.ArquivoId,
                Tamanho = arquivoModificacao.Tamanho,
                NomeArquivo = arquivoModificacao.NomeArquivo,
                DataUpload = arquivoModificacao.DataModificacao,
                Versao = arquivoModificacao.Versao
            };
        }
    }
}