# Concurrency Strategy

Este documento describe la estrategia usada para evitar overbooking y reservas solapadas.

---

## Problema

El sistema debe impedir que dos clientes reserven el mismo espacio en horarios que se crucen.

Ejemplo inválido:

```text
Reserva A:
10:00 - 11:00

Reserva B:
10:30 - 11:30
```

---

## Caso borde válido

El sistema sí permite reservas consecutivas sin cruce:

```text
Reserva A:
10:00 - 11:00

Reserva B:
11:00 - 12:00
```

---

## Condición de solapamiento

La condición usada es:

```text
existing.StartTime < requested.EndTime
AND existing.EndTime > requested.StartTime
```

---

## Estrategia implementada

La estrategia es combinada:

```text
1. Validación previa en backend.
2. Restricción en PostgreSQL.
3. Manejo de error como 409 Conflict.
4. Prueba de integración concurrente.
```

---

## 1. Validación previa en backend

Antes de crear una reserva, el backend consulta si ya existe una reserva activa para el mismo espacio y rango solicitado.

Se ignoran reservas canceladas.

Ejemplo conceptual:

```csharp
x.SpaceId == spaceId &&
x.Status != ReservationStatus.Cancelled &&
x.StartTime < requestedEndTime &&
x.EndTime > requestedStartTime
```

Si existe solapamiento, se lanza una excepción de conflicto.

Respuesta esperada:

```http
409 Conflict
```

---

## 2. Restricción en PostgreSQL

La validación previa mejora la experiencia, pero no es suficiente ante concurrencia real.

Ejemplo:

```text
Request A llega al mismo tiempo que Request B.
Ambos consultan disponibilidad.
Ambos ven libre.
Ambos intentan guardar.
```

Para proteger este caso, la base de datos mantiene una restricción contra reservas solapadas.

La base de datos es la última línea de defensa.

---

## 3. Manejo de conflicto HTTP

Cuando la aplicación detecta conflicto, responde:

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

---

## 4. Test de integración

Archivo:

```text
backend/tests/Cowork.IntegrationTests/Reservations/ReservationConcurrencyTests.cs
```

Resultado esperado:

```text
Una reserva se crea correctamente.
La otra falla con 409 Conflict.
```

---

## Manejo de fechas

El frontend puede enviar fechas con offset de Perú:

```text
2026-06-08T10:00:00-05:00
```

Antes de consultar o guardar en PostgreSQL, el backend convierte a UTC:

```text
2026-06-08T15:00:00Z
```

---

## Reservas canceladas

Las reservas canceladas no bloquean disponibilidad.

La consulta de disponibilidad y solapamiento excluye reservas con estado:

```text
Cancelled
```

---

## Por qué no solo frontend

Aunque el frontend muestre slots disponibles, no se confía solo en la interfaz porque:

- Un usuario podría llamar la API manualmente.
- Dos usuarios podrían seleccionar el mismo slot al mismo tiempo.
- La disponibilidad podría cambiar entre ver el horario y confirmar.
