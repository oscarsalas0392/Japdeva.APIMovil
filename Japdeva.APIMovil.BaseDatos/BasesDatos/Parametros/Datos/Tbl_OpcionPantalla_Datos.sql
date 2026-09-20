INSERT INTO "Tbl_OpcionPantalla" ("id", "nombre", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 1, 'TabAtencionReclamo', 'Tab de Atención en la pantalla de información de reclamo', 1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_OpcionPantalla" WHERE "id" = 1);
