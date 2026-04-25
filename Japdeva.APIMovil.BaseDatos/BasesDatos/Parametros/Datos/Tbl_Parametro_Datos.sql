INSERT INTO "Tbl_Parametro" ("id", "nombre", "valor", "descripcion", "idUsuarioInterno", "fechaRegistro", "activo")
SELECT
    1,
    'TerminosCondiciones',
    'Al utilizar esta aplicación, usted acepta los presentes términos y condiciones de uso establecidos por JAPDEVA. El acceso y uso de la aplicación está reservado exclusivamente para usuarios autorizados. Queda prohibido el uso no autorizado, la reproducción, distribución o modificación de los contenidos sin autorización previa y escrita de JAPDEVA. La información contenida en esta aplicación es de carácter confidencial y su uso indebido puede derivar en acciones legales. JAPDEVA se reserva el derecho de modificar estos términos en cualquier momento.',
    'Texto de los términos y condiciones de uso de la aplicación',
    1,
    NOW(),
    TRUE
WHERE NOT EXISTS (SELECT 1 FROM "Tbl_Parametro" WHERE "id" = 1);
