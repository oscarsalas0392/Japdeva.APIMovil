INSERT INTO "Tbl_EstadoDetalleReclamo" (id, descripcion, "idUsuarioInterno", "fechaRegistro") VALUES 
    (1, 'Pendiente', 1, CURRENT_TIMESTAMP),
    (2, 'En Revisión', 1, CURRENT_TIMESTAMP),
    (3, 'Aprobado', 1, CURRENT_TIMESTAMP),
    (4, 'Rechazado', 1, CURRENT_TIMESTAMP),
    (5, 'Devuelto', 1, CURRENT_TIMESTAMP),
    (6, 'Finalizado', 1, CURRENT_TIMESTAMP)
ON CONFLICT (id) DO NOTHING;