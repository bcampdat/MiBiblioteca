﻿using System;
using System.Collections.Generic;

namespace MiBiblioteca.Models;

public partial class Libro
{
    public string Isbn { get; set; } = null!;

    public string? Titulo { get; set; }

    public int? Paginas { get; set; }

    public decimal Precio { get; set; }

    public string? FotoPortadaUrl { get; set; }

    public bool? Descatalogados { get; set; }

    public int AutorId { get; set; }

    public int EditorialId { get; set; }

    public virtual Autore Autor { get; set; } = null!;

    public virtual Editoriale Editorial { get; set; } = null!;
}
