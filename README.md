# Bluecore API

API ASP.NET Core 10 con EF Core y PostgreSQL. Toda tabla de aplicación y el historial de migraciones pertenecen exclusivamente a esquema_c.

La explicación del desarrollo, los fixes realizados y el alcance pendiente está en [DOCUMENTACION_DESARROLLO.md](DOCUMENTACION_DESARROLLO.md).

## Configuración local

Requisitos: .NET SDK 10, dotnet-ef 10 y una conexión PostgreSQL con acceso a esquema_c.
Ejecutar los comandos desde backend.

La conexión y la clave JWT se obtienen de .NET User Secrets en desarrollo o variables de entorno. No guardar secretos en appsettings.json, scripts, archivos HTTP ni Git. El proyecto ya tiene UserSecretsId.

Configurar localmente los valores reales, sustituyendo los marcadores:

    dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<conexion-postgresql>"
    dotnet user-secrets set "Jwt:Key" "<clave-aleatoria>"

Para generar una clave nueva en PowerShell y guardarla sin mostrarla:

    $jwtSecret = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
    @{ "Jwt:Key" = $jwtSecret } | ConvertTo-Json -Compress | dotnet user-secrets set
    Remove-Variable jwtSecret

La clave debe ser aleatoria y tener como mínimo 32 bytes UTF-8. Una frase larga predecible no es una clave segura. No reemplazar una clave existente salvo que se quiera invalidar los tokens emitidos.

Issuer, Audience y ExpirationMinutes son valores públicos en appsettings.json: BluecoreApi, BluecoreClients y 60 minutos. User Secrets y variables de entorno pueden sobrescribirlos. La API rechaza configuración JWT incompleta, una clave corta o una duración fuera de 1–120 minutos al arrancar.

Variables de entorno equivalentes:

- ConnectionStrings__DefaultConnection
- Jwt__Key
- Jwt__Issuer
- Jwt__Audience
- Jwt__ExpirationMinutes

En producción usar las variables del entorno o del gestor de secretos del despliegue. User Secrets no es un almacén cifrado ni una opción para producción. Utilizar TLS para PostgreSQL con verificación del certificado y servir la API por HTTPS.

## Migraciones y ejecución

    dotnet build
    dotnet test
    dotnet ef migrations list
    dotnet ef database update
    dotnet run --launch-profile https

La fábrica de diseño de AppDbContext permite ejecutar herramientas EF sin configurar JWT. Usa conexión desde User Secrets o entorno y no expone credenciales.

Migraciones:

1. InitialCreate: migración existente de credit_cases.
2. AddUserAccounts: crea esquema_c.user_accounts, con UUID, usuario, correo, identificadores normalizados, password_hash y created_at.
3. RenameCreditCaseColumnsToSnakeCase: renombra las columnas y la restricción primaria de credit_cases conservando datos y referencias. No elimina ni recrea la tabla. Los enums PostgreSQL preexistentes no se alteran.

No ejecutar migraciones automáticamente al iniciar la API. Revisar el SQL para el despliegue con:

    dotnet ef migrations script 20261004225216_InitialCreate --idempotent --output migration.sql

El historial existente conserva su nombre __EFMigrationsHistory para no romper la continuidad. Las tablas y columnas de negocio usan snake_case.

## Autenticación

POST /api/auth/register, acceso público. Cuerpo:

    {
      "username": "<usuario>",
      "email": "<correo>",
      "password": "<contraseña>"
    }

Devuelve 201 con id, username, email y createdAt. Nunca devuelve el hash ni la contraseña. Usuario de 3–50 caracteres ASCII: letras, números, punto, guion y guion bajo. Correo válido de hasta 250 caracteres. Contraseña de 12–120 caracteres; admite frases largas y no se recorta. Correo y usuario se comparan sin distinguir mayúsculas mediante valores normalizados y tienen índices únicos independientes. Los duplicados, incluso ante una carrera de inserción, devuelven 409.

POST /api/auth/login, acceso público. Cuerpo:

    {
      "usernameOrEmail": "<usuario-o-correo>",
      "password": "<contraseña>"
    }

El login admite hasta 250 caracteres para usernameOrEmail y hasta 120 para password. Devuelve 200 con accessToken, tokenType: Bearer, expiresAt en UTC y user. Credenciales incorrectas o usuario inexistente devuelven el mismo 401. Un login válido actualiza hashes antiguos cuando PasswordHasher indica que lo requieren.

Enviar el token en solicitudes protegidas:

    Authorization: Bearer <accessToken>

