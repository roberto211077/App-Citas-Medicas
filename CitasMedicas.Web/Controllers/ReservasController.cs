using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.Reservas.ReservarTurno;
using Microsoft.AspNetCore.Mvc;

namespace CitasMedicas.Web.Controllers;

public sealed class ReservasController(
    BuscarTurnosUseCase buscarTurnos,
    ReservarTurnoUseCase reservarTurno) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reservar(
        ReservarTurnoRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            request.Turno = await buscarTurnos.BuscarDisponiblePorIdAsync(request.TurnoId, cancellationToken);
            if (request.Turno is null)
            {
                return NotFound();
            }

            var paciente = string.IsNullOrWhiteSpace(request.Dni)
                ? null
                : await reservarTurno.BuscarPacientePorDniAsync(request.Dni, cancellationToken);
            if (paciente is not null)
            {
                request.PacienteEncontrado = true;
                request.PacienteNombre = paciente.Nombre;
                request.PacienteApellido = paciente.Apellido;
                request.PacienteTelefono = paciente.Telefono;
                request.PacienteCorreo = paciente.Correo ?? string.Empty;
            }

            return View("~/Views/Especialidades/ConfirmarTurno.cshtml", request);
        }

        var resultado = await reservarTurno.EjecutarAsync(request, cancellationToken);
        if (resultado == ReservarTurnoResultado.TurnoNoEncontrado)
        {
            return NotFound();
        }

        if (resultado == ReservarTurnoResultado.TurnoYaReservado)
        {
            return Conflict("El turno seleccionado ya fue reservado.");
        }

        TempData["Mensaje"] = "La consulta se reservó correctamente.";
        return RedirectToAction("Index", "Especialidades");
    }
}
