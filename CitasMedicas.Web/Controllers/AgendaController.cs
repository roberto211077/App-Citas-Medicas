using CitasMedicas.Web.Modules.Agenda.CargarDisponibilidad;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CitasMedicas.Web.Controllers;

public sealed class AgendaController(
    ListarMedicosUseCase listarMedicos,
    CargarDisponibilidadUseCase cargarDisponibilidad) : Controller
{
    [HttpGet]
    public async Task<IActionResult> CargarDisponibilidad(CancellationToken cancellationToken)
    {
        await CargarMedicosAsync(cancellationToken);
        return View(new CargarDisponibilidadRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CargarDisponibilidad(
        CargarDisponibilidadRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await CargarMedicosAsync(cancellationToken);
            return View(request);
        }

        var resultado = await cargarDisponibilidad.EjecutarAsync(request, cancellationToken);
        if (resultado == CargarDisponibilidadResultado.MedicoNoEncontrado)
        {
            ModelState.AddModelError(nameof(request.MedicoId), "El médico seleccionado ya no está disponible.");
        }
        else if (resultado == CargarDisponibilidadResultado.HorarioInvalido)
        {
            ModelState.AddModelError(nameof(request.HoraInicio), "El turno debe finalizar antes de la medianoche.");
        }
        else if (resultado == CargarDisponibilidadResultado.HorarioFueraDeFranja)
        {
            ModelState.AddModelError(nameof(request.HoraInicio), "El turno de 30 minutos debe quedar dentro de las franjas de 08:00 a 14:00 o de 15:00 a 22:00.");
        }
        else if (resultado == CargarDisponibilidadResultado.TurnoEnConflicto)
        {
            ModelState.AddModelError(nameof(request.HoraInicio), "El médico ya tiene un turno ese día. Debe haber al menos 30 minutos libres entre turnos.");
        }
        else
        {
            TempData["Mensaje"] = "El turno disponible se cargó correctamente.";
            return RedirectToAction(nameof(CargarDisponibilidad));
        }

        await CargarMedicosAsync(cancellationToken);
        return View(request);
    }

    private async Task CargarMedicosAsync(CancellationToken cancellationToken)
    {
        var medicos = await listarMedicos.EjecutarAsync(cancellationToken);
        ViewBag.Medicos = medicos
            .Select(medico => new SelectListItem(
                $"{medico.Nombre} — {medico.Especialidad.Nombre}",
                medico.Id.ToString()))
            .ToList();
    }
}
