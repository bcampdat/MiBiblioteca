using MiBiblioteca.Services;
using Microsoft.AspNetCore.Identity;

namespace MiBiblioteca.Classes
{
    public class ResultadoHash
    {
        public string Hash { get; set; }
        public byte[] Salt { get; set; }
    }
    
    
}
