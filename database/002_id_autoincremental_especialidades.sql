USE [CitasMedicas];
GO

IF OBJECT_ID(N'dbo.Especialidades', N'U') IS NOT NULL
   AND EXISTS
   (
       SELECT 1
       FROM sys.columns
       WHERE object_id = OBJECT_ID(N'dbo.Especialidades')
         AND name = N'Id'
         AND (TYPE_NAME(user_type_id) <> N'int' OR is_identity = 0)
   )
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE referenced_object_id = OBJECT_ID(N'dbo.Especialidades')
    )
    BEGIN
        THROW 51000, 'No se puede reconstruir dbo.Especialidades mientras otras tablas tengan claves foráneas hacia ella.', 1;
    END;

    DROP TABLE dbo.Especialidades;
END;
GO

IF OBJECT_ID(N'dbo.Especialidades', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Especialidades
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Especialidades PRIMARY KEY,
        Nombre nvarchar(100) NOT NULL,
        Descripcion nvarchar(300) NOT NULL
    );
END;
GO
