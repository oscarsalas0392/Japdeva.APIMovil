INSERT INTO "Tbl_TipoCedula" ("tipo", "formato", "fechaRegistro", "activo") VALUES
    ('Cédula Física',     '^\d{9}$',                      NOW(), TRUE),
    ('Cédula Jurídica',   '^\d{10}$',                     NOW(), TRUE),
    ('DIMEX',             '^\d{11,12}$',                  NOW(), TRUE),
    ('Pasaporte',         '^[A-Z0-9]{6,9}$',              NOW(), TRUE);