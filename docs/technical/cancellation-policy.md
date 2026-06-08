# Cancellation Policy

Este documento describe la política de cancelación y reembolso aplicada a las reservas.

---

## Objetivo

Permitir cancelar reservas y calcular el monto de reembolso según la anticipación con la que se solicita la cancelación.

---

## Regla general

La política se basa en la diferencia entre:

```text
reservationStart - cancellationRequestedAt
```

---

## Tabla de reembolso

| Anticipación | Reembolso |
|---|---:|
| Más de 48 horas antes del inicio | 100% |
| Entre 24 y 48 horas antes del inicio | 50% |
| Menos de 24 horas antes del inicio | 0% |
| Después del inicio de la reserva | 0% |

---

## Ejemplos

### Cancelación con más de 48 horas

```text
Reserva inicia:       2026-06-10 10:00
Cancelación pedida:   2026-06-08 09:00
Anticipación:         49 horas
Monto final:          S/ 200
Reembolso:            S/ 200
```

### Cancelación entre 24 y 48 horas

```text
Reserva inicia:       2026-06-10 10:00
Cancelación pedida:   2026-06-08 22:00
Anticipación:         36 horas
Monto final:          S/ 200
Reembolso:            S/ 100
```

### Cancelación con menos de 24 horas

```text
Reserva inicia:       2026-06-10 10:00
Cancelación pedida:   2026-06-09 23:00
Anticipación:         11 horas
Monto final:          S/ 200
Reembolso:            S/ 0
```

### Cancelación después del inicio

```text
Reserva inicia:       2026-06-10 10:00
Cancelación pedida:   2026-06-10 10:30
Monto final:          S/ 200
Reembolso:            S/ 0
```

---

## Comportamiento en el sistema

Cuando una reserva se cancela:

1. Se calcula el reembolso.
2. Se actualiza el estado de la reserva a `Cancelled`.
3. Se registra `CancelledAt`.
4. Se registra el usuario que canceló.
5. Se guarda el monto de reembolso.
6. Se registra auditoría.
7. La reserva deja de bloquear disponibilidad.

---

## Pruebas unitarias

Archivo:

```text
backend/tests/Cowork.UnitTests/Cancellations/CancellationPolicyServiceTests.cs
```

Casos cubiertos:

- Reembolso completo con más de 48 horas.
- Reembolso del 50% entre 24 y 48 horas.
- Sin reembolso con menos de 24 horas.
- Sin reembolso después del inicio.
- Redondeo del monto de reembolso.
