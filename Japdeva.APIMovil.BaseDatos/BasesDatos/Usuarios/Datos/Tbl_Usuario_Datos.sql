INSERT INTO "Tbl_Usuario" ("identificacion", "idTipoCedula", "nombre", "apellidos", "correo", "contrasena", "fechaRegistro", "fechaEdicion", "activo") VALUES
    ('admin', 1, 'admin', 'admin', 'admin', 'admin', NOW(), NULL, TRUE)
ON CONFLICT DO NOTHING;
