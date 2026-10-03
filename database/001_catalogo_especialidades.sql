IF DB_ID(N'CitasMedicas') IS NULL
BEGIN
    CREATE DATABASE [CitasMedicas];
END;
GO

USE [CitasMedicas];
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

