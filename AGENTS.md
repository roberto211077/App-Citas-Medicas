# Repository Guidelines

## Instrucciones generales
Nunca modificar la base de datos directamente. Debes hacerlo a través de comandos de EF Core, como crear una migración y actualizar la base de datos via dicha migración.

## Project Structure & Modules

`CitasMedicas.slnx` is the solution and `CitasMedicas.Web/` is the ASP.NET Core MVC application. The web project contains startup code in `Program.cs`, MVC controllers and views, shared models, static assets in `wwwroot/`, and configuration in `appsettings*.json`. Business code belongs under `Modules/`, organized by capability (`CatalogoMedico`, `Agenda`, `Reservas`, `Clinica`). Within a module, keep `Domain`, `Application`, `Infrastructure`, and `Web` concerns local. Put complete use cases in vertical slices under `Application` (for example, `Agenda/Application/ConsultarDisponibilidad`). Keep `Shared` small and only for genuinely cross-module code. See `CONVENTIONS.md.txt` for the detailed architecture rules.

## Build, Test, and Run

Run commands from the repository root:

- `dotnet restore CitasMedicas.slnx` restores NuGet dependencies.
- `dotnet build CitasMedicas.slnx` builds the solution and reports compiler errors.
- `dotnet run --project CitasMedicas.Web` starts the web app locally; use the launch profile in `Properties/launchSettings.json`.
- `dotnet test CitasMedicas.slnx` runs tests when test projects are added. No test project or framework is currently present.

## Coding Style & Naming

Follow standard C# conventions: four spaces for indentation, `PascalCase` for types and public members, and `camelCase` for local variables and parameters. Keep nullable reference types enabled and follow nearby MVC/Razor patterns. Name modules and use-case folders in Spanish to match existing business terminology; use descriptive names such as `ReservarTurno`. Keep controllers focused on HTTP input, basic validation, invoking a use case, and returning a view or response. Put business rules in the relevant slice or domain. Respect module boundaries and avoid adding abstractions, frameworks, or layers without a concrete need.

## Testing

There is no established test framework or coverage threshold yet. Add tests for critical business behavior when introducing test projects, especially reservation availability, duplicate or concurrent booking, and cancellation releasing a time slot. Name tests for the behavior they verify and run them with `dotnet test CitasMedicas.slnx`.

## Commits & Pull Requests

No commit-message pattern is established in the available Git history. Use short, imperative commit subjects that describe one change (for example, `Add availability query slice`). Pull requests should explain the user-visible or architectural change, list validation commands and results, link a related issue when available, and include screenshots for UI changes.

## Configuration & Generated Files

Keep secrets and machine-specific values out of committed configuration; use development secrets or environment variables. Do not edit generated `bin/` or `obj/` output or vendor files under `wwwroot/lib/` unless dependency assets are intentionally being updated.


## Git Workflow

- El repositorio de GitHub de este proyecto es: `roberto211077/App-Citas-Medicas/issues`
- Nunca trabajes directamente sobre `main`.
- Antes de implementar cualquier feature, bugfix o issue, verifica la rama actual.
- Si la rama actual es `main`, crea una nueva rama antes de realizar cambios.
- Usa nombres descriptivos:
    - feature/<descripcion>
    -fix/<descripcion>
    -refactor/<descripcion>
- Realiza todos los commits en la nueva rama.
- Nunca hagas push directo a `main`