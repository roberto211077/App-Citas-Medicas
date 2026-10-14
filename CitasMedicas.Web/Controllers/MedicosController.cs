using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.CatalogoMedico.EditarMedico;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos;
using CitasMedicas.Web.Modules.CatalogoMedico.RegistrarMedico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CitasMedicas.Web.Controllers;

public sealed class MedicosController(
    ListarEspecialidadesUseCase listarEspecialidades,
    RegistrarMedicoUseCase registrarMedico,
    ListarMedicosUseCase listarMedicos,
    EditarMedicoUseCase editarMedico) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        return View(await listarMedicos.EjecutarAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Registrar(CancellationToken cancellationToken)
    {
        await CargarEspecialidadesAsync(cancellationToken);
        return View(new RegistrarMedicoRequest());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarMedicoRequest request, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            var registrado = await registrarMedico.EjecutarAsync(request, cancellationToken);
            if (!registrado)
            {
                ModelState.AddModelError(nameof(request.EspecialidadId), "La especialidad seleccionada ya no está disponible.");
            }
            else
            {
                TempData["Mensaje"] = "El médico se registró correctamente.";
                return RedirectToAction(nameof(Registrar));
            }
        }

        await CargarEspecialidadesAsync(cancellationToken);
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken cancellationToken)
    {
        var request = await editarMedico.BuscarPorIdAsync(id, cancellationToken);
        if (request is null)
        {
            return NotFound();
        }

        await CargarEspecialidadesAsync(cancellationToken);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EditarMedicoRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            if (await editarMedico.BuscarPorIdAsync(id, cancellationToken) is null)
            {
                return NotFound();
            }

            await CargarEspecialidadesAsync(cancellationToken);
            return View(request);
        }

        var resultado = await editarMedico.EjecutarAsync(request, cancellationToken);
        if (resultado == EditarMedicoResultado.MedicoNoEncontrado)
        {
            return NotFound();
        }

        if (resultado == EditarMedicoResultado.EspecialidadNoEncontrada)
        {
            ModelState.AddModelError(nameof(request.EspecialidadId), "La especialidad seleccionada ya no está disponible.");
            await CargarEspecialidadesAsync(cancellationToken);
            return View(request);
        }

        TempData["Mensaje"] = "Los datos del médico se actualizaron correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarEspecialidadesAsync(CancellationToken cancellationToken)
    {
        var especialidades = await listarEspecialidades.EjecutarAsync(cancellationToken);
        ViewBag.Especialidades = especialidades
            .Select(especialidad => new SelectListItem(especialidad.Nombre, especialidad.Id.ToString()))
            .ToList();
    }
}
