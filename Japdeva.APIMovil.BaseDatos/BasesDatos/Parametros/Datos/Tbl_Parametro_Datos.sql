INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 1, 'TerminosCondiciones',
    'Al utilizar esta aplicación, usted acepta los presentes términos y condiciones de uso establecidos por JAPDEVA. El acceso y uso de la aplicación está reservado exclusivamente para usuarios autorizados. Queda prohibido el uso no autorizado, la reproducción, distribución o modificación de los contenidos sin autorización previa y escrita de JAPDEVA. La información contenida en esta aplicación es de carácter confidencial y su uso indebido puede derivar en acciones legales. JAPDEVA se reserva el derecho de modificar estos términos en cualquier momento.',
    NULL,
    'Texto de los términos y condiciones de uso de la aplicación',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 1);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 2, 'TelefonoContacto', '800-RECLAMOS', NULL,
    'Número de teléfono de contacto para atención de reclamos',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 2);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 3, 'EmailContacto', 'soporte@japdeva.go.cr', NULL,
    'Correo electrónico de contacto para atención al usuario',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 3);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 4, 'NombreOficina', 'Oficina Central', NULL,
    'Nombre de la oficina de atención presencial',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 4);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 5, 'DireccionOficina', 'Av. Principal 123, San José', NULL,
    'Dirección de la oficina de atención presencial',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 5);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 6, 'HorarioOficina', 'Lun-Vie: 8:00 AM - 5:00 PM', NULL,
    'Horario de atención de la oficina presencial',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 6);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 7, 'FaqCrearReclamo',
    '¿Cómo puedo crear un nuevo reclamo?',
    'Presiona el botón + en el centro de la navegación inferior para crear un nuevo reclamo.',
    'Pregunta frecuente sobre creación de reclamos',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 7);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 8, 'FaqTiempoResolucion',
    '¿Cuánto tiempo tarda en resolverse un reclamo?',
    'El tiempo promedio de resolución es de 5-7 días hábiles, dependiendo de la complejidad del caso.',
    'Pregunta frecuente sobre tiempos de resolución',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 8);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 9, 'FaqDocumentos',
    '¿Qué documentos debo adjuntar?',
    'Recomendamos adjuntar facturas, fotografías del medidor y cualquier documento relevante al reclamo.',
    'Pregunta frecuente sobre documentos requeridos',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 9);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 10, 'FaqSeguimiento',
    '¿Puedo hacer seguimiento de mi reclamo?',
    'Sí, en el detalle de cada caso puedes ver el estado actual y el historial de seguimiento.',
    'Pregunta frecuente sobre seguimiento de reclamos',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 10);
