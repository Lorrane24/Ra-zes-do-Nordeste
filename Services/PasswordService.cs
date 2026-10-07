using Microsoft.AspNetCore.Identity; 
using PROJETOF; // Importa o namespace onde a classe Usuario está declarada

namespace PROJETOF.Services; 
 
public class PasswordService 
{ 
    private readonly PasswordHasher<Usuario> _hasher = new PasswordHasher<Usuario>(); 
 
    public string GerarHash(Usuario usuario, string senha) 
    { 
        return _hasher.HashPassword(usuario, senha); 
    } 
 
    public bool VerificarSenha(Usuario usuario, string senhaHash, string senha) 
    { 
        var resultado = _hasher.VerifyHashedPassword(usuario, senhaHash, senha); 
 
        return resultado == PasswordVerificationResult.Success; 
    } 
}