﻿namespace MiBiblioteca.DTOs
{
    public class DTOAutoresLibros
    {
        public class DTOAutorLibro
        {
            public int IdAutor { get; set; }
            public string Nombre { get; set; }
            public int TotalLibros { get; set; }
            public decimal PromedioPrecio { get; set; }
            public List<DTOLibroItem> Libros { get; set; }
        }

        public class DTOLibroItem
        {
            public string Isbn { get; set; }
            public string Titulo { get; set; }
            public decimal Precio { get; set; }
        }

    }
}
