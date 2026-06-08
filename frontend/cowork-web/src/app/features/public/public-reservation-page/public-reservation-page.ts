import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';
import { PublicApi } from '../public-api';
import { PublicSpace, PublicPricingPreviewResponse, PublicReservationResponse, CreatePublicReservationRequest } from '../public-models';

@Component({
  selector: 'app-public-reservation-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './public-reservation-page.html',
  styleUrl: './public-reservation-page.scss',
})
export class PublicReservationPage {
  private readonly publicApi = inject(PublicApi);
  private readonly notificationStore = inject(NotificationStore);
  private readonly route = inject(ActivatedRoute);

  readonly spaces = signal<PublicSpace[]>([]);
  readonly isLoadingSpaces = signal(false);
  readonly isPreviewing = signal(false);
  readonly isSubmitting = signal(false);
  readonly wasSubmitted = signal(false);

  readonly pricingPreview = signal<PublicPricingPreviewResponse | null>(null);
  readonly createdReservation = signal<PublicReservationResponse | null>(null);

  form = this.createEmptyForm();

  ngOnInit(): void {
    const querySpaceId = this.route.snapshot.queryParamMap.get('spaceId');

    this.loadSpaces(querySpaceId);
  }

  loadSpaces(selectedSpaceId?: string | null): void {
    this.isLoadingSpaces.set(true);

    this.publicApi
      .listSpaces()
      .pipe(finalize(() => this.isLoadingSpaces.set(false)))
      .subscribe({
        next: spaces => {
          this.spaces.set(spaces);

          if (selectedSpaceId) {
            this.form.spaceId = selectedSpaceId;
            return;
          }

          const firstAvailableSpace = spaces.find(space => space.status === 'Active');

          if (firstAvailableSpace) {
            this.form.spaceId = firstAvailableSpace.id;
          }
        },
        error: () => {
          this.notificationStore.error('No se pudieron cargar los espacios disponibles.');
        }
      });
  }

  previewPricing(): void {
    this.wasSubmitted.set(true);
    this.pricingPreview.set(null);
    this.createdReservation.set(null);

    if (!this.validateSchedule()) {
      return;
    }

    this.isPreviewing.set(true);

    this.publicApi
      .previewPricing({
        spaceId: this.form.spaceId,
        startTime: this.toPeruOffsetDateTime(this.form.startTime),
        endTime: this.toPeruOffsetDateTime(this.form.endTime)
      })
      .pipe(finalize(() => this.isPreviewing.set(false)))
      .subscribe({
        next: preview => {
          this.pricingPreview.set(preview);
          this.notificationStore.success('Precio calculado correctamente.');
        },
        error: error => {
          if (error.status === 409) {
            this.notificationStore.error('El espacio ya está reservado en ese horario.');
            return;
          }

          if (error.status === 400) {
            this.notificationStore.error('La reserva no cumple las reglas requeridas.');
            return;
          }

          this.notificationStore.error('No se pudo calcular el precio.');
        }
      });
  }

