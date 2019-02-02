namespace GED.Migrations
{
    using GED.Models;
    using GED.Models.Enums;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.IO;
    using System.Linq;
    using System.Web;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            //Popula tipos de arquivos no BD
            var listTipoArquivos = Helper.Util.GetEnumSelectListItem<FileTypes>();
            foreach (var fileType in listTipoArquivos)
            {
                TipoArquivo auxFileType = new TipoArquivo { NomeTipo = fileType.Text };
                context.TipoArquivos.Add(auxFileType);
            }
            context.SaveChanges();

            //Lista de extensões de arquivos
            List<string> extensoesExcel = new List<string>
            {
                ".xls",
                ".xlsx",
                ".xlsm"
            };
            List<string> extensoesImg = new List<string>
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".bmp",
                ".tiff"
            };
            List<string> extensoesMusica = new List<string>
            {
                ".mp3",
                ".oga"
            };
            List<string> extensoesPowerPoint = new List<string>
            {
                ".ppt",
                ".pptx"
            };
            List<string> extensoesTxt = new List<string>
            {
                ".txt",
                ".csv"
            };
            List<string> extensoesVideo = new List<string>
            {
                ".mp4",
                ".ogg",
                ".ogv",
                ".wav",
                ".webm"
            };
            List<string> extensoesWord = new List<string>
            {
                ".doc",
                ".docx",
                ".docm",
                ".pdf"
            };

            //Seed para os tipos de MIMEs de arquivos no BD
            foreach (var item in extensoesExcel)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.Excel);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo 
                });
            }
            foreach (var item in extensoesImg)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.Imagem);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo
                });
            }
            foreach (var item in extensoesMusica)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.Musica);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo
                });
            }
            foreach (var item in extensoesPowerPoint)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.PowerPorint);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo
                });
            }
            foreach (var item in extensoesTxt)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.Texto);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo
                });
            }
            foreach (var item in extensoesVideo)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.Video);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo
                });
            }
            foreach (var item in extensoesWord)
            {
                string tpFile = Helper.Util.GetEnumDescription(FileTypes.Word);
                context.TipoMimes.Add(new TipoMime
                {
                    Mime = Helper.Util.GetMimeNameFromExt(item).ToString(),
                    Extensao = item,
                    IdTipoArquivo = context.TipoArquivos.Where(f => f.NomeTipo == tpFile).SingleOrDefault().IdTipoArquivo
                });
            }

            context.SaveChanges();

            //Adicionar alguns arquivos de exemplo ao sistemas 
            //---- DEMO FILES -----
            foreach (var tipo in listTipoArquivos)
            {
                string samplePathFile = "Excel\\80d47f41-f20b-4c3b-a091-ece6a1e0b69a_v1.xls";
                string diretorio = Helper.Util.MapPath("~/Files/" + samplePathFile).Replace(samplePathFile, "") + tipo.Text + "\\";

                Console.WriteLine(diretorio);
                if (Directory.Exists(diretorio))
                {
                    var arquivos = Directory.GetFiles(diretorio).ToList();
                    if (arquivos.Count > 0)
                    {
                        foreach (var arq in arquivos)
                        {
                            FileInfo fileInfo = new FileInfo(arq);
                            context.Arquivos.Add(new Arquivo
                            {
                                ArquivoId = Guid.Parse(fileInfo.Name.Replace("_v1" + fileInfo.Extension.ToLower(), "")),
                                Tamanho = fileInfo.Length,
                                NomeArquivo = tipo.Text + "_sample" + fileInfo.Extension,
                                NomeFisicoReal = fileInfo.Name,
                                Diretorio = fileInfo.Directory.FullName,
                                DataUpload = DateTime.Now,
                                Versao = 1,
                                Extensao = fileInfo.Extension
                            });
                            context.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
