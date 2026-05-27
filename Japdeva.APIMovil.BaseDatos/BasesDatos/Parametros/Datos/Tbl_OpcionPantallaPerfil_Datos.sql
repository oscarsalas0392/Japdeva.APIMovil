INSERT INTO "Tbl_OpcionPantallaPerfil" ("id", "idOpcionPantalla", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 1, 1, 1, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OpcionPantallaPerfil" WHERE "id" = 1);

INSERT INTO "Tbl_OpcionPantallaPerfil" ("id", "idOpcionPantalla", "idPerfil", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 2, 1, 2, 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OpcionPantallaPerfil" WHERE "id" = 2);
