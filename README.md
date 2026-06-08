# CoWork Spaces Reservation System

Sistema full stack para la gestión de reservas de espacios de coworking.

El proyecto permite consultar espacios disponibles, visualizar disponibilidad por fecha, registrar reservas públicas, calcular precios dinámicos antes de confirmar, administrar espacios, clientes y reservas, generar reportes administrativos, validar concurrencia para evitar doble reserva y auditar eventos funcionales del sistema.

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
- [Flujo público de reserva](#flujo-público-de-reserva)
- [Endpoints principales](#endpoints-principales)
- [Base de datos](#base-de-datos)
- [Reglas de negocio](#reglas-de-negocio)
- [Disponibilidad por espacio](#disponibilidad-por-espacio)
- [Tarifas dinámicas](#tarifas-dinámicas)
- [Política de cancelación](#política-de-cancelación)
- [Prevención de overbooking y concurrencia](#prevención-de-overbooking-y-concurrencia)
- [Índices para disponibilidad](#índices-para-disponibilidad)
- [Reportes](#reportes)
- [Auditoría](#auditoría)
- [Pruebas](#pruebas)
- [Colección manual de requests](#colección-manual-de-requests)
- [Validación final](#validación-final)
- [Mejoras futuras](#mejoras-futuras)

---

## Funcionalidades principales

### Portal público

- Visualización de espacios disponibles.
- Selección de espacio por capacidad, tarifa y horario.
- Consulta de disponibilidad por espacio y fecha.
- Visualización de bloques libres y ocupados en intervalos de 30 minutos.
- Selección de hora de inicio desde bloques disponibles.
- Selección de duración válida sin cruzar horarios ocupados.
- Cálculo automático del precio antes de confirmar.
- Registro de reserva pública tipo invitado.
- Confirmación visual con código de reserva.
- Feedback claro para el cliente después de reservar.

### Panel interno

- Login con JWT.
- Control de acceso por roles.
- Dashboard administrativo.
- Gestión de espacios.
- Gestión de clientes.
- Gestión de reservas.
- Creación de reservas internas con preview de precio.
- Cancelación de reservas.
- Finalización de reservas.
- Reportes administrativos.
- Auditoría funcional.

### Seguridad y reglas

- Autenticación JWT.
- Roles `Admin`, `Staff` y `Customer`.
- Guards en frontend.
- Interceptor HTTP para enviar token JWT.
- Validación de horarios en bloques de 30 minutos.
- Validación de disponibilidad antes de crear reservas.
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
- Formularios con `ngModel`

### Base de datos

- PostgreSQL 16
- Scripts SQL versionados
- Constraints
- Índices
- Seeds
- Comentarios técnicos
- Auditoría en tabla `audit_logs`
- Restricción contra reservas solapadas

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

El proyecto está dividido en backend, frontend, base de datos y documentación.

### Backend

El backend utiliza una arquitectura por capas:

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
- Endpoints públicos y protegidos.

### `Cowork.Application`

Contiene:

- Servicios de aplicación.
- Casos de uso.
- DTOs.
- Interfaces.
- Reglas coordinadas de negocio.
- Cálculo de precios.
- Política de cancelación.
- Disponibilidad pública por slots.
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

- Visual Studio.
- Visual Studio Code.
- Rider.
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

La base de datos queda inicializada con:

- Catálogos.
- Usuarios demo.
- Clientes demo.
- Espacios demo.
- Reservas demo.
- Datos necesarios para la ejecución local.

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

El portal público permite crear reservas como invitado, sin crear una cuenta previamente.

Flujo implementado:

```text
1. El cliente selecciona un espacio.
2. Selecciona una fecha.
3. El sistema consulta la disponibilidad del espacio para ese día.
4. El cliente visualiza bloques libres y ocupados en intervalos de 30 minutos.
5. El cliente selecciona una hora de inicio disponible.
6. El sistema muestra duraciones válidas según los bloques libres consecutivos.
7. El cliente selecciona la duración.
8. El sistema calcula automáticamente el precio estimado.
9. El cliente confirma la reserva.
10. El sistema muestra el código de reserva.
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
GET  /api/public/spaces/{spaceId}/availability?date=2026-06-08
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
| `003_indexes_constraints.sql` | Índices, constraints y restricciones |
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
- Las reservas canceladas no bloquean disponibilidad.
- La disponibilidad mostrada en frontend se vuelve a validar en backend antes de crear la reserva.

### Espacios

- El nombre es obligatorio.
- La capacidad debe ser mayor a cero.
- La tarifa por hora debe ser mayor a cero.
- La hora de apertura debe ser menor que la hora de cierre.
- Los horarios de apertura y cierre deben usar bloques de 30 minutos.
- La zona horaria es obligatoria.

### Clientes

- El nombre completo es obligatorio.
- El correo debe tener formato válido.
- El teléfono debe cumplir longitud mínima.
- Los datos del cliente permiten registrar reservas públicas tipo invitado.

---

## Disponibilidad por espacio

El sistema incluye un endpoint público de disponibilidad:

```http
GET /api/public/spaces/{spaceId}/availability?date=2026-06-08
```

Este endpoint:

- Recibe el espacio y la fecha.
- Obtiene el horario de apertura y cierre del espacio.
- Genera bloques de 30 minutos.
- Consulta reservas existentes del espacio en ese día.
- Marca cada bloque como disponible u ocupado.
- Ignora reservas canceladas.
- Devuelve la lista de bloques al frontend.

Respuesta esperada:

```json
{
  "spaceId": "00000000-0000-0000-0000-000000000001",
  "spaceName": "Oficina Privada",
  "date": "2026-06-08",
  "openingTime": "08:00:00",
  "closingTime": "18:00:00",
  "timeZoneId": "America/Lima",
  "slots": [
    {
      "startTime": "08:00:00",
      "endTime": "08:30:00",
      "isAvailable": true
    },
    {
      "startTime": "08:30:00",
      "endTime": "09:00:00",
      "isAvailable": false
    }
  ],
  "reservedSlots": [
    {
      "startTime": "08:30:00",
      "endTime": "09:00:00"
    }
  ]
}
```

En el frontend, el cliente no escribe manualmente la hora de inicio y fin. Selecciona un bloque libre y luego una duración válida. El sistema calcula internamente `startTime` y `endTime`.

---

## Tarifas dinámicas

El motor de tarifas calcula el precio de la reserva antes de confirmar.

El flujo de cálculo es:

```text
Precio base = tarifa por hora × duración
```

Luego aplica reglas en orden:

```text
1. Recargo por hora pico
2. Recargo por fin de semana
3. Descuento por reserva larga
4. Descuento por reserva anticipada
```

### Reglas aplicadas

| Regla | Condición | Ajuste |
|---|---|---:|
| Hora pico | Reserva cruza 09:00-11:00 o 17:00-19:00 | +25% |
| Fin de semana | Reserva inicia sábado o domingo | +15% |
| Reserva larga | Duración mayor o igual a 4 horas | -10% |
| Reserva anticipada | Reserva creada al menos 7 días antes | -5% |

El resultado se almacena en la reserva mediante `pricing_breakdown`, permitiendo trazabilidad del cálculo aplicado.

Ejemplo:

```text
Tarifa por hora: S/ 50
Duración: 1 h 30 min
Subtotal base: S/ 75
Recargo hora pico +25%: S/ 18.75
Total estimado: S/ 93.75
```

---

## Política de cancelación

La política de cancelación calcula el reembolso según la anticipación:

| Anticipación | Reembolso |
|---|---:|
| Más de 48 horas | 100% |
| Entre 24 y 48 horas | 50% |
| Menos de 24 horas | 0% |
| Después del inicio de la reserva | 0% |

Cuando se cancela una reserva:

- Cambia su estado a `Cancelled`.
- Se calcula el reembolso según la política.
- Se registra auditoría.
- La reserva cancelada deja de bloquear disponibilidad.

---

## Prevención de overbooking y concurrencia

El sistema previene doble reserva usando una estrategia combinada.

### 1. Validación previa en backend

Antes de crear una reserva, el backend consulta si existe una reserva activa que se solape con el mismo espacio y rango horario.

La condición de solapamiento usada es:

```text
existing.StartTime < requested.EndTime
AND existing.EndTime > requested.StartTime
```

Esto permite casos borde válidos como:

```text
10:00 - 11:00
11:00 - 12:00
```

pero bloquea casos como:

```text
10:00 - 11:00
10:30 - 11:30
```

### 2. Restricción en PostgreSQL

La base de datos mantiene una restricción para impedir reservas superpuestas a nivel persistencia.

Esto protege el sistema incluso si dos requests llegan al mismo tiempo y pasan la validación previa.

### 3. Manejo como conflicto HTTP

Si se detecta un solapamiento, la API responde como conflicto:

```http
409 Conflict
```

Ejemplo:

```json
{
  "status": 409,
  "error": "Reservation conflict",
  "message": "The selected time slot is already reserved for this space."
}
```

### 4. Prueba de integración

Existe una prueba de integración que simula dos requests concurrentes intentando reservar el mismo espacio y horario. El comportamiento esperado es:

```text
Una reserva se crea correctamente.
La otra falla con conflicto 409.
```

---

## Índices para disponibilidad

Para optimizar la consulta de disponibilidad se consideran los siguientes campos:

- `space_id`
- `start_time`
- `end_time`
- `status`
- rango temporal de reserva

Estos índices ayudan a resolver rápidamente preguntas como:

```text
¿Qué reservas existen para este espacio entre la apertura y cierre de esta fecha?
```

La consulta principal usada para disponibilidad y solapamiento busca reservas activas que cumplan:

```text
space_id = espacio seleccionado
start_time < fin del rango
end_time > inicio del rango
status != Cancelled
```

La restricción de exclusión en PostgreSQL actúa como garantía final contra overbooking.

---

## Reportes

El endpoint de reportes devuelve información administrativa dentro de un rango de fechas.

Endpoint:

```http
GET /api/reports
GET /api/reports?from=2026-06-01T00:00:00Z&to=2026-06-30T23:59:59Z
```

El reporte incluye:

- Total de reservas.
- Reservas pendientes.
- Reservas confirmadas.
- Reservas canceladas.
- Reservas completadas.
- Ingresos totales.
- Total reembolsado.
- Ingresos por espacio.
- Tasa de ocupación por espacio.
- Espacio más reservado.
- Hora más demandada.
- Demanda por hora.
- Reservas por espacio.

La tasa de ocupación se calcula como:

```text
ocupación (%) =
minutos reservados no cancelados dentro del rango
/
minutos disponibles del espacio dentro del rango
× 100
```

El frontend muestra visualizaciones con barras para:

- Ocupación por espacio.
- Ingresos por espacio.
- Reservas por espacio.
- Demanda por hora.

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

## Pruebas

Para ejecutar las pruebas:

```bash
cd backend
dotnet test
```

Las pruebas cubren:

### Motor de tarifas dinámicas

- Tarifa base simple.
- Recargo por hora pico.
- Recargo por fin de semana.
- Descuento por reserva larga.
- Descuento por reserva anticipada.
- Combinación de reglas en orden esperado.
- Validación de inicio mayor o igual al fin.
- Validación de duración menor a 30 minutos.
- Validación de duración mayor a 8 horas.

### Política de cancelación

- Reembolso completo con más de 48 horas.
- Reembolso del 50% entre 24 y 48 horas.
- Sin reembolso con menos de 24 horas.
- Sin reembolso después del inicio de la reserva.
- Redondeo de monto de reembolso.

### Integración y concurrencia

- Dos requests concurrentes para el mismo espacio y horario.
- Solo una reserva debe crearse.
- La otra debe fallar con `409 Conflict`.

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
- Disponibilidad pública por espacio y fecha.
- Preview de precio.
- Creación de reserva pública.
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

Ejemplo de disponibilidad pública:

```http
GET https://localhost:7011/api/public/spaces/00000000-0000-0000-0000-000000000001/availability?date=2026-06-08
Accept: application/json
```

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

## Validación final

Antes de entregar o revisar el proyecto desde cero:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

Luego:

```bash
cd backend
dotnet build
dotnet test
cd ..
```

Luego:

```bash
cd frontend/cowork-web
npm install
npx ng build
cd ../..
```

Prueba manual recomendada:

```text
1. Iniciar sesión como admin.
2. Revisar espacios.
3. Crear o editar un espacio.
4. Entrar al portal público.
5. Seleccionar espacio y fecha.
6. Ver slots disponibles y ocupados.
7. Seleccionar hora de inicio y duración.
8. Confirmar reserva pública.
9. Guardar el código de reserva.
10. Volver a consultar disponibilidad y verificar que el horario quedó ocupado.
11. Intentar reservar el mismo horario y validar conflicto.
12. Revisar la reserva en el panel interno.
13. Cancelar o completar una reserva.
14. Revisar reportes.
15. Revisar auditoría.
```

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
- Vista semanal tipo calendario.
- Bloqueo temporal de slots mientras el usuario confirma.
- Notificaciones por correo al crear o cancelar reservas.

---

## Estado actual

Estado del proyecto:

- Base de datos inicializable con Docker.
- Backend funcional.
- Frontend funcional.
- Portal público implementado.
- Disponibilidad pública por slots implementada.
- Preview de precio implementado.
- Login JWT implementado.
- Roles implementados.
- Panel interno implementado.
- Gestión de espacios implementada.
- Gestión de clientes implementada.
- Gestión de reservas implementada.
- Reportes implementados.
- Tasa de ocupación por espacio implementada.
- Auditoría implementada.
- Pruebas unitarias e integración disponibles.
- Colección HTTP documentada.