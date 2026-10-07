using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("CitasMedicas");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Configure la cadena de conexión en ConnectionStrings:CitasMedicas o en la variable de entorno ConnectionStrings__CitasMedicas.");
}

builder.Services.AddDbContext<CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure.ClinicaDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades.ListarEspecialidadesUseCase>();
builder.Services.AddScoped<CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad.CrearEspecialidadUseCase>();
builder.Services.AddScoped<CitasMedicas.Web.Modules.Agenda.BuscarTurnos.BuscarTurnosUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Especialidades}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
