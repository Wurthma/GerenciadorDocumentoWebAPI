using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GerenciadorDocumentos.Models
{
    public class FileTypesModel
    {
        [Key]
        public int ID { get; set; }
        public string NomeTipo { get; set; }
        public virtual ICollection<ListFilesTypesModel> TipoArquivo { get; set; }
    }
}