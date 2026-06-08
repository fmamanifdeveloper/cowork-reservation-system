# Dynamic Pricing

Este documento describe el motor de tarifas dinámicas usado para calcular el precio de una reserva.

---

## Objetivo

Calcular el precio estimado antes de confirmar una reserva, aplicando reglas de recargo o descuento según horario, duración y anticipación.

---

## Fórmula base

```text
Precio base = tarifa por hora × duración en horas
```

Ejemplo:

```text
Tarifa por hora: S/ 50
Duración: 1.5 horas

Precio base = 50 × 1.5 = S/ 75
```

---

## Orden de reglas

Las reglas se aplican en este orden:

```text
1. Recargo por hora pico
2. Recargo por fin de semana
3. Descuento por reserva larga
4. Descuento por reserva anticipada
```

El orden es importante porque cada regla se aplica sobre el monto resultante de la regla anterior.

---

## Reglas

### 1. Hora pico

Condición:

```text
La reserva cruza 09:00 - 11:00
o
La reserva cruza 17:00 - 19:00
```

Ajuste:

```text
+25%
```

### 2. Fin de semana

Condición:

```text
La reserva inicia sábado o domingo
```

Ajuste:

```text
+15%
```

### 3. Reserva larga

Condición:

```text
Duración mayor o igual a 4 horas
```

Ajuste:

```text
-10%
```

### 4. Reserva anticipada

Condición:

```text
La reserva se crea al menos 7 días antes del inicio
```

Ajuste:

```text
-5%
```

---

## Ejemplo con múltiples reglas

Reserva:

```text
Tarifa: S/ 50/hora
Duración: 4 horas
Día: sábado
Horario: 09:00 - 13:00
Anticipación: 8 días
```

Cálculo:

```text
Base = 50 × 4 = 200

Hora pico +25%:
200 → 250

Fin de semana +15%:
250 → 287.50

Reserva larga -10%:
287.50 → 258.75

Reserva anticipada -5%:
258.75 → 245.81
```

Total:

```text
S/ 245.81
```

---

## Respuesta de preview

```json
{
  "baseAmount": 75,
  "finalAmount": 93.75,
  "adjustments": [
    {
      "rule": "PeakHour",
      "percentage": 0.25,
      "description": "Reservation overlaps 09:00-11:00 or 17:00-19:00.",
      "amountBefore": 75,
      "amountAfter": 93.75
    }
  ]
}
```

---

## Trazabilidad

El resultado del cálculo se almacena en la reserva mediante:

```text
pricing_breakdown
```

---

## Validaciones

El motor valida:

- Inicio menor que fin.
- Duración mínima de 30 minutos.
- Duración máxima de 8 horas.

---

## Pruebas unitarias

Archivo:

```text
backend/tests/Cowork.UnitTests/Pricing/DynamicPricingCalculatorTests.cs
```

Casos cubiertos:

- Tarifa base simple.
- Hora pico.
- Fin de semana.
- Reserva larga.
- Reserva anticipada.
- Combinación de reglas.
- Duración inválida.
- Inicio mayor o igual al fin.
- Duración mayor a 8 horas.