Los JWT se firman con HS256 y contienen sub (UUID de usuario), unique_name y jti. Se verifican firma, algoritmo, emisor, audiencia y expiración, sin tolerancia adicional de reloj. No incluyen contraseñas, hashes ni datos de crédito. Una modificación del token o su expiración produce 401.

Registro y login comparten un límite de 10 solicitudes por minuto por IP, por instancia de API, con respuesta 429. No configurar encabezados reenviados de proxies no confiables. En despliegues con varias instancias se necesita un límite compartido en el gateway.

## Endpoints de crédito

Todos requieren JWT válido:

| Método | Ruta | Resultado |
|---|---|---|
| POST | /api/credit-requests | 201 y Location de la solicitud creada |
| GET | /api/credit-requests?status=Pending | 200; filtro opcional Pending, Approved o Rejected |
| GET | /api/credit-requests/{id} | 200 o 404 |
| PATCH | /api/credit-requests/{id}/status | 200 o 404; solo Approved o Rejected |

Crear requiere applicantId (hasta 50 caracteres), amount (500–50000) y termMonths (6–60).
Actualizar estado requiere status y comment (no vacío, hasta 2000 caracteres).

La autorización implementada permite a cualquier usuario autenticado operar sobre los créditos existentes. No se añadió propiedad de créditos ni roles porque el modelo original no define esa relación. Antes de ofrecer acceso a clientes finales, definir políticas por rol y propietario, especialmente para listar créditos y aprobar/rechazar solicitudes.

## Errores y arquitectura

Errores HTTP en application/problem+json (ProblemDetails), con status, title y detail; la validación MVC añade errors. Errores globales incluyen traceId. Códigos: 400 datos inválidos, 401 autenticación, 403 permisos, 404 recurso inexistente, 409 cuenta duplicada, 429 límite de solicitudes, 503 error de base de datos y 500 error inesperado. Los detalles internos solo se registran en el servidor.

Controllers delgados delegan en services. AuthenticationService valida, normaliza y verifica hashes mediante PasswordHasher de ASP.NET Core (PBKDF2 con sal aleatoria). UserAccountRepository encapsula consultas EF y convierte violaciones PostgreSQL de los índices únicos de usuario/correo a conflicto de negocio. JwtTokenService emite tokens. AppDbContext usa configuraciones separadas por entidad. Lecturas usan AsNoTracking y las operaciones de datos aceptan CancellationToken.

Archivos principales:

- Models/UserAccount.cs; Data/Configurations/UserAccountConfiguration.cs.
- Data/Configurations/CreditRequestConfiguration.cs; Data/AppDbContext.cs; Data/AppDbContextFactory.cs.
- DTOs/RegisterUserDto.cs, LoginDto.cs, AuthenticationResponseDto.cs y UserAccountResponseDto.cs.
- Services/Implementations/AuthenticationService.cs, UserAccountRepository.cs, JwtTokenService.cs y CreditRequestService.cs.
- Services/Interfaces/IAuthenticationService.cs, IUserAccountRepository.cs, ITokenService.cs e ICreditRequestService.cs.
- Controllers/AuthenticationController.cs y CreditRequestsController.cs.
- Configuration/JwtOptions.cs y AuthenticationConfiguration.cs; Middleware/ExceptionHandlingMiddleware.cs; Program.cs.
- Migrations/AddUserAccounts y RenameCreditCaseColumnsToSnakeCase, con sus snapshots generados.
- Tests/CreditRequestServiceTests.cs contiene exactamente dos tests unitarios: límites del monto ($500–$50,000) y del plazo (6–60 meses). Verifican el rechazo de valores fuera de rango antes de acceder a la base y la aceptación de los valores límite en la validación.

Los dos tests unitarios no requieren una conexión PostgreSQL activa ni secretos reales.

## Alcance de la prueba técnica

El flujo base cubre crear solicitudes, listar y filtrar por estado, y aprobar o rechazar con comentario. Las validaciones requeridas son monto entre $500 y $50,000 y plazo entre 6 y 60 meses. Se incluyen dos tests unitarios de esas reglas. La autenticación JWT corresponde al bonus del documento; se mantiene el registro persistido solicitado para este proyecto. PostgreSQL se conserva como la base elegida.

## Alcance pendiente

No se implementaron refresh tokens, revocación inmediata, cierre de sesión en servidor, recuperación de contraseña, verificación de correo, MFA ni roles. El cliente puede descartar su token al salir, pero seguirá siendo válido hasta expirar. Considerar esas funciones y políticas por propietario/rol según los requisitos del producto. Mantener pruebas de integración PostgreSQL en una base aislada de CI.
