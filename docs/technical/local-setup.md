# Local Setup

Guía rápida para levantar y validar el entorno local del sistema **CoWork Spaces**.

---

## Requisitos

- Docker Desktop
- .NET 10 SDK
- Node.js
- Angular CLI
- Git

---

## Levantar PostgreSQL con Docker

Desde la raíz del repositorio:

```bash
docker compose up -d
```

Verificar contenedor:

```bash
docker ps
```

Ver logs:

```bash
docker logs cowork-postgres
```

---

## Reiniciar la base de datos desde cero

Usar este comando cuando cambien los scripts SQL de la carpeta `database/`.

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

---

## Validar scripts montados en el contenedor

```bash
docker exec -it cowork-postgres sh -c "ls -la /docker-entrypoint-initdb.d"
```

---

## Validar tablas

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "\dt"
```

---

## Validar usuarios demo

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT username, role_id, status_id FROM app_users;"
```

---

## Validar clientes

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT full_name, email FROM customers;"
```

---

## Validar espacios

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT name, capacity, base_hourly_rate, status_id, time_zone_id FROM spaces;"
```

---

## Validar reserva demo

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "SELECT reservation_code, start_time, end_time, final_amount FROM reservations;"
```

---

## Abrir consola psql

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations
```

Salir de `psql`:

```text
\q
```

---

## Ejecutar backend

Desde la raíz del repositorio:

```bash
dotnet run --project backend/src/Cowork.Api/Cowork.Api.csproj
```

---

## Ejecutar pruebas backend

```bash
cd backend
dotnet test
```

---

## Ejecutar frontend

```bash
cd frontend/cowork-web
ng serve
```

Abrir:

```text
http://localhost:4200
```

---

## Puertos usados

| Servicio | Puerto |
|---|---:|
| PostgreSQL Docker | `5433` |
| Backend API | Según `launchSettings.json` |
| Angular | `4200` |

---

## Problemas comunes

### Docker no ejecuta scripts SQL nuevos

PostgreSQL solo ejecuta los scripts de `/docker-entrypoint-initdb.d` cuando el volumen está vacío.

Solución:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

### Error por rutas en Git Bash

Si Git Bash convierte rutas como `/docker-entrypoint-initdb.d`, usar:

```bash
docker exec -it cowork-postgres sh -c "ls -la /docker-entrypoint-initdb.d"
```

### Verificar errores de inicialización

```bash
docker logs cowork-postgres
```