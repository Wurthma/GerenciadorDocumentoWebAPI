using System.Collections.Generic;

namespace GED.Models.Interfaces
{
    public interface IHistoricoModificacaoArquivo
    {
        ICollection<ArquivoModificacao> ArquivoModificacoes { get; set; }
    }
}