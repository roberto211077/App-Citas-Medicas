using CitasMedicas.Web.Controllers;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.CatalogoMedico.RegistrarMedico;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class RegistrarMedicoTests
{
    [Fact]
    public async Task EjecutarAsync_RegistraMedicoAsociadoAEspecialidad()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad { Id = 1, Nombre = "Cardiología", Descripcion = "Atención cardiológica" });
        await dbContext.SaveChangesAsync();

        var registrado = await new RegistrarMedicoUseCase(dbContext).EjecutarAsync(new RegistrarMedicoRequest
        {
            Nombre = "  Ana Pérez  ",
            EspecialidadId = 1
        });

        Assert.True(registrado);
        var medico = await dbContext.Medicos.SingleAsync();
        Assert.Equal("Ana Pérez", medico.Nombre);
        Assert.Equal(1, medico.EspecialidadId);
    }

    [Fact]
    public async Task EjecutarAsync_EspecialidadInexistente_NoRegistraMedico()
    {
        await using var dbContext = CrearContexto();

        var registrado = await new RegistrarMedicoUseCase(dbContext).EjecutarAsync(new RegistrarMedicoRequest
        {
            Nombre = "Ana Pérez",
            EspecialidadId = 999
        });

        Assert.False(registrado);
        Assert.Empty(await dbContext.Medicos.ToListAsync());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Nombre de médico que supera ampliamente los ciento cincuenta caracteres y por lo tanto debe ser rechazado por la validación antes de intentar guardarlo en la base de datos para mantener el límite definido en el modelo" )]
    public void RegistrarMedicoRequest_NombreVacioOExcesivo_NoEsValido(string nombre)
    {
        var request = new RegistrarMedicoRequest { Nombre = nombre, EspecialidadId = 1 };

        var errores = new List<ValidationResult>();
        var valido = Validator.TryValidateObject(request, new ValidationContext(request), errores, validateAllProperties: true);

        Assert.False(valido);
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(RegistrarMedicoRequest.Nombre)));
    }

    [Fact]
    public async Task Registrar_Get_OfreceEspecialidadesDisponibles()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad { Id = 1, Nombre = "Cardiología", Descripcion = "Atención cardiológica" });
        await dbContext.SaveChangesAsync();
        var controller = CrearController(dbContext);

        var result = await controller.Registrar(default);

        Assert.IsType<ViewResult>(result);
        var opciones = Assert.IsAssignableFrom<IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>>(controller.ViewBag.Especialidades);
        Assert.Equal("Cardiología", Assert.Single(opciones).Text);
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
        new CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos.ListarMedicosUseCase(dbContext),
        new CitasMedicas.Web.Modules.CatalogoMedico.EditarMedico.EditarMedicoUseCase(dbContext));
}
