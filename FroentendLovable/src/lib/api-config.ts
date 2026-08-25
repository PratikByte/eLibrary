/**
 * Backend API configuration.
 *
 * The base URL for the ASP.NET Core backend is read from the
 * VITE_API_BASE_URL environment variable. This keeps the API address
 * configurable per environment (local dev, staging, production) without
 * changing any application code.
 *
 * Default: http://localhost:5285
 */
export const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5285").replace(
  /\/$/,
  "",
);
