# CoWork Spaces Reservation System

Sistema full stack para la gestión de reservas de espacios de coworking.

El proyecto permite consultar espacios disponibles, registrar reservas públicas, calcular precios dinámicos, administrar espacios, clientes y reservas, controlar accesos por rol, generar reportes administrativos y auditar eventos funcionales del sistema.

---

## Tabla de contenido

- [Funcionalidades principales](#funcionalidades-principales)
- [Tecnologías utilizadas](#tecnologías-utilizadas)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Arquitectura general](#arquitectura-general)
- [Requisitos previos](#requisitos-previos)
- [Ejecución local](#ejecución-local)
- [Usuarios demo](#usuarios-demo)
- [Rutas principales](#rutas-principales)
- [Endpoints principales](#endpoints-principales)
- [Base de datos](#base-de-datos)
- [Reglas de negocio](#reglas-de-negocio)
- [Pruebas](#pruebas)
- [Colección manual de requests](#colección-manual-de-requests)
- [Documentación adicional](#documentación-adicional)
- [Mejoras futuras](#mejoras-futuras)

---

## Funcionalidades principales

### Portal público

- Visualización de espacios disponibles.
- Selección de espacio por capacidad, tarifa y horario.
- Registro de reserva pública tipo invitado.
- Cálculo previo del precio antes de confirmar.
- Confirmación visual con código de reserva.
- Indicaciones claras para el cliente después de reservar.

### Panel interno

- Login con JWT.
- Control de acceso por roles.
- Dashboard administrativo.
- Gestión de espacios.
- Gestión de clientes.
- Gestión de reservas.
- Cancelación de reservas.
- Finalización de reservas.
- Reportes administrativos.
- Auditoría funcional.

### Seguridad y reglas

- Autenticación JWT.
- Roles `Admin`, `Staff` y `Customer`.
- Guards en frontend.
- Interceptor para enviar token JWT.
- Validación de horarios en bloques de 30 minutos.
- Prevención de reservas solapadas.
- Manejo de errores HTTP.
- Auditoría de acciones relevantes.

---

## Tecnologías utilizadas

### Backend

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Bearer Authentication
- BCrypt
- xUnit
- Docker

### Frontend

- Angular
- TypeScript
- SCSS
- Angular Router
- Angular Signals
- HTTP Interceptors
- Route Guards

### Base de datos

- PostgreSQL 16
- Scripts SQL versionados
- Constraints
- Índices
- Seeds
- Comentarios técnicos
- Auditoría en tabla `audit_logs`

---

## Estructura del repositorio

```text
cowork-reservation-system/
├── backend/
│   ├── src/
│   │   ├── Cowork.Api/
│   │   ├── Cowork.Application/
│   │   ├── Cowork.Domain/
│   │   └── Cowork.Infrastructure/
│   │
│   └── tests/
│       ├── Cowork.UnitTests/
│       └── Cowork.IntegrationTests/
│
├── frontend/
│   └── cowork-web/
│
├── database/
│   ├── 000_extensions.sql
│   ├── 001_catalogs.sql
│   ├── 002_schema.sql
│   ├── 003_indexes_constraints.sql
│   ├── 004_seed.sql
│   ├── 005_comments.sql
│   └── README.md
│
├── docs/
│   ├── diagrams/
│   ├── requests/
│   │   └── requests.http
│   └── technical/
│
├── docker-compose.yml
└── README.md
```

---

## Arquitectura general

El proyecto está separado en backend, frontend, base de datos y documentación.

### Backend

El backend usa una arquitectura por capas:

```text
Cowork.Api
Cowork.Application
Cowork.Domain
Cowork.Infrastructure
```

### `Cowork.Api`

Contiene:

- Controllers REST.
- Configuración de autenticación JWT.
- Configuración de CORS.
- Middleware global de errores.
- Configuración de OpenAPI/Scalar.
- Servicio de usuario actual.

### `Cowork.Application`

Contiene:

- Servicios de aplicación.
- Casos de uso.
- DTOs.
- Interfaces.
- Reglas coordinadas de negocio.
- Cálculo de precios.
- Política de cancelación.
- Auditoría funcional.
- Reportes.

### `Cowork.Domain`

Contiene:

- Entidades.
- Enums.
- Reglas reutilizables del dominio.
- Validaciones internas de entidades.

### `Cowork.Infrastructure`

Contiene:

- DbContext.
- Configuraciones de Entity Framework Core.
- Repositorios.
- Unit of Work.
- Generación de JWT.
- Hashing de contraseñas.
- Acceso a PostgreSQL.

---

## Requisitos previos

Para ejecutar el proyecto localmente se requiere:

- Docker Desktop.
- .NET SDK 10.
- Node.js.
- npm.
- Git.

Opcional:

- Visual Studio 2026, Visual Studio Code o Rider.
- Extensión REST Client para probar `docs/requests/requests.http`.

---

## Ejecución local

### 1. Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd cowork-reservation-system
```

---

### 2. Levantar PostgreSQL con Docker

Desde la raíz del repositorio:

```bash
docker compose up -d
```

Esto crea el contenedor PostgreSQL y ejecuta automáticamente los scripts ubicados en:

```text
database/
```

La base de datos queda inicializada con catálogos, usuarios demo, clientes demo, espacios demo y datos necesarios para la ejecución local.

---

### 3. Verificar contenedor de PostgreSQL

```bash
docker ps
```

También se puede validar la creación de tablas con:

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "\dt"
```

---

### 4. Ejecutar backend

Desde la raíz del repositorio:

```bash
dotnet run --project backend/src/Cowork.Api/Cowork.Api.csproj
```

La API queda disponible en:

```text
https://localhost:7011
```

Documentación interactiva:

```text
https://localhost:7011/scalar/v1
```

OpenAPI JSON:

```text
https://localhost:7011/openapi/v1.json
```

---

### 5. Ejecutar frontend

Desde la raíz del repositorio:

```bash
cd frontend/cowork-web
npm install
npm start
```

Si no existe script `start`, usar:

```bash
npx ng serve
```

El frontend queda disponible en:

```text
http://localhost:4200
```

---

## Reiniciar base de datos local

Para eliminar el volumen y reconstruir la base de datos desde cero:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

Esto borra los datos locales y vuelve a ejecutar los scripts SQL.

---

## Usuarios demo

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `staff01` | `Admin123!` | Staff |
| `juan.perez` | `Admin123!` | Customer |

Las credenciales demo son solo para entorno local de desarrollo.

---

## Rutas principales

### Portal público

```text
/public/spaces
/public/reservation
```

### Autenticación

```text
/auth/login
```

### Panel interno

```text
/admin/dashboard
/admin/spaces
/admin/customers
/admin/reservations
/admin/reports
/admin/audit-logs
/forbidden
```

---

## Permisos por rol

| Funcionalidad | Admin | Staff | Customer |
|---|---:|---:|---:|
| Login | Sí | Sí | Sí |
| Portal público | Sí | Sí | Sí |
| Ver dashboard | Sí | Sí | No |
| Gestionar espacios | Sí | Sí | No |
| Gestionar clientes | Sí | Sí | No |
| Ver reservas | Todas | Todas | Solo propias |
| Crear reservas internas | Sí | Sí | Sí |
| Cancelar reservas | Sí | Sí | Solo propias |
| Completar reservas | Sí | Sí | No |
| Ver reportes | Sí | Sí | No |
| Ver auditoría | Sí | Sí | No |

---

## Flujo público de reserva

El portal público permite crear reservas como invitado.

Flujo:

```text
1. El cliente selecciona un espacio.
2. Ingresa sus datos de contacto.
3. Define inicio y fin de la reserva.
4. Calcula el precio estimado.
5. Confirma la reserva.
6. El sistema muestra un código de reserva.
```

Después de crear la reserva, el cliente debe guardar su código. Ese código permite que el personal ubique la reserva desde el panel interno.

Actualmente el portal público no crea automáticamente una cuenta de usuario para el cliente. La gestión interna queda a cargo de `Admin` y `Staff`.

---

## Endpoints principales

### Auth

```http
POST /api/auth/login
GET  /api/auth/me
```

### Público

```http
GET  /api/public/spaces
POST /api/public/pricing/preview
POST /api/public/reservations
```

### Espacios

```http
GET    /api/spaces
GET    /api/spaces/{id}
POST   /api/spaces
PUT    /api/spaces/{id}
DELETE /api/spaces/{id}
```

### Clientes

```http
GET    /api/customers
GET    /api/customers/{id}
POST   /api/customers
PUT    /api/customers/{id}
DELETE /api/customers/{id}
```

### Reservas

```http
GET  /api/reservations
GET  /api/reservations/{id}
POST /api/reservations
POST /api/reservations/{id}/cancel
POST /api/reservations/{id}/complete
```

### Reportes

```http
GET /api/reports
GET /api/reports?from=2026-06-01T00:00:00Z&to=2026-06-30T23:59:59Z
```

### Auditoría

```http
GET /api/audit-logs
```

---

## Base de datos

La base de datos se inicializa desde scripts SQL ubicados en:

```text
database/
```

Archivos principales:

| Archivo | Descripción |
|---|---|
| `000_extensions.sql` | Extensiones y funciones compartidas |
| `001_catalogs.sql` | Catálogos base |
| `002_schema.sql` | Tablas principales |
| `003_indexes_constraints.sql` | Índices, constraints y triggers |
| `004_seed.sql` | Datos iniciales y usuarios demo |
| `005_comments.sql` | Comentarios técnicos de base de datos |

Más detalle en:

```text
database/README.md
```

---

## Modelo general de datos

Entidades principales:

- `app_users`
- `customers`
- `spaces`
- `reservations`
- `audit_logs`

Catálogos principales:

- `app_user_roles`
- `app_user_statuses`
- `space_statuses`
- `reservation_statuses`

---

## Reglas de negocio

### Reservas

- La fecha/hora de inicio debe ser menor que la fecha/hora de fin.
- La reserva debe estar dentro del horario de atención del espacio.
- La reserva debe iniciar y finalizar el mismo día local.
- La duración mínima es de 30 minutos.
- La duración máxima es de 8 horas.
- Los horarios deben usar bloques de 30 minutos.
- No se permiten reservas solapadas para el mismo espacio.
- No se pueden reservar espacios inactivos o en mantenimiento.

### Espacios

- El nombre es obligatorio.
- La capacidad debe ser mayor a cero.
- La tarifa por hora debe ser mayor a cero.
- La hora de apertura debe ser menor que la hora de cierre.
- Los horarios de apertura y cierre deben usar bloques de 30 minutos.
- La zona horaria es obligatoria.

### Cancelaciones

La política de cancelación calcula el reembolso según la anticipación:

| Anticipación | Reembolso |
|---|---:|
| Más de 48 horas | 100% |
| Entre 24 y 48 horas | 50% |
| Menos de 24 horas | 0% |
| Después del inicio de la reserva | 0% |

---

## Tarifas dinámicas

El motor de tarifas aplica reglas en orden:

1. Precio base = tarifa por hora × duración.
2. Recargo por hora pico.
3. Recargo por fin de semana.
4. Descuento por reserva larga.
5. Descuento por reserva anticipada.

El resultado se almacena en la reserva mediante `pricing_breakdown`, permitiendo trazabilidad del cálculo aplicado.

---

## Prevención de overbooking

El sistema previene reservas solapadas para el mismo espacio y horario.

La estrategia combina:

- Validaciones en backend.
- Restricciones a nivel de base de datos.
- Manejo de conflicto en API.
- Respuesta HTTP `409 Conflict`.
- Prueba de integración para concurrencia.

Ejemplo de respuesta:

```json
{
  "status": 409,
  "error": "Reservation conflict",
  "message": "The selected time slot is already reserved for this space."
}
```

---

## Auditoría

La auditoría funcional se almacena en:

```text
audit_logs
```

Registra eventos como:

- Login exitoso.
- Login fallido.
- Creación de espacio.
- Actualización de espacio.
- Eliminación lógica de espacio.
- Creación de cliente.
- Actualización de cliente.
- Eliminación lógica de cliente.
- Creación de reserva.
- Cancelación de reserva.
- Finalización de reserva.

La auditoría permite identificar:

- Qué acción ocurrió.
- Sobre qué entidad ocurrió.
- Quién ejecutó la acción.
- Cuándo ocurrió.
- Qué valores cambiaron.

---

## Reportes

El dashboard administrativo muestra indicadores como:

- Total de reservas.
- Reservas pendientes.
- Reservas confirmadas.
- Reservas canceladas.
- Reservas completadas.
- Ingresos totales.
- Total reembolsado.
- Espacio más reservado.
- Hora más demandada.
- Reservas por espacio.
- Demanda por hora.

Endpoint:

```http
GET /api/reports
```

---

## Pruebas

Para ejecutar las pruebas:

```bash
cd backend
dotnet test
```

Las pruebas cubren:

- Motor de tarifas dinámicas.
- Política de cancelación.
- Concurrencia en creación de reservas.
- Prevención de overbooking.
- Respuesta esperada `409 Conflict`.

---

## Validación de compilación

### Backend

```bash
cd backend
dotnet build
dotnet test
```

### Frontend

```bash
cd frontend/cowork-web
npm install
npx ng build
```

---

## Colección manual de requests

La colección manual está en:

```text
docs/requests/requests.http
```

Incluye pruebas para:

- Login.
- Endpoints públicos.
- Endpoints protegidos.
- CRUD de espacios.
- CRUD de clientes.
- Gestión de reservas.
- Cancelación.
- Finalización.
- Validaciones.
- Overbooking.
- Auditoría.
- Reportes.
- Restricciones por rol.

Puede ejecutarse con:

- Visual Studio Code + REST Client.
- Visual Studio.
- Rider.

---

## Manejo de errores

La API responde con códigos HTTP consistentes:

| Caso | Código |
|---|---:|
| Datos inválidos | 400 |
| No autenticado | 401 |
| Sin permisos | 403 |
| Recurso no encontrado | 404 |
| Conflicto de reserva o duplicado | 409 |
| Error inesperado | 500 |

El frontend muestra mensajes visuales en español para el usuario final.

---

## Configuración del frontend

La URL base de la API se configura en:

```text
frontend/cowork-web/src/environments/environment.ts
```

Ejemplo local:

```ts
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:7011/api'
};
```

---

## Configuración de Docker

El archivo:

```text
docker-compose.yml
```

levanta PostgreSQL en el puerto local:

```text
5433
```

Credenciales locales:

```text
Database: cowork_reservations
User: cowork_user
Password: cowork_password
```

Estas credenciales son solo para entorno local.

---

## Documentación adicional

```text
database/README.md
frontend/cowork-web/README.md
docs/requests/requests.http
docs/technical/local-setup.md
docs/technical/future-improvements.md
docs/diagrams/
```

---

## Comandos rápidos

Levantar base de datos:

```bash
docker compose up -d
```

Ejecutar backend:

```bash
dotnet run --project backend/src/Cowork.Api/Cowork.Api.csproj
```

Ejecutar frontend:

```bash
cd frontend/cowork-web
npm install
npm start
```

Ejecutar pruebas:

```bash
cd backend
dotnet test
```

Reiniciar base de datos:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

---

## Mejoras futuras

Mejoras previstas para una siguiente versión:

- Registro de cuenta para clientes.
- Consulta pública de reserva por código y correo.
- Envío de confirmación por email.
- Refresh token.
- Recuperación de contraseña.
- Pagos en línea.
- Serilog para logging técnico.
- Dapper para reportes complejos.
- Vistas SQL para dashboards avanzados.
- Endpoints PATCH para actualizaciones parciales.
- Tests end-to-end.
- CI/CD.
- Despliegue cloud.

---

## Estado actual

Estado del proyecto:

- Base de datos inicializable con Docker.
- Backend funcional.
- Frontend funcional.
- Portal público implementado.
- Login JWT implementado.
- Roles implementados.
- Panel interno implementado.
- Gestión de espacios implementada.
- Gestión de clientes implementada.
- Gestión de reservas implementada.
- Reportes implementados.
- Auditoría implementada.
- Pruebas unitarias e integración disponibles.
- Colección HTTP documentada.