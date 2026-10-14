using CitasMedicas.Web.Controllers;
using CitasMedicas.Web.Modules.CatalogoMedico.EditarMedico;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos;
using CitasMedicas.Web.Modules.CatalogoMedico.RegistrarMedico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class EditarMedicoTests
{
    [Fact]
    public async Task EjecutarAsync_ActualizaNombreYEspecialidad()
    {
        await using var dbContext = CrearContexto();
        PrepararCatalogo(dbContext);
        var medico = new Medico { Nombre = "Ana Pérez", EspecialidadId = 1 };
        dbContext.Medicos.Add(medico);
        await dbContext.SaveChangesAsync();

        var resultado = await new EditarMedicoUseCase(dbContext).EjecutarAsync(new EditarMedicoRequest
        {
            Id = medico.Id,
            Nombre = "  Ana Gómez  ",
            EspecialidadId = 2
        });

        Assert.Equal(EditarMedicoResultado.Actualizado, resultado);
        var actualizado = await dbContext.Medicos.AsNoTracking().SingleAsync();
        Assert.Equal("Ana Gómez", actualizado.Nombre);
        Assert.Equal(2, actualizado.EspecialidadId);
    }

    [Fact]
    public async Task EjecutarAsync_MedicoInexistente_NoModificaMedicos()
    {
        await using var dbContext = CrearContexto();
        PrepararCatalogo(dbContext);
        var medico = new Medico { Nombre = "Ana Pérez", EspecialidadId = 1 };
        dbContext.Medicos.Add(medico);
        await dbContext.SaveChangesAsync();

        var resultado = await new EditarMedicoUseCase(dbContext).EjecutarAsync(new EditarMedicoRequest
        {
            Id = medico.Id + 1,
            Nombre = "Nombre cambiado",
            EspecialidadId = 2
        });

        Assert.Equal(EditarMedicoResultado.MedicoNoEncontrado, resultado);
        Assert.Equal("Ana Pérez", (await dbContext.Medicos.SingleAsync()).Nombre);
    }

    [Fact]
    public async Task EjecutarAsync_EspecialidadInexistente_NoGuardaCambios()
    {
        await using var dbContext = CrearContexto();
        PrepararCatalogo(dbContext);
        var medico = new Medico { Nombre = "Ana Pérez", EspecialidadId = 1 };
        dbContext.Medicos.Add(medico);
        await dbContext.SaveChangesAsync();

        var resultado = await new EditarMedicoUseCase(dbContext).EjecutarAsync(new EditarMedicoRequest
        {
            Id = medico.Id,
            Nombre = "Nombre cambiado",
            EspecialidadId = 999
        });

        Assert.Equal(EditarMedicoResultado.EspecialidadNoEncontrada, resultado);
        var sinCambios = await dbContext.Medicos.AsNoTracking().SingleAsync();
        Assert.Equal("Ana Pérez", sinCambios.Nombre);
        Assert.Equal(1, sinCambios.EspecialidadId);
    }

    [Fact]
    public async Task Editar_Get_Inexistente_DevuelveNotFound()
    {
        await using var dbContext = CrearContexto();

        var resultado = await CrearController(dbContext).Editar(404, default);

        Assert.IsType<NotFoundResult>(resultado);
    }

    [Fact]
    public async Task Editar_Get_CargaDatosDelMedicoYEspecialidades()
    {
        await using var dbContext = CrearContexto();
        PrepararCatalogo(dbContext);
        dbContext.Medicos.Add(new Medico { Id = 5, Nombre = "Ana Pérez", EspecialidadId = 1 });
        await dbContext.SaveChangesAsync();
        var controller = CrearController(dbContext);

        var resultado = await controller.Editar(5, default);

        var view = Assert.IsType<ViewResult>(resultado);
        var request = Assert.IsType<EditarMedicoRequest>(view.Model);
        Assert.Equal("Ana Pérez", request.Nombre);
        Assert.Equal(1, request.EspecialidadId);
        IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> opciones = controller.ViewBag.Especialidades;
        Assert.Equal(2, opciones.Count());
    }

    [Fact]
    public async Task Index_ListaMedicosConSuEspecialidad()
    {
        await using var dbContext = CrearContexto();
        PrepararCatalogo(dbContext);
        dbContext.Medicos.Add(new Medico { Nombre = "Ana Pérez", EspecialidadId = 1 });
        await dbContext.SaveChangesAsync();

        var resultado = await CrearController(dbContext).Index(default);

        var view = Assert.IsType<ViewResult>(resultado);
        var medicos = Assert.IsAssignableFrom<IReadOnlyList<Medico>>(view.Model);
        var medico = Assert.Single(medicos);
        Assert.Equal("Cardiología", medico.Especialidad.Nombre);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Nombre excesivamente largo de médico que supera el límite de ciento cincuenta caracteres y por ello debe fallar la validación definida para editar el nombre del profesional en la aplicación médica")]
    public void EditarMedicoRequest_NombreVacioOExcesivo_NoEsValido(string nombre)
    {
        var request = new EditarMedicoRequest { Id = 1, Nombre = nombre, EspecialidadId = 1 };
        var errores = new List<ValidationResult>();

        var valido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.False(valido);
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(EditarMedicoRequest.Nombre)));
    }

    private static void PrepararCatalogo(ClinicaDbContext dbContext)
    {
        dbContext.Especialidades.AddRange(
            new Especialidad { Id = 1, Nombre = "Cardiología", Descripcion = "Atención cardiológica" },
            new Especialidad { Id = 2, Nombre = "Neurología", Descripcion = "Atención neurológica" });
        dbContext.SaveChanges();
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }

    private static MedicosController CrearController(ClinicaDbContext dbContext) => new(
        new CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades.ListarEspecialidadesUseCase(dbContext),
        new RegistrarMedicoUseCase(dbContext),
        new ListarMedicosUseCase(dbContext),
        new EditarMedicoUseCase(dbContext));
}
