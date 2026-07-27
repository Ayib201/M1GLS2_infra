using M1GLS2_infra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace M1GLS2_infra.Controllers;

/// <summary>
/// Contrôleur "mince" : reçoit la requête HTTP, délègue à IProjetService,
/// traduit le résultat en réponse HTTP. Toute la logique (et notamment la
/// vérification "ce projet appartient-il à l'appelant ?") vit dans le
/// service -- voir Services/ProjetService.cs.
///
/// [Authorize] appliqué au niveau de la CLASSE : toutes les actions ci-dessous
/// exigent un jeton Bearer valide, pas besoin de le répéter sur chacune.
/// </summary>
[ApiController]
[Route("api/v1/projets")]
[Authorize]
public class ProjetsController : ControllerBase
{
    private readonly IProjetService _projetService;

    public ProjetsController(IProjetService projetService)
    {
        _projetService = projetService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Lister()
    {
        var resultat = await _projetService.ListerMesProjetsAsync(User);

        // Header custom (même idée que les CDN/caches HTTP classiques :
        // X-Cache-Status). Visible dans l'onglet Network du navigateur ou
        // via `curl -i` -- utile pour PROUVER en démo que le cache a servi
        // la réponse, sans avoir à lire les logs serveur.
        Response.Headers["X-Cache-Status"] = resultat.ProvientDuCache ? "HIT" : "MISS";

        return Ok(resultat.Projets);
    }

    [HttpGet("{projetId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obtenir(Guid projetId)
    {
        var projet = await _projetService.ObtenirProjetAsync(projetId, User);
        return projet is null ? NotFound() : Ok(projet);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Creer([FromBody] CreerProjetRequest requete)
    {
        var projet = await _projetService.CreerProjetAsync(requete, User);
        return CreatedAtAction(nameof(Obtenir), new { projetId = projet.Id }, projet);
    }

    [HttpDelete("{projetId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid projetId)
    {
        var supprime = await _projetService.SupprimerProjetAsync(projetId, User);
        return supprime ? NoContent() : NotFound();
    }
}
