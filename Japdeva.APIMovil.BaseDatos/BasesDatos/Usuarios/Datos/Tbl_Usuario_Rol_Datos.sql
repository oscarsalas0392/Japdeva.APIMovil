INSERT INTO "Tbl_Usuario_Rol" ("idUsuario", "idRol", "idUsuarioAdministrador", "fechaRegistro", "activo")
SELECT 1, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Usuario_Rol" WHERE "idUsuario" = 1 AND "idRol" = 1);
