USE [CitasMedicas];
GO

IF OBJECT_ID(N'dbo.TurnosDisponibles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TurnosDisponibles
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_TurnosDisponibles PRIMARY KEY,
        EspecialidadId int NOT NULL,
        ProfesionalNombre nvarchar(150) NOT NULL,
        Fecha date NOT NULL,
        HoraInicio time NOT NULL,
        HoraFin time NOT NULL,
        Disponible bit NOT NULL CONSTRAINT DF_TurnosDisponibles_Disponible DEFAULT (1),
        CONSTRAINT FK_TurnosDisponibles_Especialidades FOREIGN KEY (EspecialidadId)
            REFERENCES dbo.Especialidades (Id),
        CONSTRAINT CK_TurnosDisponibles_Horario CHECK (HoraFin > HoraInicio)
    );
END;
GO

-- Ejemplos asociados a la primera especialidad disponible. Se pueden ejecutar varias veces.
DECLARE @EspecialidadId int = (SELECT TOP (1) Id FROM dbo.Especialidades ORDER BY Id);
DECLARE @Fecha date = DATEADD(day, 7, CAST(GETDATE() AS date));

IF @EspecialidadId IS NOT NULL
BEGIN
    INSERT INTO dbo.TurnosDisponibles
        (EspecialidadId, ProfesionalNombre, Fecha, HoraInicio, HoraFin, Disponible)
    SELECT @EspecialidadId, ejemplo.ProfesionalNombre, @Fecha, ejemplo.HoraInicio, ejemplo.HoraFin, ejemplo.Disponible
    FROM (VALUES
        (N'Ana Gómez', CAST('09:00' AS time), CAST('09:30' AS time), CAST(1 AS bit)),
        (N'Luis Pérez', CAST('10:00' AS time), CAST('10:30' AS time), CAST(1 AS bit)),
        (N'María López', CAST('14:00' AS time), CAST('14:30' AS time), CAST(1 AS bit)),
        (N'Carlos Ruiz', CAST('15:00' AS time), CAST('15:30' AS time), CAST(0 AS bit))
    ) AS ejemplo(ProfesionalNombre, HoraInicio, HoraFin, Disponible)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TurnosDisponibles existente
        WHERE existente.EspecialidadId = @EspecialidadId
          AND existente.ProfesionalNombre = ejemplo.ProfesionalNombre
          AND existente.Fecha = @Fecha
          AND existente.HoraInicio = ejemplo.HoraInicio
    );
END;
GO
