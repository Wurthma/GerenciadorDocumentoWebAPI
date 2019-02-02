using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GerenciadorDocumentos.Models
{
    public class ListFilesTypesModel
    {
        [Key]
        public string Mime { get; set; }
        public virtual FileTypesModel TipoArquivo { get; set; }
}
}