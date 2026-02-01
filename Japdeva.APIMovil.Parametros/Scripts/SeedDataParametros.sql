-- =============================================
-- Script de datos iniciales para el módulo de Parámetros
-- Base de datos: PostgreSQL
-- Proyecto: Japdeva.APIMovil.Parametros
-- =============================================

-- =============================================
-- Datos iniciales para Tbl_Pantalla
-- =============================================
INSERT INTO "Tbl_Pantalla" ("descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    ('Inicio', 1, NOW(), TRUE),
    ('Reclamos', 1, NOW(), TRUE),
    ('Mensajes', 1, NOW(), TRUE),
    ('Configuración', 1, NOW(), TRUE),
    ('Usuarios', 1, NOW(), TRUE)
ON CONFLICT DO NOTHING;

-- =============================================
-- Datos iniciales para Tbl_Menu
-- =============================================
INSERT INTO "Tbl_Menu" ("descripcion", "mostrar", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    ('Dashboard', TRUE, 1, NOW(), TRUE),
    ('Reclamos', TRUE, 1, NOW(), TRUE),
    ('Nuevo Reclamo', TRUE, 1, NOW(), TRUE),
    ('Mis Reclamos', TRUE, 1, NOW(), TRUE),
    ('Reportes', TRUE, 1, NOW(), TRUE),
    ('Configuración', TRUE, 1, NOW(), TRUE),
    ('Usuarios', FALSE, 1, NOW(), TRUE),
    ('Administración', FALSE, 1, NOW(), TRUE)
ON CONFLICT DO NOTHING;

-- =============================================
-- Datos iniciales para Tbl_MenuPerfil
-- =============================================
-- Perfil 1 (Administrador) - Tiene acceso a todos los menús
INSERT INTO "Tbl_MenuPerfil" ("idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    (1, 1, 1, NOW(), TRUE),  -- Dashboard
    (2, 1, 1, NOW(), TRUE),  -- Reclamos
    (3, 1, 1, NOW(), TRUE),  -- Nuevo Reclamo
    (4, 1, 1, NOW(), TRUE),  -- Mis Reclamos
    (5, 1, 1, NOW(), TRUE),  -- Reportes
    (6, 1, 1, NOW(), TRUE),  -- Configuración
    (7, 1, 1, NOW(), TRUE),  -- Usuarios
    (8, 1, 1, NOW(), TRUE)   -- Administración
ON CONFLICT DO NOTHING;

-- Perfil 2 (Usuario) - Tiene acceso limitado
INSERT INTO "Tbl_MenuPerfil" ("idMenu", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    (1, 2, 1, NOW(), TRUE),  -- Dashboard
    (2, 2, 1, NOW(), TRUE),  -- Reclamos
    (3, 2, 1, NOW(), TRUE),  -- Nuevo Reclamo
    (4, 2, 1, NOW(), TRUE)   -- Mis Reclamos
ON CONFLICT DO NOTHING;

-- =============================================
-- Datos iniciales para Tbl_TipoMensaje
-- =============================================
INSERT INTO "Tbl_TipoMensaje" ("descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    ('Información', 1, NOW(), TRUE),
    ('Advertencia', 1, NOW(), TRUE),
    ('Error', 1, NOW(), TRUE),
    ('Éxito', 1, NOW(), TRUE),
    ('Confirmación', 1, NOW(), TRUE)
ON CONFLICT DO NOTHING;

-- =============================================
-- Datos iniciales para Tbl_Mensaje
-- =============================================
INSERT INTO "Tbl_Mensaje" ("descripcion", "idTipoMensaje", "idPantalla", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    ('Bienvenido al sistema de gestión de reclamos', 1, 1, 1, NOW(), TRUE),
    ('¿Está seguro de que desea eliminar este reclamo?', 5, 2, 1, NOW(), TRUE),
    ('El reclamo fue creado exitosamente', 4, 2, 1, NOW(), TRUE),
    ('Debe completar todos los campos obligatorios', 2, 2, 1, NOW(), TRUE),
    ('Error al procesar la solicitud. Por favor, intente nuevamente', 3, 2, 1, NOW(), TRUE),
    ('No tiene mensajes nuevos', 1, 3, 1, NOW(), TRUE),
    ('Configuración actualizada correctamente', 4, 4, 1, NOW(), TRUE),
    ('Acceso denegado. No tiene permisos suficientes', 3, 5, 1, NOW(), TRUE)
ON CONFLICT DO NOTHING;

-- =============================================
-- Datos iniciales para Tbl_PlantillaCorreo
-- =============================================
INSERT INTO "Tbl_PlantillaCorreo" ("Plantilla", "idUsuarioInterno", "fechaRegistro", "activo")
VALUES
    (
        '<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Nuevo Reclamo Registrado</title>
</head>
<body>
    <h2>Estimado usuario,</h2>
    <p>Su reclamo ha sido registrado exitosamente en nuestro sistema.</p>
    <p><strong>Número de Reclamo:</strong> @idReclamo</p>
    <p>Recibirá notificaciones sobre el estado de su reclamo a través de este correo electrónico.</p>
    <p>Para consultar el estado de su reclamo, puede ingresar al sistema con su número de reclamo.</p>
    <br>
    <p>Atentamente,</p>
    <p><strong>Sistema de Gestión de Reclamos - JAPDEVA</strong></p>
</body>
</html>',
        1,
        NOW(),
        TRUE
    )
ON CONFLICT DO NOTHING;

-- =============================================
-- Fin del script de datos iniciales
-- =============================================
