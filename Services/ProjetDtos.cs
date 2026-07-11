using M1GLS2_infra.Models;

namespace M1GLS2_infra.Services;

// Records = types immuables, comparés par valeur -- parfaits pour des DTOs
// (Data Transfer Objects) qui ne font que transporter des données entre le
// client et le service, sans logique ni identité propre.

public sealed record CreerProjetRequest(string Nom, string? Description);

public sealed record CreerTacheRequest(string Titre, string? Description, DateTime? DateEcheance);

public sealed record MettreAJourTacheRequest(
    string Titre,
    string? Description,
    StatutTache Statut,
    DateTime? DateEcheance);

public sealed record CreerCommentaireRequest(string Contenu);
