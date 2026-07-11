// Contrats TypeScript décrivant exactement ce que renvoie l'API (backend
// .NET). Les avoir à un seul endroit évite de "deviner" la forme des objets
// dans chaque composant, et le compilateur TypeScript nous avertit si un
// champ est mal orthographié ou absent.
//
// Domaine métier : Utilisateur (déjà géré côté auth) -> Projet -> Tache ->
// Commentaire. Ces types reflètent les entités C# (Models/Projet.cs,
// Tache.cs, Commentaire.cs), en camelCase (convention JSON par défaut
// d'ASP.NET Core, voir Extensions/ControllersServiceExtensions.cs).

export type StatutTache = "AFaire" | "EnCours" | "Terminee";

export interface Projet {
  id: string;
  nom: string;
  description: string | null;
  dateCreation: string;
  utilisateurId: string;
}

export interface Tache {
  id: string;
  titre: string;
  description: string | null;
  statut: StatutTache;
  dateCreation: string;
  dateEcheance: string | null;
  projetId: string;
}

export interface Commentaire {
  id: string;
  contenu: string;
  dateCreation: string;
  tacheId: string;
  utilisateurId: string;
}

// Corps de requêtes (correspondent aux records C# dans Services/ProjetDtos.cs)

export interface CreerProjetRequest {
  nom: string;
  description?: string | null;
}

export interface CreerTacheRequest {
  titre: string;
  description?: string | null;
  dateEcheance?: string | null;
}

export interface MettreAJourTacheRequest {
  titre: string;
  description?: string | null;
  statut: StatutTache;
  dateEcheance?: string | null;
}

export interface CreerCommentaireRequest {
  contenu: string;
}

export interface ApiErrorResponse {
  error: string;
}
