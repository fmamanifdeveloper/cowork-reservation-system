import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';
import { PublicApi } from '../public-api';
import { PublicSpace, PricingPreviewResponse, PublicReservationResponse } from '../public-models';

@Component({
  selector: 'app-public-reservation-page',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './public-reservation-page.html',
  styleUrl: './public-reservation-page.scss',
})
export class PublicReservationPage {
  private readonly route = inject(ActivatedRoute);
  private readonly publicApi = inject(PublicApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly spaces = signal<PublicSpace[]>([]);
  readonly selectedSpace = signal<PublicSpace | null>(null);
  readonly pricingPreview = signal<PricingPreviewResponse | null>(null);
  readonly createdReservation = signal<PublicReservationResponse | null>(null);

  readonly isLoadingSpaces = signal(false);
  readonly isPreviewLoading = signal(false);
  readonly isCreating = signal(false);

  spaceId = '';
  customerFullName = '';
  customerEmail = '';
  customerPhone = '';
  customerDocumentNumber = '';
  startTime = '';
  endTime = '';

  ngOnInit(): void {
    this.spaceId = this.route.snapshot.queryParamMap.get('spaceId') ?? '';
    this.loadSpaces();
  }

  loadSpaces(): void {
    this.isLoadingSpaces.set(true);

    this.publicApi
      .listSpaces()
      .pipe(finalize(() => this.isLoadingSpaces.set(false)))
      .subscribe({
        next: spaces => {
          this.spaces.set(spaces);

          if (!this.spaceId && spaces.length > 0) {
            this.spaceId = spaces[0].id;
          }

          this.updateSelectedSpace();
        },
        error: () => {
          this.notificationStore.error('No se pudieron cargar los espacios.');
        }
      });
  }

  updateSelectedSpace(): void {
    const space = this.spaces().find(item => item.id === this.spaceId) ?? null;
    this.selectedSpace.set(space);
    this.pricingPreview.set(null);
    this.createdReservation.set(null);
  }

  previewPrice(): void {
    if (!this.validateSchedule()) {
      return;
    }

    this.isPreviewLoading.set(true);

    this.publicApi
      .previewPricing({
        spaceId: this.spaceId,
        startTime: this.toPeruOffsetDateTime(this.startTime),
        endTime: this.toPeruOffsetDateTime(this.endTime)
      })
      .pipe(finalize(() => this.isPreviewLoading.set(false)))
      .subscribe({
        next: response => {
          this.pricingPreview.set(response);
          this.notificationStore.success('Precio calculado correctamente.');
        },
        error: () => {
          this.notificationStore.error('No se pudo calcular el precio.');
        }
      });
  }

  createReservation(): void {
    if (!this.customerFullName.trim()) {
      this.notificationStore.warning('Ingresa tu nombre completo.');
      return;
    }

    if (!this.validateSchedule()) {
      return;
    }

    this.isCreating.set(true);

    this.publicApi
      .createReservation({
        spaceId: this.spaceId,
        customerFullName: this.customerFullName.trim(),
        customerEmail: this.customerEmail.trim() || null,
        customerPhone: this.customerPhone.trim() || null,
        customerDocumentNumber: this.customerDocumentNumber.trim() || null,
        startTime: this.toPeruOffsetDateTime(this.startTime),
        endTime: this.toPeruOffsetDateTime(this.endTime)
      })
      .pipe(finalize(() => this.isCreating.set(false)))
      .subscribe({
        next: response => {
          this.createdReservation.set(response);
          this.notificationStore.success('Reserva creada correctamente.');
        },
        error: error => {
          if (error.status === 409) {
            this.notificationStore.error('El espacio ya fue reservado en ese horario.');
            return;
          }

          this.notificationStore.error('No se pudo crear la reserva.');
        }
      });
  }

  private validateSchedule(): boolean {
    if (!this.spaceId) {
      this.notificationStore.warning('Selecciona un espacio.');
      return false;
    }

    if (!this.startTime || !this.endTime) {
      this.notificationStore.warning('Selecciona fecha y hora de inicio y fin.');
      return false;
    }

    if (new Date(this.startTime) >= new Date(this.endTime)) {
      this.notificationStore.warning('La hora de inicio debe ser menor que la hora de fin.');
      return false;
    }

    return true;
  }

  private toPeruOffsetDateTime(value: string): string {
    return value.length === 16 ? `${value}:00-05:00` : `${value}-05:00`;
  }
}
