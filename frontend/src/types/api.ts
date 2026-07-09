// Contrats TypeScript décrivant exactement ce que renvoie l'API (backend
// .NET). Les avoir à un seul endroit évite de "deviner" la forme des objets
// dans chaque composant, et le compilateur TypeScript nous avertit si un
// champ est mal orthographié ou absent.

export interface TokenResponse {
  access_token: string;
  expires_in: number;
  token_type: string;
}

export interface ServiceAResponse {
  service: "A";
  secretRecupere: string;
}

export interface ServiceBResponse {
  service: "B";
  message: string;
}

export interface ProfilResponse {
  id: string;
  nom: string;
  email: string;
  dateInscription: string;
}

export interface ApiErrorResponse {
  error: string;
}
