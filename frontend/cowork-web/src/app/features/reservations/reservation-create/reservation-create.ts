import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PricingApi } from '@core/api/pricing-api';
import { ReservationsApi } from '@core/api/reservations-api';
import { SpacesApi } from '@core/api/spaces-api';
import { PricingPreviewResponse } from '@core/models/pricing';
import { Reservation } from '@core/models/reservation';
import { Space } from '@core/models/space';

@Component({
  selector: 'app-reservation-create',
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation-create.html',
  styleUrl: './reservation-create.scss',
})
export class ReservationCreate implements OnInit {
  private readonly spacesApi = inject(SpacesApi);
  private readonly pricingApi = inject(PricingApi);
  private readonly reservationsApi = inject(ReservationsApi);

  readonly spaces = signal<Space[]>([]);
  readonly loadingSpaces = signal(false);
  readonly loadingPreview = signal(false);
  readonly saving = signal(false);

  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly preview = signal<PricingPreviewResponse | null>(null);
  readonly createdReservation = signal<Reservation | null>(null);

  selectedSpaceId = '';
  startTime = '';
  endTime = '';

  readonly canSubmit = computed(() =>
    Boolean(this.selectedSpaceId && this.startTime && this.endTime)
  );

  ngOnInit(): void {
    this.loadSpaces();
  }

  loadSpaces(): void {
    this.loadingSpaces.set(true);
    this.error.set(null);

    this.spacesApi.list().subscribe({
      next: (spaces) => {
        this.spaces.set(spaces.filter(space => space.status === 'Active'));
        this.loadingSpaces.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los espacios disponibles.');
        this.loadingSpaces.set(false);
      }
    });
  }

  calculatePreview(): void {
    if (!this.canSubmit()) {
      this.error.set('Selecciona un espacio, fecha de inicio y fecha de fin.');
      return;
    }

    this.loadingPreview.set(true);
    this.error.set(null);
    this.success.set(null);
    this.preview.set(null);

    this.pricingApi.preview({
      spaceId: this.selectedSpaceId,
      startTime: this.toApiDateTime(this.startTime),
      endTime: this.toApiDateTime(this.endTime)
    }).subscribe({
      next: (response) => {
        this.preview.set(response);
        this.loadingPreview.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.error.set(this.resolveErrorMessage(error));
        this.loadingPreview.set(false);
      }
    });
  }

  createReservation(): void {
    if (!this.canSubmit()) {
      this.error.set('Completa los datos de la reserva.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.success.set(null);

    this.reservationsApi.create({
      spaceId: this.selectedSpaceId,
      startTime: this.toApiDateTime(this.startTime),
      endTime: this.toApiDateTime(this.endTime)
    }).subscribe({
      next: (reservation) => {
        this.createdReservation.set(reservation);
        this.success.set('Reserva creada correctamente.');
        this.saving.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.error.set(this.resolveErrorMessage(error));
        this.saving.set(false);
      }
    });
  }

  private toApiDateTime(value: string): string {
    return value.length === 16 ? `${value}:00Z` : value;
  }

  private resolveErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return 'El horario seleccionado ya se encuentra reservado para este espacio.';
    }

    if (error.error?.detail) {
      return error.error.detail;
    }

    return 'No se pudo procesar la operación.';
  }
}