  createReservation(): void {
    this.wasSubmitted.set(true);
    this.createdReservation.set(null);

    if (!this.validateForm()) {
      return;
    }

    this.isSubmitting.set(true);

    const request: CreatePublicReservationRequest = {
      spaceId: this.form.spaceId,
      customerFullName: this.form.customerFullName.trim(),
      customerEmail: this.form.customerEmail.trim(),
      customerPhone: this.form.customerPhone.trim(),
      customerDocumentNumber: this.form.customerDocumentNumber.trim() || null,
      startTime: this.toPeruOffsetDateTime(this.form.startTime),
      endTime: this.toPeruOffsetDateTime(this.form.endTime)
    };

    this.publicApi
      .createReservation(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: reservation => {
          this.createdReservation.set(reservation);
          this.pricingPreview.set(null);
          this.wasSubmitted.set(false);
          this.notificationStore.success('Reserva creada correctamente.');
        },
        error: error => {
          if (error.status === 409) {
            this.notificationStore.error('El espacio ya está reservado en ese horario.');
            return;
          }

          if (error.status === 400) {
            this.notificationStore.error('La reserva no cumple las reglas requeridas.');
            return;
          }

          this.notificationStore.error('No se pudo crear la reserva.');
        }
      });
  }

  resetForm(): void {
    const currentSpaceId = this.form.spaceId;

    this.form = this.createEmptyForm();
    this.form.spaceId = currentSpaceId;

    this.wasSubmitted.set(false);
    this.pricingPreview.set(null);
    this.createdReservation.set(null);
  }

  getSelectedSpace(): PublicSpace | null {
    return this.spaces().find(space => space.id === this.form.spaceId) ?? null;
  }

  getCapacityLabel(capacity: number): string {
    return capacity === 1 ? '1 persona' : `${capacity} personas`;
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      Active: 'Disponible',
      Maintenance: 'Mantenimiento',
      Inactive: 'No disponible'
    };

    return labels[status] ?? status;
  }

  isSelectedSpaceAvailable(): boolean {
    return this.getSelectedSpace()?.status === 'Active';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(value);
  }

  formatSchedule(openingTime: string, closingTime: string): string {
    return `${this.formatTime(openingTime)} - ${this.formatTime(closingTime)}`;
  }

  formatTime(value: string): string {
    if (!value) {
      return '--:--';
    }

    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  isFullNameInvalid(): boolean {
    return this.wasSubmitted() && !this.form.customerFullName.trim();
  }

  isEmailInvalid(): boolean {
    if (!this.wasSubmitted()) {
      return false;
    }

    const email = this.form.customerEmail.trim();

    if (!email) {
      return true;
    }

    return !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  isPhoneInvalid(): boolean {
    return this.wasSubmitted() && this.form.customerPhone.trim().length < 6;
  }

  isSpaceInvalid(): boolean {
    return this.wasSubmitted() && !this.form.spaceId;
  }

  isStartTimeInvalid(): boolean {
    return this.wasSubmitted() && !this.form.startTime;
  }

  isEndTimeInvalid(): boolean {
    return this.wasSubmitted() && !this.form.endTime;
  }

  isStartTimeStepInvalid(): boolean {
    return (
      this.wasSubmitted() &&
      !!this.form.startTime &&
      !this.hasAllowedTimeStep(this.form.startTime)
    );
  }

  isEndTimeStepInvalid(): boolean {
    return (
      this.wasSubmitted() &&
      !!this.form.endTime &&
      !this.hasAllowedTimeStep(this.form.endTime)
    );
  }

  isScheduleInvalid(): boolean {
    return (
      this.wasSubmitted() &&
      !!this.form.startTime &&
      !!this.form.endTime &&
      new Date(this.form.startTime) >= new Date(this.form.endTime)
    );
  }

  private validateForm(): boolean {
    if (!this.form.customerFullName.trim()) {
      this.notificationStore.warning('Ingresa tu nombre completo.');
      return false;
    }

    if (this.isEmailInvalid()) {
      this.notificationStore.warning('Ingresa un correo válido.');
      return false;
    }

    if (this.form.customerPhone.trim().length < 6) {
      this.notificationStore.warning('Ingresa un teléfono válido.');
      return false;
    }

    return this.validateSchedule();
  }

  private validateSchedule(): boolean {
    if (!this.form.spaceId) {
      this.notificationStore.warning('Selecciona un espacio.');
      return false;
    }

    if (!this.isSelectedSpaceAvailable()) {
      this.notificationStore.warning('El espacio seleccionado no está disponible.');
      return false;
    }

    if (!this.form.startTime || !this.form.endTime) {
      this.notificationStore.warning('Ingresa fecha y hora de inicio y fin.');
      return false;
    }

    if (
      !this.hasAllowedTimeStep(this.form.startTime) ||
      !this.hasAllowedTimeStep(this.form.endTime)
    ) {
      this.notificationStore.warning('Las reservas deben usar horarios en bloques de 30 minutos.');
      return false;
    }

    if (new Date(this.form.startTime) >= new Date(this.form.endTime)) {
      this.notificationStore.warning('La hora de inicio debe ser menor que la hora de fin.');
      return false;
    }

    return true;
  }

  private hasAllowedTimeStep(value: string): boolean {
    const timePart = value.split('T')[1];

    if (!timePart) {
      return false;
    }

    const minuteText = timePart.split(':')[1];
    const minutes = Number(minuteText);

    return Number.isInteger(minutes) && minutes % 30 === 0;
  }

  private toPeruOffsetDateTime(value: string): string {
    return value.length === 16 ? `${value}:00-05:00` : `${value}-05:00`;
  }

  private createEmptyForm() {
    return {
      spaceId: '',
      customerFullName: '',
      customerEmail: '',
      customerPhone: '',
      customerDocumentNumber: '',
      startTime: '',
      endTime: ''
    };
  }

  copyReservationCode(): void {
    const reservation = this.createdReservation();

    if (!reservation?.reservationCode) {
      this.notificationStore.warning('No hay código de reserva para copiar.');
      return;
    }

    navigator.clipboard
      .writeText(reservation.reservationCode)
      .then(() => {
        this.notificationStore.success('Código de reserva copiado.');
      })
      .catch(() => {
        this.notificationStore.warning('No se pudo copiar el código automáticamente.');
      });
  }

  getReservationStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      Pending: 'Pendiente',
      Confirmed: 'Confirmada',
      Cancelled: 'Cancelada',
      Completed: 'Completada'
    };

    return labels[status] ?? status;
  }
}
