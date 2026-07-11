namespace M1GLS2_infra.Models;

/// <summary>
/// Statut d'avancement d'une tâche. Une "enum" (énumération) restreint les
/// valeurs possibles à une liste fermée : impossible de stocker autre chose
/// que ces trois états, ni en C# ni en base (EF Core la convertit en texte
/// dans PostgreSQL, voir AppDbContext.OnModelCreating).
/// </summary>
public enum StatutTache
{
    AFaire,
    EnCours,
    Terminee
}
