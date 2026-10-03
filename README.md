# Citas Médicas

## Conexión local a SQL Server

La aplicación lee `ConnectionStrings:CitasMedicas` desde la configuración de .NET. Para desarrollo en PowerShell, configure la variable de entorno en la sesión antes de iniciar la aplicación:

```powershell
$env:ConnectionStrings__CitasMedicas = 'Server=OPERACIONES3\SQLSERVER2019;Database=CitasMedicas;Integrated Security=True;TrustServerCertificate=True'
dotnet run --project CitasMedicas.Web
```

La variable usa autenticación integrada de Windows. No guarde esta cadena en archivos versionados; en otros entornos, configúrela como variable de entorno `ConnectionStrings__CitasMedicas`.

El script [database/001_catalogo_especialidades.sql](database/001_catalogo_especialidades.sql) crea la base y una tabla vacía `dbo.Especialidades` con `Id int IDENTITY(1,1)`. Para actualizar una instalación anterior con identificadores de texto, ejecute [database/002_id_autoincremental_especialidades.sql](database/002_id_autoincremental_especialidades.sql). Esa actualización reconstruye la tabla y elimina los registros existentes.
