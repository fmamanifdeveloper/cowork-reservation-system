# Local Setup

Este documento describe cómo levantar el proyecto localmente.

---

## Requisitos

Instalar:

- Docker Desktop.
- .NET SDK 10.
- Node.js.
- npm.
- Git.

Opcional:

- Visual Studio Code.
- Visual Studio.
- Rider.
- Extensión REST Client.

---

## 1. Clonar el repositorio

```bash
git clone <URL_DEL_REPOSITORIO>
cd cowork-reservation-system
```

---

## 2. Levantar PostgreSQL

Desde la raíz del proyecto:

```bash
docker compose up -d
```

Esto levanta PostgreSQL y ejecuta scripts de inicialización.

---

## 3. Verificar contenedor

```bash
docker ps
```

También puedes verificar tablas:

```bash
docker exec -it cowork-postgres psql -U cowork_user -d cowork_reservations -c "\dt"
```

---

## 4. Ejecutar backend

Desde raíz:

```bash
dotnet run --project backend/src/Cowork.Api/Cowork.Api.csproj
```

API local:

```text
https://localhost:7011
```

Scalar:

```text
https://localhost:7011/scalar/v1
```

OpenAPI JSON:

```text
https://localhost:7011/openapi/v1.json
```

---

## 5. Ejecutar frontend

```bash
cd frontend/cowork-web
npm install
npm start
```

Si no existe script `start`:

```bash
npx ng serve
```

Frontend local:

```text
http://localhost:4200
```

---

## 6. Usuarios demo

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `staff01` | `Admin123!` | Staff |
| `juan.perez` | `Admin123!` | Customer |

---

## 7. Probar endpoints manualmente

Usar:

```text
docs/requests/requests.http
```

---

## 8. Ejecutar tests

```bash
cd backend
dotnet test
```

---

## 9. Build backend

```bash
cd backend
dotnet build
```

---

## 10. Build frontend

```bash
cd frontend/cowork-web
npm install
npx ng build
```

---

## 11. Reiniciar base de datos

Para borrar datos locales y reconstruir:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

---

## 12. Flujo de prueba recomendado

```text
1. Levantar Docker.
2. Ejecutar backend.
3. Ejecutar frontend.
4. Iniciar sesión como admin.
5. Revisar espacios.
6. Revisar reservas.
7. Entrar al portal público.
8. Seleccionar espacio y fecha.
9. Ver slots disponibles.
10. Crear reserva.
11. Revisar que el slot quede ocupado.
12. Revisar reportes.
13. Revisar auditoría.
```

---

## 13. Problemas comunes

### La API no responde

Validar:

```text
https://localhost:7011/scalar/v1
```

### Frontend no consume API

Validar:

```text
frontend/cowork-web/src/environments/environment.ts
```

Debe apuntar a:

```ts
apiBaseUrl: 'https://localhost:7011/api'
```

### Error de certificados HTTPS

```bash
dotnet dev-certs https --trust
```

### Base de datos con datos antiguos

```bash
docker compose down --volumes --remove-orphans
docker compose up -d
```

---

## 14. Validación final

Antes de entregar:

```bash
docker compose down --volumes --remove-orphans
docker compose up -d

cd backend
dotnet build
dotnet test
cd ..

cd frontend/cowork-web
npm install
npx ng build
cd ../..
```
