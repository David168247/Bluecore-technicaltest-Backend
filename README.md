# API de créditos

Backend para registrar usuarios, iniciar sesión y gestionar solicitudes de crédito. Desarrollado con ASP.NET Core 10, Entity Framework Core y PostgreSQL.

## Requisitos

- .NET SDK 10.
- PostgreSQL.
- Postman.

## Descargar el proyecto

```sh
git clone https://github.com/David168247/Bluecore-technicaltest-Backend.git
cd Bluecore-technicaltest-Backend
dotnet restore
```

Si ya tienes el proyecto descargado, ejecuta los siguientes comandos desde la carpeta que contiene `BluecoreApi.csproj`.

## Configurar la conexión

Reemplaza los valores de la conexión por los de tu PostgreSQL:

```sh
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=creditos;Username=postgres;Password=TU_PASSWORD"
```

Configura una clave para los tokens JWT. En PowerShell 7:

```powershell
$jwtKey = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set "Jwt:Key" $jwtKey
```

Esta configuración se guarda fuera del repositorio. Solo necesitas hacerla al preparar el proyecto en una máquina nueva. Si ya tienes una conexión y una clave configuradas, puedes continuar con el siguiente paso.

## Preparar la base de datos

Instala la herramienta de Entity Framework si todavía no la tienes:

```sh
dotnet tool install --global dotnet-ef --version 10.0.12
```

Aplica las migraciones:

```sh
dotnet ef database update
```

El comando crea las tablas de usuarios y créditos en el esquema `esquema_c` de la base configurada.

## Iniciar la API

```sh
dotnet run --launch-profile http
```

La API queda disponible en **http://localhost:5171**. La dirección base no muestra una página: utiliza Postman para enviar las peticiones.

Puedes abrir **http://localhost:5171/openapi/v1.json** para consultar la descripción de los endpoints durante el desarrollo. El proyecto no incluye una interfaz Swagger.

Para detener el servidor, presiona `Ctrl+C` en la terminal.

## Probar la autenticación

En Postman, selecciona `Body → raw → JSON`.

Primero crea una cuenta con `POST http://localhost:5171/api/auth/register`:

```json
{
  "username": "usuario_demo",
  "email": "usuario_demo@example.com",
  "password": "Ejemplo-registro-2026"
}
```

La contraseña del registro debe tener al menos 12 caracteres. El registro devuelve la cuenta creada; después debes iniciar sesión.

Envía `POST http://localhost:5171/api/auth/login`:

```json
{
  "usernameOrEmail": "usuario_demo",
  "password": "Ejemplo-registro-2026"
}
```

Copia el `accessToken` de la respuesta. En las peticiones de créditos, selecciona `Authorization → Bearer Token` y pega ese token.

## Solicitudes de crédito

| Método | Ruta | Función |
|---|---|---|
| POST | `/api/credit-requests` | Crear una solicitud |
| GET | `/api/credit-requests` | Consultar todas las solicitudes |
| GET | `/api/credit-requests?status=Pending` | Filtrar por estado |
| GET | `/api/credit-requests/{id}` | Consultar una solicitud |
| PATCH | `/api/credit-requests/{id}/status` | Aprobar o rechazar |

Para crear una solicitud, envía este JSON:

```json
{
  "applicantId": "8-123-456",
  "amount": 5000,
  "termMonths": 24
}
```

El monto permitido es de 500 a 50000 USD y el plazo de 6 a 60 meses. La solicitud se crea pendiente.

Para aprobarla, reemplaza `{id}` por el identificador de la solicitud y envía:

```json
{
  "status": "Approved",
  "comment": "Solicitud revisada y aprobada."
}
```

Para rechazarla, usa `Rejected`. El comentario es obligatorio. El filtro admite `Pending`, `Approved` o `Rejected`; las respuestas representan esos estados como 0, 1 y 2.

## Compilar y ejecutar las pruebas

```sh
dotnet build
dotnet test Tests/BluecoreApi.Tests.csproj
```

Las tres pruebas verifican monto, plazo y comentario obligatorio. No requieren una base de datos activa.
