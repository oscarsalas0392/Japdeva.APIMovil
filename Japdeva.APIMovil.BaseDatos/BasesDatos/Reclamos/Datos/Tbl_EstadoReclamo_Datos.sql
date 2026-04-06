INSERT INTO "Tbl_EstadoReclamo" (id, descripcion, "idUsuarioInterno", "fechaRegistro") VALUES 
    (1, 'Pendiente', 1, CURRENT_TIMESTAMP),
    (2, 'En Proceso', 1, CURRENT_TIMESTAMP),
    (3, 'Completado', 1, CURRENT_TIMESTAMP),
    (4, 'Rechazado', 1, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;