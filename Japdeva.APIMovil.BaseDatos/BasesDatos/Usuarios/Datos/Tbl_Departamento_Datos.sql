INSERT INTO "Tbl_Departamento" ("descripcion", "fechaRegistro", "activo") VALUES
    ('Unidad de Intendencia Portuaria', NOW(), TRUE),
    ('Unidad Financiero Contable',      NOW(), TRUE),
    ('Unidad de Asesoria Jurídica',     NOW(), TRUE),
    ('Gerencia Portuaria',              NOW(), TRUE)
ON CONFLICT DO NOTHING;
