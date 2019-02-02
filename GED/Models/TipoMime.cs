using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GED.Models
{
    public class TipoMime
    {
        public string Extensao { get; set; }
        public string Mime { get; set; }

        //FK
        public int IdTipoArquivo { get; set; }
        public virtual TipoArquivo TipoArquivo { get; set; }

        public virtual ICollection<Arquivo> Arquivos { get; set; }
    }
}