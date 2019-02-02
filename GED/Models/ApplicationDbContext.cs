using GED.Models.Interfaces;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;

namespace GED.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public override int SaveChanges()
        {
            try
            {
                foreach (var arquivo in ChangeTracker.Entries()
                                .Where(e => e.Entity is IHistoricoModificacaoArquivo && (e.State == EntityState.Added || e.State == EntityState.Modified))
                                .Select(e => e.Entity as Arquivo))
                {
                    ArquivoModificacoes.Add(new ArquivoModificacao
                    {
                        ModificacaoId = Guid.NewGuid(),
                        Tamanho = arquivo.Tamanho,
                        NomeArquivo = arquivo.NomeArquivo,
                        NomeFisicoReal = arquivo.NomeFisicoReal,
                        Diretorio = arquivo.Diretorio,
                        Versao = arquivo.Versao,
                        ArquivoId = arquivo.ArquivoId,
                        DataModificacao = arquivo.DataUpload
                    });
                }
                var result = base.SaveChanges();
                return result;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join(": ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, "Os erros de validação são: ", fullErrorMessage);
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Configure default schema
            //modelBuilder.HasDefaultSchema("Admin");

            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<TipoArquivo>()
                .HasKey(k => k.IdTipoArquivo);

            modelBuilder.Entity<TipoMime>()
                .HasKey(k => k.Extensao)
                .Property(p => p.Extensao)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            modelBuilder.Entity<Arquivo>()
                .HasKey(k => k.ArquivoId);

            modelBuilder.Entity<ArquivoModificacao>()
                .HasKey(k => k.ModificacaoId);
        }

        public DbSet<TipoArquivo> TipoArquivos { get; set; }
        public DbSet<TipoMime> TipoMimes { get; set; }
        public DbSet<Arquivo> Arquivos { get; set; }
        public DbSet<ArquivoModificacao> ArquivoModificacoes { get; set; }
    }
}