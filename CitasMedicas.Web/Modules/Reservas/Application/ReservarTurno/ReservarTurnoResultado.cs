namespace CitasMedicas.Web.Modules.Reservas.Application.ReservarTurno;

public sealed record ReservarTurnoResultado(bool Exitoso, int? ReservaId, string Mensaje);
