# trackr

## Local development (recommended for UI work)

Run the **API in Docker** and the **Blazor WebAssembly app on the host** so Razor changes show up without rebuilding the nginx image.

1. Start only the API (from the repo root):

   ```bash
   docker compose up -d api
   ```

2. Run the frontend with **F5** on `src/frontend/frontend.csproj` (or `dotnet watch run --project src/frontend`). Use the **https** / **http** launch profile; `wwwroot/appsettings.Development.json` points `ApiBaseUrl` at `http://localhost:5080` (mapped to the API container).

3. CORS: `docker-compose.yml` allows the Blazor dev server origins (`http://localhost:5247`, `https://localhost:7205`) as well as the nginx UI (`http://localhost:5081`). After changing compose env vars, recreate the API: `docker compose up -d --force-recreate api`.

Use **Docker Compose with `web`** when you want a production-like static host (`http://localhost:5081`) or CI; rebuild `web` when the published WASM changes.