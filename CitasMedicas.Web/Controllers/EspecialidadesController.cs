using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CitasMedicas.Web.Controllers;

public class EspecialidadesController(
    ListarEspecialidadesUseCase listarEspecialidades,
    CrearEspecialidadUseCase crearEspecialidad) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await listarEspecialidades.EjecutarAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> BuscarTurnos(int? id, CancellationToken cancellationToken)
    {
        if (id is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var especialidad = await listarEspecialidades.BuscarPorIdAsync(id.Value, cancellationToken);
        if (especialidad is null)
        {
            return NotFound();
        }

        return View(especialidad);
    }

    [HttpGet]
    public IActionResult Crear()
    {
        return View(new CrearEspecialidadRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearEspecialidadRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        await crearEspecialidad.EjecutarAsync(request, cancellationToken);
        TempData["Mensaje"] = "La especialidad se creó correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
