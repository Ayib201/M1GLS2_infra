using M1GLS2_infra.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace M1GLS2_infra.Controllers;

/// <summary>
/// Route imbriquée sous sa tâche ET son projet parents :
/// /api/v1/projets/{projetId}/taches/{tacheId}/commentaires. Même principe
/// que TachesController : la chaîne complète de propriété (projet -> tâche
/// -> commentaire) est vérifiée par ICommentaireService.
/// </summary>
[ApiController]
[Route("api/v1/projets/{projetId:guid}/taches/{tacheId:guid}/commentaires")]
[Authorize]
public class CommentairesController : ControllerBase
{
    private readonly ICommentaireService _commentaireService;

    public CommentairesController(ICommentaireService commentaireService)
    {
        _commentaireService = commentaireService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lister(Guid projetId, Guid tacheId)
    {
        var resultat = await _commentaireService.ListerCommentairesAsync(projetId, tacheId, User);

        if (resultat is null)
        {
            return NotFound();
        }

        Response.Headers["X-Cache-Status"] = resultat.ProvientDuCache ? "HIT" : "MISS";

        return Ok(resultat.Commentaires);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Creer(Guid projetId, Guid tacheId, [FromBody] CreerCommentaireRequest requete)
    {
        var commentaire = await _commentaireService.CreerCommentaireAsync(projetId, tacheId, requete, User);

        if (commentaire is null)
        {
            return NotFound(new { error = "Projet ou tâche introuvable." });
        }

        return CreatedAtAction(nameof(Lister), new { projetId, tacheId }, commentaire);
    }

    [HttpDelete("{commentaireId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer(Guid projetId, Guid tacheId, Guid commentaireId)
    {
        var supprime = await _commentaireService.SupprimerCommentaireAsync(projetId, tacheId, commentaireId, User);
        return supprime ? NoContent() : NotFound();
    }
}
