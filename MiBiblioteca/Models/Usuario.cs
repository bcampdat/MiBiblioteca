﻿using System;
using System.Collections.Generic;

namespace MiBiblioteca.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public byte[]? Salt { get; set; }

    public string? EnlaceCambioPass { get; set; }

    public DateTime? FechaEnvioEnlace { get; set; }
}
