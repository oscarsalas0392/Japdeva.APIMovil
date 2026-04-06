INSERT INTO "Tbl_Cola" ("nombre", "activo", "fechaRegistro") VALUES
    ('ObtenerPlantilla', TRUE, NOW()),
    ('Respuesta',        TRUE, NOW()),
    ('EnviarCorreo',     TRUE, NOW())
ON CONFLICT DO NOTHING;
