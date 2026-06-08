# CoWork Spaces Reservation System

Sistema full stack para la gestión de reservas de espacios de coworking.

El proyecto permite administrar espacios, clientes, reservas, tarifas dinámicas, autenticación con JWT, roles, auditoría de negocio, reportes administrativos y prevención de reservas solapadas mediante restricciones a nivel de base de datos.

---

## Funcionalidades principales

- Portal público para consultar espacios disponibles.
- Creación de reservas públicas sin login.
- Cálculo de precio estimado antes de reservar.
- Autenticación JWT.
- Roles `Admin`, `Staff` y `Customer`.
- Panel interno para administración.
- Gestión de espacios.
- Gestión de clientes.
- Gestión de reservas.
- Cancelación de reservas con política de reembolso.
- Finalización de reservas.
- Reportes administrativos.
- Auditoría de eventos funcionales.
- Prevención de overbooking.
- Pruebas unitarias e integración.
- Colección manual de requests HTTP para validación de API.

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
- Angular Signals
- Angular Router
- HTTP Interceptors
- Route Guards

### Base de datos

- PostgreSQL 16
- Scripts SQL versionados
- Constraints
- Índices
- Seeds
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

## Arquitectura del backend

El backend está organizado por capas:

```text
Cowork.Api
Cowork.Application
Cowork.Domain
Cowork.Infrastructure
```

### `Cowork.Api`

Contiene:

- Controllers
- Configuración HTTP
- Middleware de errores
- Autenticación JWT
- Configuración de CORS
- Scalar/OpenAPI
- Current user context

### `Cowork.Application`

Contiene:

- Casos de uso
- Servicios de aplicación
- DTOs
- Interfaces
- Reglas de negocio coordinadas
- Servicios de pricing, cancelación, auditoría y reportes

### `Cowork.Domain`

Contiene:

- Entidades
- Enums
- Reglas base del dominio
- Validaciones internas de entidades

### `Cowork.Infrastructure`

Contiene:

- DbContext
- Configuraciones EF Core
- Repositorios
- Unit of Work
- JWT token generator
- Password hasher
- Implementaciones de infraestructura

---

## Base de datos

La base de datos se inicializa desde scripts SQL ubicados en:

```text
database/
```

Incluye:

- Catálogos de roles y estados.
- Usuarios demo.
- Clientes demo.
- Espacios demo.
- Reservas.
- Auditoría.
- Índices.
- Constraints.
- Comentarios técnicos.
- Prevención de reservas solapadas.

Más detalle en:

```text
database/README.md
```

---

## Autenticación y roles

El sistema usa JWT Bearer Authentication.

Usuarios demo:

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `staff01` | `Admin123!` | Staff |
| `juan.perez` | `Admin123!` | Customer |

> Credenciales solo para entorno local de desarrollo.

### Permisos por rol

| Funcionalidad | Admin | Staff | Customer |
|---|---:|---:|---:|
| Login | Sí | Sí | Sí |
| Ver espacios públicos | Sí | Sí | Sí |
| Crear reserva pública | Sí | Sí | Sí |
| Gestionar espacios | Sí | Sí | No |
| Gestionar clientes | Sí | Sí | No |
| Ver reservas | Todas | Todas | Solo propias |
| Crear reserva interna | Sí | Sí | Sí |
| Cancelar reserva | Sí | Sí | Solo propias |
| Completar reserva | Sí | Sí | No |
| Ver reportes | Sí | Sí | No |
| Ver auditoría | Sí | Sí | No |

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

### Auditoría

```http
GET /api/audit-logs
```

### Reportes

```http
GET /api/reports
GET /api/reports?from=2026-06-01T00:00:00Z&to=2026-06-30T23:59:59Z
```

---

## Frontend

El frontend está desarrollado en Angular y consume los endpoints públicos e internos del backend.

Incluye:

- Portal público de espacios.
- Reserva pública.
- Preview de precio.
- Login.
- Panel administrativo.
- Menú responsive.
- Guards por autenticación y rol.
- Interceptor JWT.
- Gestión de espacios.
- Gestión de clientes.
- Gestión de reservas.
- Dashboard administrativo.
- Reportes.
- Auditoría.
- Página de acceso restringido.

Rutas principales:

```text
/public/spaces
/public/reservation
/auth/login
/admin/dashboard
/admin/spaces
/admin/customers
/admin/reservations
/admin/reports
/admin/audit-logs
/forbidden
```

Más detalle en:

```text
frontend/cowork-web/README.md
```

---

## Concurrencia y overbooking

La prevención de reservas solapadas se garantiza principalmente a nivel de base de datos mediante una `exclusion constraint` de PostgreSQL.

Esto evita que dos solicitudes simultáneas puedan confirmar reservas para el mismo espacio y horario.

Cuando ocurre un conflicto, la API responde:

```http
409 Conflict
```

Respuesta esperada:

