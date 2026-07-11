using M1GLS2_infra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace M1GLS2_infra.Controllers;

/// <summary>
/// Route imbriquée sous son projet parent : /api/v1/projets/{projetId}/taches.
/// Ce choix REST reflète directement la hiérarchie métier (une tâche
/// n'existe jamais "seule", toujours rattachée à un projet) et évite de
/// devoir revérifier manuellement l'appartenance dans chaque contrôleur --
/// c'est ITacheService qui s'en charge à partir des deux ID de la route.
/// </summary>
[ApiController]
[Route("api/v1/projets/{projetId:guid}/taches")]
[Authorize]
public class TachesController : ControllerBase
{
    private readonly ITacheService _tacheService;

    public TachesController(ITacheService tacheService)
    {
        _tacheService = tacheService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lister(Guid projetId)
    {
        var taches = await _tacheService.ListerTachesAsync(projetId, User);
        return taches is null ? NotFound() : Ok(taches);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Creer(Guid projetId, [FromBody] CreerTacheRequest requete)
    {
        var tache = await _tacheService.CreerTacheAsync(projetId, requete, User);

        if (tache is null)
        {
            return NotFound(new { error = "Projet introuvable." });
        }

        return CreatedAtAction(nameof(Lister), new { projetId }, tache);
    }

    [HttpPut("{tacheId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MettreAJour(
        Guid projetId, Guid tacheId, [FromBody] MettreAJourTacheRequest requete)
    {
        var tache = await _tacheService.MettreAJourTacheAsync(projetId, tacheId, requete, User);
        return tache is null ? NotFound() : Ok(tache);
    }

    [HttpDelete("{tacheId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid projetId, Guid tacheId)
    {
        var supprime = await _tacheService.SupprimerTacheAsync(projetId, tacheId, User);
        return supprime ? NoContent() : NotFound();
    }
}
