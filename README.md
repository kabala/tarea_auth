# AuthApp — App de autenticación con .NET 10 + Angular

Aplicación simple que demuestra autenticación end-to-end entre un backend
.NET 10 (Minimal APIs) y un frontend Angular 20.

## Stack

- **Backend:** .NET 10, Minimal APIs, EF Core (SQL Server / LocalDB), JWT en cookie HttpOnly
- **Frontend:** Angular 20 (standalone, signals), formularios reactivos, interceptores y guards
- **Tests:** xUnit + WebApplicationFactory

## Requisitos

- .NET SDK 10
- Node.js 24+ y npm
- SQL Server LocalDB (incluido con Visual Studio / SQL Server Express)

## Estructura del repositorio

```
tarea_auth/
├── AuthApp.sln
├── src/
│   ├── AuthAppApi/              # Backend .NET 10
│   │   ├── Domain/              # Entidades
│   │   ├── Application/         # Servicios, interfaces, DTOs, filtros
│   │   ├── Infrastructure/      # DbContext, repositorios, seeder
│   │   ├── Endpoints/           # Endpoints de auth y antiforgery
│   │   ├── Migrations/          # Migraciones de EF Core
│   │   └── Program.cs
│   └── AuthApp.Web/             # Frontend Angular 20
│       └── src/app/
│           ├── core/            # AuthService, interceptores, guards, modelos
│           └── features/        # Componentes de login y dashboard
├── tests/
│   └── AuthAppApiTests/         # Pruebas unitarias e de integración
├── docs/
│   └── AuthApp.http             # Colección de peticiones HTTP
└── README.md
```

## Puesta en marcha

### 1. Backend (.NET)

```bash
# Restaurar paquetes
dotnet restore

# Crear la base de datos y aplicar migraciones
dotnet ef database update --project src/AuthAppApi

# Ejecutar la API (http://localhost:5272)
dotnet run --project src/AuthAppApi
```

La base de datos se crea automáticamente en LocalDB. Al arrancar, la API
siembra dos usuarios de demostración:

| Usuario             | Contraseña    | Rol   |
|---------------------|---------------|-------|
| demo@example.com    | Password123!  | user  |
| admin@example.com   | Admin123!     | admin |

### 2. Frontend (Angular)

```bash
cd src/AuthApp.Web

# Instalar dependencias
npm install

# Ejecutar el servidor de desarrollo (http://localhost:4200)
npm start
```

El frontend usa un proxy (`proxy.conf.mjs`) que redirige `/api` a
`http://localhost:5272`, por lo que no hay problemas de CORS en desarrollo.

### 3. Probar la aplicación

1. Abrir `http://localhost:4200` en el navegador.
2. Serás redirigido a `/login`.
3. Inicia sesión con `demo@example.com` / `Password123!`.
4. Verás el dashboard con los datos del usuario.
5. Haz clic en "Cerrar sesión" para volver al login.

Para probar el código 403: inicia sesión como `demo@example.com` y pulsa
"Obtener datos de administrador". Como no es admin, recibirás un error 403.

## Variables de entorno

Toda la configuración está en `appsettings.json` y `appsettings.Development.json`.
Para producción, usa variables de entorno (prefijo `__` para jerarquías):

| Variable                             | Descripción                          | Valor por defecto (dev) |
|--------------------------------------|--------------------------------------|-------------------------|
| `ConnectionStrings__Default`         | Cadida de conexión a SQL Server      | LocalDB / AuthAppDb     |
| `Jwt__SigningKey`                    | Clave de firma del JWT (mín. 32 car.)| Clave de desarrollo     |
| `Jwt__Issuer`                        | Emisor del token                     | AuthApp                 |
| `Jwt__Audience`                      | Audiencia del token                  | AuthApp-Web             |
| `Jwt__ExpiryMinutes`                 | Duración del token en minutos        | 60 (dev) / 15 (prod)    |
| `Cookie__Secure`                     | Flag Secure de la cookie             | false (dev) / true      |
| `Cookie__SameSite`                   | Política SameSite                    | Lax                     |
| `Cors__AllowedOrigin`                | Origen permitido para CORS           | http://localhost:4200   |

**Importante:** nunca guardes la clave de firma en el código. En producción
usa variables de entorno, user-secrets o un gestor de secretos.

## Seguridad

- **Cookie HttpOnly:** el JWT se almacena en una cookie `auth_app_session` con
  `HttpOnly=true`, por lo que JavaScript no puede acceder al token.
- **Flags Secure y SameSite:** `Secure=true` en producción, `SameSite=Lax`.
- **Protección CSRF/XSRF:** la API usa antiforgery de ASP.NET Core. Angular lee
  la cookie `XSRF-TOKEN` (no HttpOnly) y la envía como header `X-XSRF-TOKEN` en
  peticiones POST/PUT/DELETE.
- **Regeneración de sesión:** cada login genera un JWT nuevo con un `jti`
  único y rota los tokens antiforgery.
- **withCredentials:** el interceptor `authInterceptor` activa
  `withCredentials: true` en todas las peticiones para enviar cookies.

## Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test
```

Las pruebas cubren:
- **Integración:** login válido (200), credenciales inválidas (401), campos
  vacíos (400), sin antiforgery token (400), `/me` sin sesión (401), `/me` con
  sesión (200), logout (200), endpoint admin con usuario normal (403), endpoint
  admin con admin (200).
- **Unitarias:** `PasswordHasher` (hash/verify), `TokenService` (claims, exp,
  jti único), `AuthService` (login válido/inválido/usuario inexistente).

Las pruebas de integración usan SQLite en memoria (sin tocar SQL Server).

## Archivos .http

En `docs/AuthApp.http` encontrarás peticiones listas para ejecutar desde VS
Code (extensión REST Client) o Visual Studio. Incluye todos los endpoints con
comentarios en español.
