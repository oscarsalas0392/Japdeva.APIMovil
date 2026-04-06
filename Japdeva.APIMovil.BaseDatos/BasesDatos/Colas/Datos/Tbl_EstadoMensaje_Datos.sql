INSERT INTO "Tbl_EstadoMensaje" ("id", "nombre", "descripcion") VALUES
    (1, 'Pendiente',  'Mensaje en espera de ser procesado'),
    (2, 'EnProceso',  'Mensaje siendo procesado por un consumidor'),
    (3, 'Exitoso',    'Mensaje procesado correctamente'),
    (4, 'Fallido',    'Procesamiento fallido, pendiente de reintento'),
    (5, 'Expirado',   'Mensaje no procesado dentro del tiempo límite')
ON CONFLICT ("id") DO NOTHING;
