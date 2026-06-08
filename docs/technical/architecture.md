# Architecture

Este documento describe la arquitectura general del sistema **CoWork Spaces Reservation System**.

---

## Vista general

```text
Frontend Angular
      |
      | HTTP / JSON
      v
ASP.NET Core Web API
      |
      | EF Core
      v
PostgreSQL
```

---

## Backend

El backend usa arquitectura por capas:

```text
Cowork.Api
Cowork.Application
Cowork.Domain
Cowork.Infrastructure
```

---

## Cowork.Api

Responsabilidades:

- Exponer endpoints REST.
- Configurar autenticación JWT.
- Configurar CORS.
- Configurar OpenAPI/Scalar.
- Manejar errores globales.
- Resolver el usuario actual.
- Recibir requests HTTP y delegar a servicios de aplicación.

---

## Cowork.Application

Responsabilidades:

- Implementar casos de uso.
- Coordinar reglas de negocio.
- Definir interfaces para infraestructura.
- Definir DTOs.
- Ejecutar validaciones de aplicación.
- Calcular precios.
- Calcular políticas de cancelación.
- Generar disponibilidad pública.
- Generar reportes.
- Registrar auditoría funcional.

---

## Cowork.Domain

Responsabilidades:

- Representar el modelo de dominio.
- Definir entidades.
- Definir enums.
- Encapsular reglas propias de entidades.
- Evitar dependencias de infraestructura.

---

## Cowork.Infrastructure

Responsabilidades:

- Acceso a PostgreSQL.
- Entity Framework Core.
- Configuración de entidades.
- Repositorios.
- Unit of Work.
- Hashing de contraseñas.
- Generación de JWT.
- Implementaciones técnicas de interfaces.

---

## Frontend

El frontend está construido con Angular y contiene:

- Portal público.
- Login.
- Panel administrativo.
- Guards por autenticación/rol.
- Interceptor para JWT.
- Manejo de errores.
- Visualización de disponibilidad por slots.
- Preview de precio antes de confirmar reserva.

---

## Base de datos

PostgreSQL se inicializa con scripts SQL versionados.

```text
database/
├── 000_extensions.sql
├── 001_catalogs.sql
├── 002_schema.sql
├── 003_indexes_constraints.sql
├── 004_seed.sql
└── 005_comments.sql
```

---

## Flujo de reserva pública

```text
1. Cliente selecciona espacio.
2. Cliente selecciona fecha.
3. Frontend consulta disponibilidad.
4. Backend devuelve slots de 30 minutos.
5. Cliente selecciona hora de inicio.
6. Frontend calcula duraciones válidas.
7. Frontend solicita preview de precio.
8. Backend calcula precio dinámico.
9. Cliente confirma reserva.
10. Backend valida reglas y solapamiento.
11. PostgreSQL garantiza que no exista overbooking.
12. Se crea reserva y se muestra código al cliente.
```

---

## Decisiones principales

- Separación por capas para mantener responsabilidades claras.
- El frontend guía al usuario, pero el backend siempre valida nuevamente.
- La garantía final contra doble reserva está en PostgreSQL.
- La reserva pública es tipo invitado para reducir fricción.

---

## Mejoras futuras

- Refresh token.
- Envío de correos.
- Pagos en línea.
- Workers para notificaciones.
- Serilog.
- CI/CD.
- Deploy cloud.
- Vista semanal tipo calendario.
