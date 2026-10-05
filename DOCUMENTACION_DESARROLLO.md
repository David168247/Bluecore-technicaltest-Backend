# Documentación del desarrollo del backend Bluecore

## Estado actual

El backend gestiona solicitudes de crédito con ASP.NET Core 10, Entity Framework Core y PostgreSQL. Incluye registro de cuentas persistidas, inicio de sesión y autenticación JWT. Las solicitudes de crédito requieren un token válido. La suite actual contiene exactamente dos tests unitarios, dedicados al monto y al plazo.

Este documento explica los cambios realizados sobre el proyecto existente y cómo funcionan sus componentes. README.md contiene las instrucciones para configurarlo y ejecutarlo.

## Relación con la prueba técnica

La referencia es Prueba_Bluecore.docx. El documento pide crear solicitudes de crédito, listarlas con filtro por estado y aprobarlas o rechazarlas con comentario. Exige monto entre $500 y $50,000, plazo entre 6 y 60 meses y un mínimo de dos tests unitarios. La autenticación JWT básica es un bonus.

Se conserva PostgreSQL por la elección explícita de este proyecto. El documento permite elegir la base de datos; no se copiaron a código ni documentación las credenciales incluidas en la referencia.

| Requisito | Implementación en backend |
|---|---|
| Crear una solicitud | POST /api/credit-requests |
| Listar y filtrar solicitudes | GET /api/credit-requests, con status opcional |
| Aprobar o rechazar con comentario | PATCH /api/credit-requests/{id}/status |
| Validar monto y plazo | DataAnnotations y validación en el servicio |
| Errores HTTP legibles | ProblemDetails y middleware global |
| Dos tests unitarios | CreditRequestServiceTests |
| Login y token JWT | POST /api/auth/login |
| Instrucciones de ejecución | README.md |

El frontend conectado al backend y el funcionamiento de docker-compose no se verificaron como parte de estos cambios. Tener sus directorios o archivos no demuestra que esas partes de la entrega estén completas.

## Trabajo realizado por etapas

### Revisión inicial

Se revisaron la solución, los archivos C#, los cambios preparados en Git, el historial y las migraciones. Ya existían el modelo de crédito, sus DTOs, su servicio, su controller y una migración inicial.

Se detectaron dos archivos de configuración preparados por el usuario. JwtOptions.cs se incorporó a la autenticación. AdminCredentialsOptions.cs se conservó pendiente; la autenticación implementada consulta cuentas en PostgreSQL y no utiliza credenciales administrativas compartidas.

### Usuarios y autenticación

Se creó UserAccount y se configuró su tabla user_accounts. El registro valida los datos, normaliza usuario y correo, verifica duplicados, calcula el hash y persiste la cuenta.

El login busca por usuario o correo normalizado, verifica el hash y devuelve un JWT. Contraseña incorrecta y usuario inexistente producen el mismo error. PasswordHasher puede actualizar hashes antiguos tras un login válido. Para cuentas inexistentes se ejecuta también una verificación de hash, evitando una salida inmediata en esa rama.

La API verifica firma, algoritmo, emisor, audiencia y expiración del token. El middleware de autenticación se ejecuta antes de la autorización. CreditRequestsController usa Authorize y AuthenticationController permite acceso anónimo.

### Base de datos

Se inspeccionó el esquema PostgreSQL y se contrastó con la migración existente antes de aplicar cambios. Se creó user_accounts y se renombraron las columnas de credit_cases a snake_case.

Los cambios quedaron limitados a esquema_c. Los enums PostgreSQL existentes no se modificaron. La clave primaria de credit_cases se renombró sin eliminarla, conservando sus referencias.

Se aplicaron las dos migraciones nuevas. Se comprobó registro, login, persistencia de hashes, manejo de restricciones únicas y consultas de créditos contra PostgreSQL real. La transacción de comprobación se revirtió y no dejó cuentas de prueba.

### Organización y errores

Se separaron las configuraciones EF por entidad. El servicio de créditos se movió desde Services/Interfaces a Services/Implementations. Se retiraron los bloques try/catch repetidos de los controllers y se centralizaron los errores.

Los servicios validan sus entradas aunque se invoquen fuera de un controller. Las lecturas usan AsNoTracking. Las operaciones asincrónicas reciben CancellationToken desde la petición HTTP.

### Historial de Git

Los cambios se guardaron en commits separados y descriptivos. Posteriormente se tradujeron al español los mensajes de los 26 commits que existían entonces en main, manteniendo los prefijos de Conventional Commits.

La traducción cambió los identificadores de los commits, pero conservó sus árboles de archivos, autores y fechas. No se publicó ni se envió el proyecto por correo.

