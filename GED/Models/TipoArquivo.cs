using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GED.Models
{
    public class TipoArquivo
    {
        public int IdTipoArquivo { get; set; }
        public string NomeTipo { get; set; }
        public virtual ICollection<TipoMime> TipoMime { get; set; }
    }
}