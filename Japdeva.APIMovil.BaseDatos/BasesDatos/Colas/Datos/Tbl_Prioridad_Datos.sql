INSERT INTO "Tbl_Prioridad" ("id", "nombre", "descripcion") VALUES
    (1, 'Alta',  'Mensajes críticos con atención inmediata'),
    (2, 'Media', 'Mensajes de prioridad estándar'),
    (3, 'Baja',  'Mensajes de baja urgencia')
ON CONFLICT ("id") DO NOTHING;