using MiBiblioteca.Validators;

namespace MiBiblioteca.DTOs
{
    public class DTOLibroAgregar
    {
        public string Isbn { get; set; }
        public string Titulo { get; set; }

        [PaginasValidacion]
        public int Paginas { get; set; }
        public decimal Precio { get; set; }
        [PesoArchivoValidacion(PesoMaximoEnMegaBytes: 4)]
        [TipoArchivoValidacion(grupoTipoArchivo: GrupoTipoArchivo.Imagen)]
        public IFormFile Foto { get; set; }
        public int AutorId { get; set; }
        public int EditorialId { get; set; }
    }
}
