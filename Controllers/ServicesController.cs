using M1GLS2_infra.Services;
using Microsoft.AspNetCore.Mvc;

namespace M1GLS2_infra.Controllers;

/// <summary>
/// Endpoints de démonstration Service A / Service B (logique bouchonnée,
/// aucune vraie règle métier -- l'objectif de ce projet est la plomberie
/// d'infrastructure, pas la logique applicative).
///
/// [ApiController] active des comportements pratiques pour une API REST
/// (validation automatique, réponses 400 automatiques sur modèle invalide...).
/// [Route("api/v1")] fixe le préfixe commun à toutes les actions ci-dessous.
/// </summary>
[ApiController]
[Route("api/v1")]
public class ServicesController : ControllerBase
{
    private readonly IVaultSecretService _vaultSecretService;

    // Injection de dépendances via le constructeur : ASP.NET Core fournit
    // automatiquement l'implémentation enregistrée dans Program.cs
    // (voir VaultServiceExtensions.AddVaultSecretService).
    public ServicesController(IVaultSecretService vaultSecretService)
    {
        _vaultSecretService = vaultSecretService;
    }

    [HttpGet("serviceA")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ServiceA()
    {
        // Lit le secret dans Vault à CHAQUE appel (le client VaultSharp, lui,
        // a été authentifié une seule fois au démarrage de l'application).
        var secretValue = await _vaultSecretService.GetSecretValueAsync(
            secretPath: "external-api",
            secretKey: "CleSecreteExterne");

        return Ok(new { service = "A", secretRecupere = secretValue });
    }

    [HttpGet("serviceB")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ServiceB()
    {
        return Ok(new { service = "B", message = "Statut OK" });
    }
}