### Ajuste de tests

La primera implementación añadió una suite más amplia. Tras revisar la referencia y la preferencia del usuario, se retiraron esas pruebas y se dejaron exactamente dos tests. También se eliminó la dependencia Microsoft.AspNetCore.Mvc.Testing, que solo se necesitaba para las pruebas HTTP retiradas.

Las comprobaciones puntuales posteriores no se agregaron a la suite automatizada. Actualmente dotnet test ejecuta dos tests, no 37.

## Componentes principales

| Componente | Responsabilidad |
|---|---|
| AuthenticationController | Recibir registro/login y devolver la respuesta HTTP |
| AuthenticationService | Validar, normalizar, registrar y autenticar cuentas |
| IUserAccountRepository / UserAccountRepository | Consultar y guardar usuarios mediante EF Core |
| ITokenService / JwtTokenService | Emitir tokens JWT |
| AuthenticationConfiguration | Configurar JWT, PasswordHasher y dependencias |
| CreditRequestsController | Recibir las operaciones de crédito |
| CreditRequestService | Validar las reglas y consultar o modificar créditos |
| AppDbContext | Representar el modelo persistido |
| Configuraciones de entidades | Definir tablas, columnas e índices |
| AppDbContextFactory | Crear el contexto para herramientas EF sin depender de JWT |
| ExceptionHandlingMiddleware | Traducir errores a ProblemDetails |
| DTOs | Definir entradas y respuestas de la API |

No se añadió una capa genérica de repositorios para todas las entidades. El repositorio de usuarios concentra el tratamiento de duplicados y las consultas de autenticación; el servicio de créditos usa directamente el contexto existente.

## Flujo de autenticación

### Registro

1. Recibir username, email y password.
2. Validar el DTO.
3. Normalizar usuario y correo con mayúsculas invariantes para comparar.
4. Comprobar si el usuario o correo ya existe.
5. Hashear la contraseña con PasswordHasher de ASP.NET Core.
6. Guardar la cuenta y responder 201 con sus datos públicos.

Los índices únicos garantizan que dos registros simultáneos no creen duplicados. Una violación de esos índices se convierte en HTTP 409.

### Login

1. Recibir usernameOrEmail y password.
2. Validar tamaños y campos obligatorios.
3. Buscar por usuario o correo sin distinguir mayúsculas.
4. Verificar la contraseña contra el hash.
5. Actualizar el hash si lo requiere PasswordHasher.
6. Emitir el token y responder 200.

La respuesta contiene accessToken, tokenType, expiresAt y user. No contiene la contraseña ni el hash.

### Uso del token

Enviar Authorization: Bearer <accessToken> en las peticiones de crédito. El token usa HS256, contiene sub, unique_name y jti, y caduca según ExpirationMinutes. La tolerancia adicional de reloj está configurada en cero.

Registro y login comparten un límite de diez peticiones por minuto por IP y por instancia de API. Al superar el límite se devuelve 429.

## Validaciones actuales

| Dato | Regla |
|---|---|
| Username de registro | 3–50 caracteres; letras ASCII, números, punto, guion o guion bajo |
| Email de registro | Formato de correo y máximo 250 caracteres |
| Password de registro | 12–120 caracteres |
| UsernameOrEmail de login | Obligatorio; máximo 250 caracteres |
| Password de login | Obligatorio; máximo 120 caracteres |
| ApplicantId | Obligatorio; máximo 50 caracteres |
| Amount | Decimal entre 500 y 50000, incluidos ambos límites |
| TermMonths | Entero entre 6 y 60 |
| Status de filtro | Pending, Approved o Rejected, sin distinguir mayúsculas |
| Status de actualización | Approved o Rejected |
| Comment de actualización | Obligatorio; máximo 2000 caracteres |

La contraseña no se recorta. Los identificadores y comentarios se recortan donde corresponde.

La columna email de PostgreSQL conserva su capacidad original de 254 caracteres; la API limita nuevas entradas a 250. No fue necesario modificar la tabla para ese ajuste. Si hubiera cuentas antiguas con contraseñas mayores de 120 caracteres, el nuevo límite del DTO impediría usarlas en el login y habría que contemplar su actualización.

## Fixes realizados tras simplificar los tests

### Montos con centavos

Antes se usaba Range con límites enteros sobre una propiedad decimal. Esa combinación convertía el valor a entero: 499.99 podía convertirse a 500 y superar la validación.

Ahora Range compara valores decimal y usa cultura invariante. Se rechazan 499.99 y 50000.01; se aceptan los límites 500 y 50000 en la validación.

Commit: d807524 — fix: validar los límites del monto sin redondear los decimales.