```json
{
  "status": 409,
  "error": "Reservation conflict",
  "message": "The selected time slot is already reserved for this space."
}
```

La estrategia combina:

- Restricción fuerte en PostgreSQL.
- Manejo de excepción en backend.
- Respuesta HTTP consistente.
- Prueba de integración para validar concurrencia real.

---

## Tarifas dinámicas

El motor de tarifas calcula el precio de una reserva aplicando reglas en orden:

1. Precio base = tarifa por hora × duración.
2. Recargo por hora pico.
3. Recargo por fin de semana.
4. Descuento por reserva larga.
5. Descuento por reserva anticipada.

El resultado se guarda en la reserva mediante `pricing_breakdown`, permitiendo trazabilidad del cálculo aplicado.

---

## Política de cancelación

La política de cancelación calcula el reembolso según la anticipación:

| Anticipación | Reembolso |
|---|---:|
| Más de 48 horas | 100% |
| Entre 24 y 48 horas | 50% |
| Menos de 24 horas | 0% |
| Después del inicio de la reserva | 0% |

---

## Auditoría

La auditoría de negocio se almacena en la tabla:

```text
audit_logs
```

Registra eventos como:

- Login exitoso.
- Login fallido.
- Creación de cliente.
- Actualización de cliente.
- Eliminación lógica de cliente.
- Creación de espacio.
- Actualización de espacio.
- Eliminación lógica de espacio.
- Creación de reserva.
- Cancelación de reserva.
- Finalización de reserva.

La auditoría permite responder preguntas como:

- Qué acción ocurrió.
- Sobre qué entidad ocurrió.
- Quién ejecutó la acción.
- Cuándo ocurrió.
- Qué valores cambiaron.

---

## Reportes

El sistema expone un dashboard administrativo con:

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

## Ejecución local

### 1. Levantar PostgreSQL

Desde la raíz del repositorio:

```bash
docker compose up -d
```

Esto crea el contenedor PostgreSQL y ejecuta los scripts de inicialización de la carpeta `database/`.

### 2. Ejecutar backend

```bash
dotnet run --project backend/src/Cowork.Api/Cowork.Api.csproj
```

Por defecto, la API se ejecuta en:

```text
https://localhost:7011
```

Scalar/OpenAPI:

```text
https://localhost:7011/scalar/v1
```

### 3. Ejecutar frontend

```bash
cd frontend/cowork-web
npm install
ng serve
```

Abrir:

```text
http://localhost:4200
```

---

## Reiniciar base de datos local

Para restaurar la base al estado inicial del seed:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

Esto elimina el volumen local y vuelve a ejecutar los scripts SQL.

---

## Pruebas automatizadas

Ejecutar pruebas:

```bash
cd backend
dotnet test
```

Las pruebas cubren:

- Motor de tarifas dinámicas.
- Política de cancelaciones.
- Concurrencia real en creación de reservas.
- Prevención de overbooking.
- Respuesta esperada `409 Conflict`.

---

## Colección manual de requests

La colección manual de pruebas está en:

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

Este archivo puede ejecutarse con:

- VS Code + extensión REST Client.
- Visual Studio.
- Rider.

---

## Validaciones principales

El sistema valida:

- Espacio obligatorio.
- Cliente obligatorio.
- Fecha/hora de inicio menor que fecha/hora de fin.
- Duración mínima de reserva.
- Duración máxima de reserva.
- Horario dentro de apertura y cierre del espacio.
- Espacio activo para permitir reservas.
- Usuario autenticado para endpoints internos.
- Rol permitido para acciones administrativas.
- Prevención de reservas solapadas.

---

## Manejo de errores

La API maneja errores de forma consistente:

| Caso | HTTP |
|---|---:|
| Datos inválidos | 400 |
| No autenticado | 401 |
| Sin permisos | 403 |
| Recurso no encontrado | 404 |
| Conflicto de reserva / duplicado | 409 |
| Error inesperado | 500 |

---

## Documentación adicional

```text
database/README.md
frontend/cowork-web/README.md
docs/requests/requests.http
docs/technical/local-setup.md
docs/technical/future-improvements.md
docs/diagrams/mermaid/database-er.md
```

---

## Mejoras futuras

Algunas mejoras previstas:

- Refresh token.
- Serilog para logs técnicos.
- Dapper para reportes complejos.
- Vistas SQL para dashboards avanzados.
- Endpoints PATCH para actualizaciones parciales.
- Componentes reutilizables en frontend.
- Manejo de errores frontend por código de negocio.
- Tests end-to-end.
- CI/CD.
- Despliegue cloud.

---

## Estado del proyecto

Estado actual:

- Backend funcional.
- Base de datos inicializable con Docker.
- Autenticación JWT implementada.
- Roles implementados.
- Endpoints públicos implementados.
- Endpoints administrativos implementados.
- Auditoría implementada.
- Reportes implementados.
- Frontend funcional con Angular.
- Pruebas unitarias e integración disponibles.
- Colección HTTP documentada para validación manual.