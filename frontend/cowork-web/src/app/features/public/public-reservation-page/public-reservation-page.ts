import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';
import { PublicApi } from '../public-api';
import { PublicSpace, PublicPricingPreviewResponse, PublicReservationResponse, CreatePublicReservationRequest, PublicAvailabilityResponse, PublicAvailabilitySlot, PublicPricingAdjustment } from '../public-models';

interface PublicReservationForm {
  spaceId: string;
  customerFullName: string;
  customerEmail: string;
  customerPhone: string;
  customerDocumentNumber: string;
  reservationDate: string;
  selectedStartTime: string;
  durationMinutes: number | null;
  startTime: string;
  endTime: string;
}

interface DurationOption {
  minutes: number;
  label: string;
  endTime: string;
}

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

  private pricingPreviewTimeoutId: ReturnType<typeof setTimeout> | null = null;

  readonly spaces = signal<PublicSpace[]>([]);
  readonly availability = signal<PublicAvailabilityResponse | null>(null);

  readonly isLoadingSpaces = signal(false);
  readonly isLoadingAvailability = signal(false);
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

  ngOnDestroy(): void {
    this.clearPricingPreviewTimeout();
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
          } else {
            const firstAvailableSpace = spaces.find(space => space.status === 'Active');

            if (firstAvailableSpace) {
              this.form.spaceId = firstAvailableSpace.id;
            }
          }

          this.loadAvailabilityForCurrentSelection();
        },
        error: () => {
          this.notificationStore.error('No se pudieron cargar los espacios disponibles.');
        }
      });
  }

  onSpaceChanged(): void {
    this.clearReservationSelection();
    this.loadAvailabilityForCurrentSelection();
  }

  onDateChanged(): void {
    this.clearReservationSelection();
    this.loadAvailabilityForCurrentSelection();
  }

  loadAvailabilityForCurrentSelection(): void {
    if (!this.form.spaceId || !this.form.reservationDate) {
      this.availability.set(null);
      return;
    }

    this.isLoadingAvailability.set(true);

    this.publicApi
      .getSpaceAvailability(this.form.spaceId, this.form.reservationDate)
      .pipe(finalize(() => this.isLoadingAvailability.set(false)))
      .subscribe({
        next: availability => {
          this.availability.set(availability);
          this.removeInvalidSelectedSlot();
        },
        error: () => {
          this.availability.set(null);
          this.notificationStore.error('No se pudo cargar la disponibilidad del espacio.');
        }
      });
  }

  selectStartSlot(slot: PublicAvailabilitySlot): void {
    if (!slot.isAvailable) {
      return;
    }

    this.form.selectedStartTime = this.normalizeTime(slot.startTime);
    this.form.durationMinutes = null;

    this.pricingPreview.set(null);
    this.createdReservation.set(null);

    const durationOptions = this.getDurationOptions();

    if (durationOptions.length > 0) {
      this.form.durationMinutes = durationOptions[0].minutes;
      this.updateComputedReservationTimes();
      this.scheduleAutomaticPricingPreview();
    }
  }

  selectDuration(minutes: number): void {
    this.form.durationMinutes = minutes;
    this.updateComputedReservationTimes();
    this.scheduleAutomaticPricingPreview();
  }

  previewPricing(): void {
    this.wasSubmitted.set(true);
    this.runPricingPreview(true);
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
          this.loadAvailabilityForCurrentSelection();
        },
        error: error => {
          if (error.status === 409) {
            this.notificationStore.error('El espacio ya está reservado en ese horario.');
            this.loadAvailabilityForCurrentSelection();
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
    const currentDate = this.form.reservationDate;

    this.form = this.createEmptyForm();
    this.form.spaceId = currentSpaceId;
    this.form.reservationDate = currentDate;

    this.wasSubmitted.set(false);
    this.pricingPreview.set(null);
    this.createdReservation.set(null);

    this.loadAvailabilityForCurrentSelection();
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

  getSelectedSpace(): PublicSpace | null {
    return this.spaces().find(space => space.id === this.form.spaceId) ?? null;
  }

  getSelectedDate(): string | null {
    return this.form.reservationDate || null;
  }

  getDurationOptions(): DurationOption[] {
    const availability = this.availability();

    if (!availability || !this.form.selectedStartTime) {
      return [];
    }

    const slots = availability.slots;
    const selectedIndex = slots.findIndex(slot =>
      this.normalizeTime(slot.startTime) === this.form.selectedStartTime
    );

    if (selectedIndex < 0 || !slots[selectedIndex].isAvailable) {
      return [];
    }

    const options: DurationOption[] = [];
    let accumulatedMinutes = 0;

    for (let index = selectedIndex; index < slots.length; index++) {
      const slot = slots[index];

      if (!slot.isAvailable) {
        break;
      }

      accumulatedMinutes += 30;

      if (accumulatedMinutes > 480) {
        break;
      }

      options.push({
        minutes: accumulatedMinutes,
        label: this.formatDurationLabel(accumulatedMinutes),
        endTime: this.normalizeTime(slot.endTime)
      });
    }

    return options;
  }

  getSelectedRangeLabel(): string {
    if (!this.form.startTime || !this.form.endTime) {
      return 'Selecciona horario';
    }

    return `${this.formatTime(this.form.selectedStartTime)} - ${this.formatTimeFromDateTime(this.form.endTime)}`;
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

  getPricingAdjustmentLabel(adjustment: PublicPricingAdjustment): string {
    const labels: Record<string, string> = {
      PeakHour: 'Recargo por hora pico',
      Weekend: 'Recargo por fin de semana',
      LongReservation: 'Descuento por reserva larga',
      AdvanceBooking: 'Descuento por reserva anticipada'
    };

    return labels[adjustment.rule] ?? adjustment.rule;
  }

  getPricingAdjustmentAmount(adjustment: PublicPricingAdjustment): number {
    return adjustment.amountAfter - adjustment.amountBefore;
  }

  getPricingAdjustmentPercentageLabel(adjustment: PublicPricingAdjustment): string {
    const percentage = adjustment.percentage * 100;
    const sign = percentage > 0 ? '+' : '';

    return `${sign}${percentage}%`;
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

  isSelectedSpaceAvailable(): boolean {
    return this.getSelectedSpace()?.status === 'Active';
  }

  isSlotSelected(slot: PublicAvailabilitySlot): boolean {
    return this.form.selectedStartTime === this.normalizeTime(slot.startTime);
  }

  isDurationSelected(minutes: number): boolean {
    return this.form.durationMinutes === minutes;
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

  formatTimeFromDateTime(value: string): string {
    if (!value || !value.includes('T')) {
      return '--:--';
    }

    return value.split('T')[1].slice(0, 5);
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  formatDateOnly(value: string): string {
    if (!value) {
      return 'Selecciona una fecha';
    }

    const [year, month, day] = value.split('-').map(Number);
    const date = new Date(year, month - 1, day);

    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'full'
    }).format(date);
  }

  getSelectedSpaceHourlyRate(): number | null {
    return this.getSelectedSpace()?.baseHourlyRate ?? null;
  }

  getReservationDurationLabel(): string {
    if (!this.form.durationMinutes) {
      return 'Pendiente';
    }

    return this.formatDurationLabel(this.form.durationMinutes);
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

  isDateInvalid(): boolean {
    return this.wasSubmitted() && !this.form.reservationDate;
  }

  isStartSlotInvalid(): boolean {
    return this.wasSubmitted() && !this.form.selectedStartTime;
  }

  isDurationInvalid(): boolean {
    return this.wasSubmitted() && !this.form.durationMinutes;
  }

  private runPricingPreview(showValidationFeedback: boolean): void {
    if (showValidationFeedback) {
      this.wasSubmitted.set(true);
    }

    this.updateComputedReservationTimes();

    if (!this.validateSchedule(showValidationFeedback)) {
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

          if (showValidationFeedback) {
            this.notificationStore.success('Precio calculado correctamente.');
          }
        },
        error: error => {
          this.pricingPreview.set(null);

          if (error.status === 409) {
            this.notificationStore.error('El espacio ya está reservado en ese horario.');
            this.loadAvailabilityForCurrentSelection();
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

  private scheduleAutomaticPricingPreview(): void {
    this.pricingPreview.set(null);
    this.createdReservation.set(null);

    this.clearPricingPreviewTimeout();

    this.pricingPreviewTimeoutId = setTimeout(() => {
      if (this.canAutoPreviewPrice()) {
        this.runPricingPreview(false);
      }
    }, 500);
  }

  private canAutoPreviewPrice(): boolean {
    this.updateComputedReservationTimes();

    return (
      !!this.form.spaceId &&
      !!this.form.reservationDate &&
      !!this.form.selectedStartTime &&
      !!this.form.durationMinutes &&
      !!this.form.startTime &&
      !!this.form.endTime &&
      this.isSelectedSpaceAvailable()
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

    return this.validateSchedule(true);
  }

  private validateSchedule(showFeedback = true): boolean {
    this.updateComputedReservationTimes();

    if (!this.form.spaceId) {
      if (showFeedback) {
        this.notificationStore.warning('Selecciona un espacio.');
      }

      return false;
    }

    if (!this.isSelectedSpaceAvailable()) {
      if (showFeedback) {
        this.notificationStore.warning('El espacio seleccionado no está disponible.');
      }

      return false;
    }

    if (!this.form.reservationDate) {
      if (showFeedback) {
        this.notificationStore.warning('Selecciona una fecha.');
      }

      return false;
    }

    if (!this.form.selectedStartTime) {
      if (showFeedback) {
        this.notificationStore.warning('Selecciona una hora de inicio disponible.');
      }

      return false;
    }

    if (!this.form.durationMinutes) {
      if (showFeedback) {
        this.notificationStore.warning('Selecciona una duración válida.');
      }

      return false;
    }

    if (!this.form.startTime || !this.form.endTime) {
      if (showFeedback) {
        this.notificationStore.warning('No se pudo calcular el horario de la reserva.');
      }

      return false;
    }

    if (new Date(this.form.startTime) >= new Date(this.form.endTime)) {
      if (showFeedback) {
        this.notificationStore.warning('La hora de inicio debe ser menor que la hora de fin.');
      }

      return false;
    }

    if (!this.isSelectedSlotStillAvailable()) {
      if (showFeedback) {
        this.notificationStore.warning('El horario seleccionado ya no está disponible.');
      }

      return false;
    }

    return true;
  }

  private isSelectedSlotStillAvailable(): boolean {
    const availability = this.availability();

    if (!availability || !this.form.selectedStartTime || !this.form.durationMinutes) {
      return false;
    }

    const selectedIndex = availability.slots.findIndex(slot =>
      this.normalizeTime(slot.startTime) === this.form.selectedStartTime
    );

    if (selectedIndex < 0) {
      return false;
    }

    const slotsNeeded = this.form.durationMinutes / 30;

    for (let offset = 0; offset < slotsNeeded; offset++) {
      const slot = availability.slots[selectedIndex + offset];

      if (!slot || !slot.isAvailable) {
        return false;
      }
    }

    return true;
  }

  private updateComputedReservationTimes(): void {
    if (
      !this.form.reservationDate ||
      !this.form.selectedStartTime ||
      !this.form.durationMinutes
    ) {
      this.form.startTime = '';
      this.form.endTime = '';
      return;
    }

    const startTime = this.normalizeTime(this.form.selectedStartTime);
    const startDate = this.buildLocalDate(this.form.reservationDate, startTime);
    const endDate = new Date(startDate.getTime() + this.form.durationMinutes * 60_000);

    this.form.startTime = this.toLocalDateTimeInputValue(startDate);
    this.form.endTime = this.toLocalDateTimeInputValue(endDate);
  }

  private clearReservationSelection(): void {
    this.form.selectedStartTime = '';
    this.form.durationMinutes = null;
    this.form.startTime = '';
    this.form.endTime = '';

    this.pricingPreview.set(null);
    this.createdReservation.set(null);

    this.clearPricingPreviewTimeout();
  }

  private removeInvalidSelectedSlot(): void {
    if (!this.form.selectedStartTime) {
      return;
    }

    if (!this.isSelectedSlotStillAvailable()) {
      this.clearReservationSelection();
    }
  }

  private normalizeTime(value: string): string {
    if (!value) {
      return '';
    }

    return value.length >= 5 ? value.slice(0, 5) : value;
  }

  private formatDurationLabel(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} min`;
    }

    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;

    if (remainingMinutes === 0) {
      return hours === 1 ? '1 h' : `${hours} h`;
    }

    return `${hours} h ${remainingMinutes} min`;
  }

  private buildLocalDate(date: string, time: string): Date {
    const [year, month, day] = date.split('-').map(Number);
    const [hour, minute] = time.split(':').map(Number);

    return new Date(year, month - 1, day, hour, minute, 0, 0);
  }

  private toLocalDateTimeInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    const hour = `${date.getHours()}`.padStart(2, '0');
    const minute = `${date.getMinutes()}`.padStart(2, '0');

    return `${year}-${month}-${day}T${hour}:${minute}`;
  }

  private toPeruOffsetDateTime(value: string): string {
    return value.length === 16 ? `${value}:00-05:00` : `${value}-05:00`;
  }

  private getTodayInputDate(): string {
    return this.toDateInputValue(new Date());
  }

  private toDateInputValue(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private clearPricingPreviewTimeout(): void {
    if (this.pricingPreviewTimeoutId) {
      clearTimeout(this.pricingPreviewTimeoutId);
      this.pricingPreviewTimeoutId = null;
    }
  }

  private createEmptyForm(): PublicReservationForm {
    return {
      spaceId: '',
      customerFullName: '',
      customerEmail: '',
      customerPhone: '',
      customerDocumentNumber: '',
      reservationDate: this.getTodayInputDate(),
      selectedStartTime: '',
      durationMinutes: null,
      startTime: '',
      endTime: ''
    };
  }
}