### Estados enviados como números

Enum.TryParse admite números como 1, además de nombres. Antes eso podía aceptar valores que no coincidían con el contrato documentado.

Ahora se exige también que el texto recibido coincida con el nombre del estado. Se rechazan números y combinaciones de nombres, conservando la aceptación sin distinguir mayúsculas.

Commit: ee0cda8 — fix: rechazar estados numéricos en las solicitudes de crédito.

### Límites de login y registro

Se cambiaron los máximos del login a 250 caracteres para usuario/correo y 120 para contraseña. Se alineó el registro para impedir que una cuenta nueva se cree con datos que luego rechace el login.

Commit: 2e3f9e6 — fix: unificar los límites de correo y contraseña en autenticación.

### Errores de conexión envueltos por EF

EF puede envolver una excepción Npgsql en InvalidOperationException. El middleware anterior clasificaba ese caso como un error inesperado 500.

Ahora reconoce el error de base de datos envuelto y devuelve 503 con un mensaje legible. Una comprobación puntual confirmó que no expone el detalle interno de la excepción.

Commit: 018ba61 — fix: devolver HTTP 503 para errores de conexión envueltos por EF.

Estos fixes corresponden a problemas o ajustes efectivos del código; los commits describen los cambios aplicados.

## Tablas y migraciones

### esquema_c.user_accounts

Contiene id UUID, username, normalized_username, email, normalized_email, password_hash y created_at. Tiene una clave primaria y dos índices únicos sobre los identificadores normalizados.

### esquema_c.credit_cases

Contiene id, applicant_id, amount, term_months, status, comment, created_at y updated_at. Status se almacena como el entero de CreditStatus. Esta implementación no utiliza los enums PostgreSQL preexistentes.

### Migraciones del proyecto

1. 20261004225216_InitialCreate.
2. 20261005001213_AddUserAccounts.
3. 20261005002912_RenameCreditCaseColumnsToSnakeCase.

El historial permanece en esquema_c.__EFMigrationsHistory. No se ejecutan migraciones automáticamente durante el arranque.

## Respuestas HTTP

- 201: usuario o solicitud creados.
- 200: consulta, actualización o login correctos.
- 400: datos o reglas de negocio inválidos.
- 401: credenciales incorrectas o token ausente/inválido.
- 403: permisos insuficientes si una política de autorización los requiere.
- 404: recurso inexistente.
- 409: usuario o correo duplicados.
- 429: límite de solicitudes.
- 503: error de base de datos reconocido.
- 500: error inesperado.

Los errores usan application/problem+json. Las validaciones MVC incluyen errors. El middleware añade traceId a sus respuestas y conserva los detalles técnicos en los logs del servidor.

## Los dos tests actuales

CreditRequestServiceTests contiene:

1. CreateAsync_RejectsAmountsOutsideAllowedRange: verifica el rechazo de 499.99 y 50000.01 antes de acceder a PostgreSQL, y la aceptación de 500 y 50000 en la validación.
2. CreateAsync_RejectsTermsOutsideAllowedRange: verifica el rechazo de 5 y 61 meses antes de acceder a PostgreSQL, y la aceptación de 6 y 60 en la validación.

No necesitan una base activa ni secretos. Cubren las reglas de negocio centrales de la prueba técnica. La suite actual no incluye pruebas automatizadas de JWT, registro, middleware ni flujo HTTP.

Commit de reducción: 4e88a7a — test: limitar las pruebas unitarias a las dos reglas de negocio.

## Configuración y ejecución

Usar .NET User Secrets en desarrollo o variables de entorno en producción. La conexión PostgreSQL y Jwt:Key no deben guardarse en Git. README.md describe su configuración.

Desde backend:

    dotnet build
    dotnet test
    dotnet ef migrations list
    dotnet run --launch-profile https

La configuración actual de JWT se validó sin mostrar sus secretos. Issuer, Audience y ExpirationMinutes tienen valores públicos por defecto y pueden sobrescribirse desde el entorno.

## Qué falta para completar la entrega

La autorización actual permite a cualquier usuario autenticado operar sobre todos los créditos. No hay relación entre una cuenta y la cédula del solicitante ni políticas de trabajador/cliente. Definir esas reglas antes de abrir acceso a clientes finales.

No se implementaron refresh tokens, recuperación de contraseña, verificación de correo, MFA ni revocación inmediata. No se bloquean nuevas modificaciones de estado tras aprobar o rechazar una solicitud; esa regla adicional requiere una decisión de negocio.

El frontend conectado, el bonus de Angular y el funcionamiento de Docker deben verificarse en sus respectivos componentes. Esta documentación no los da por completados.
