import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Configuration minimale : le plugin React active le "Fast Refresh"
// (rechargement à chaud sans perdre l'état des composants pendant le développement).
export default defineConfig({
  plugins: [react()],
});
