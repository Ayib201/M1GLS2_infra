using M1GLS2_infra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace M1GLS2_infra.Controllers;

/// <summary>
/// Contrôleur "mince" (thin controller) : il ne fait que (1) recevoir la
/// requête, (2) déléguer à IProfilService, (3) traduire le résultat en
/// réponse HTTP. Aucune logique métier ni accès EF Core ici -- tout ça vit
/// dans Services/ProfilService.cs. Voir IProfilService pour le "pourquoi".
/// </summary>
[ApiController]
[Route("api/v1/profils")]
public class ProfilsController : ControllerBase
{
    private readonly IProfilService _profilService;

    public ProfilsController(IProfilService profilService)
    {
        _profilService = profilService;
    }

    /// <summary>
    /// [Authorize] = accès refusé (401) avant même que le corps de la
    /// méthode ne s'exécute, si aucun jeton Bearer valide n'est fourni.
    /// C'est le middleware UseAuthorization (voir Program.cs) qui bloque en amont.
    /// </summary>
    [HttpPost("creer")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Creer()
    {
        // "User" est fourni automatiquement par ControllerBase : c'est
        // l'identité extraite du jeton JWT par le middleware d'authentification.
        var resultat = await _profilService.CreerProfilDepuisJetonAsync(User);

        if (!resultat.EstReussi)
        {
            return BadRequest(new { error = resultat.MessageErreur });
        }

        return CreatedAtAction(nameof(Creer), new { id = resultat.Utilisateur!.Id }, resultat.Utilisateur);
    }
}
