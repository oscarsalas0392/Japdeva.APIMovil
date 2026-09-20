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

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 11, 'MinimoCaracteresContrasena', '6', NULL,
    'Cantidad mínima de caracteres requerida para la contraseña de usuario',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 11);

-- ─── Ayuda usuario interno ───────────────────────────────────────────────────

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 12, 'AyudaInternaBienvenida',
    'Sistema de Gestión de Reclamos JAPDEVA',
    'Esta aplicación te permite gestionar los reclamos de los clientes asignados a tu departamento. Aquí encontrás una guía rápida para sacarle el máximo provecho.',
    'Título y descripción de bienvenida en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 12);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 13, 'AyudaInternaComoFuncionaTitulo', '¿Cómo funciona?', NULL,
    'Título de la sección ¿Cómo funciona? en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 13);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 14, 'AyudaInternaComoFunciona1',
    'Al iniciar sesión, la pantalla de inicio muestra los reclamos pendientes en tu departamento.', NULL,
    'Paso 1 de la sección ¿Cómo funciona? en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 14);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 15, 'AyudaInternaComoFunciona2',
    'Cada reclamo indica su estado: Pendiente, En Proceso o Resuelto.', NULL,
    'Paso 2 de la sección ¿Cómo funciona? en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 15);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 16, 'AyudaInternaComoFunciona3',
    'Podés tomar un reclamo pendiente asignándotelo con el botón ''Tomar caso''.', NULL,
    'Paso 3 de la sección ¿Cómo funciona? en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 16);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 17, 'AyudaInternaComoFunciona4',
    'Una vez asignado, el reclamo pasa a ''En Proceso'' y queda bajo tu responsabilidad.', NULL,
    'Paso 4 de la sección ¿Cómo funciona? en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 17);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 18, 'AyudaInternaComoFunciona5',
    'El cliente recibe notificaciones por correo electrónico con las actualizaciones del caso.', NULL,
    'Paso 5 de la sección ¿Cómo funciona? en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 18);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 19, 'AyudaInternaRevisionTitulo', 'Revisión de casos', NULL,
    'Título de la sección Revisión de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 19);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 20, 'AyudaInternaRevision1',
    'Tocá sobre cualquier reclamo de la lista para ver su información completa.', NULL,
    'Paso 1 de la sección Revisión de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 20);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 21, 'AyudaInternaRevision2',
    'En el detalle podés consultar los datos del cliente, la descripción del problema y los documentos adjuntos.', NULL,
    'Paso 2 de la sección Revisión de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 21);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 22, 'AyudaInternaRevision3',
    'El historial de atenciones previas está disponible en la pestaña Atención.', NULL,
    'Paso 3 de la sección Revisión de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 22);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 23, 'AyudaInternaAtencionTitulo', 'Atención de casos', NULL,
    'Título de la sección Atención de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 23);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 24, 'AyudaInternaAtencion1',
    'Asignáte el caso tocando ''Tomar caso'' en la pantalla de inicio.', NULL,
    'Paso 1 de la sección Atención de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 24);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 25, 'AyudaInternaAtencion2',
    'Luego seleccioná ''Atender'' para acceder al formulario de atención.', NULL,
    'Paso 2 de la sección Atención de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 25);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 26, 'AyudaInternaAtencion3',
    'Completá la descripción de la atención y adjuntá los documentos internos necesarios.', NULL,
    'Paso 3 de la sección Atención de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 26);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 27, 'AyudaInternaAtencion4',
    'Al guardar, el estado del reclamo se actualiza y el cliente es notificado por correo.', NULL,
    'Paso 4 de la sección Atención de casos en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 27);

INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor1", "valor2", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT 28, 'AyudaInternaAyuda',
    '¿Necesitás ayuda?',
    'Si encontrás algún inconveniente con el sistema o tenés dudas sobre cómo proceder en un caso, contactá a tu superior directo o a los administradores del sistema para recibir asistencia.',
    'Título y descripción de la sección de ayuda/contacto en la página de ayuda del usuario interno',
    1, NOW(), TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 28);
