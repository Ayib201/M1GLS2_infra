using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

/// <summary>
/// Résultat d'une tentative de création de profil : soit un succès (avec
/// l'utilisateur créé), soit un échec (avec un message explicite). Ce type
/// évite au contrôleur d'avoir à interpréter des exceptions pour un cas
/// d'erreur "normal" (email absent du jeton) -- les exceptions restent
/// réservées aux cas réellement anormaux (gérées par le middleware global,
/// voir ExceptionHandlingExtensions).
/// </summary>
public sealed record ProfilCreationResultat(bool EstReussi, Utilisateur? Utilisateur, string? MessageErreur)
{
    public static ProfilCreationResultat Succes(Utilisateur utilisateur) => new(true, utilisateur, null);

    public static ProfilCreationResultat Echec(string messageErreur) => new(false, null, messageErreur);
}
