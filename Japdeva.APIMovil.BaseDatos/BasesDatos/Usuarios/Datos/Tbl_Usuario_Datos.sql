INSERT INTO "Tbl_Usuario" ("id", "identificacion", "idTipoCedula", "nombre", "apellidos", "correo", "contrasena", "fechaRegistro", "fechaEdicion", "activo")
SELECT 1, 'admin', 1, 'admin', 'admin', 'admin@japdeva.com', 'adminJapdeva', NOW(), NULL, TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Usuario" WHERE "id" = 1);
