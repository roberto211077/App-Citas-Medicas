using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad;
using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace CitasMedicas.Web.Controllers;

public class EspecialidadesController(
    ListarEspecialidadesUseCase listarEspecialidades,
    CrearEspecialidadUseCase crearEspecialidad,
    BuscarTurnosUseCase buscarTurnos) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await listarEspecialidades.EjecutarAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> BuscarTurnos(
        int? id,
        DateOnly? fecha,
        FranjaHoraria? franja,
        CancellationToken cancellationToken)
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

        var busquedaRealizada = Request.Query.ContainsKey(nameof(fecha)) || Request.Query.ContainsKey(nameof(franja));
        if (busquedaRealizada)
        {
            if (!fecha.HasValue)
            {
                ModelState.AddModelError(nameof(fecha), "Seleccioná una fecha válida.");
            }

            if (!franja.HasValue)
            {
                ModelState.AddModelError(nameof(franja), "Seleccioná una franja horaria válida.");
            }
        }

        var model = new BuscarTurnosViewModel
        {
            EspecialidadId = especialidad.Id,
            EspecialidadNombre = especialidad.Nombre,
            EspecialidadDescripcion = especialidad.Descripcion,
            Fecha = fecha,
            Franja = franja,
            BusquedaRealizada = busquedaRealizada,
            Turnos = busquedaRealizada && fecha.HasValue && franja.HasValue && ModelState.IsValid
                ? await buscarTurnos.EjecutarAsync(especialidad.Id, fecha.Value, franja.Value, cancellationToken)
                : []
        };

        return View(model);
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
