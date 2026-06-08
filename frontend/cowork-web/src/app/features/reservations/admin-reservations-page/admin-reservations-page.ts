import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomersApi } from '@core/api/customers-api';
import { ReservationsApi } from '@core/api/reservations-api';
import { AuthStore } from '@core/auth/auth-store';
import { Customer } from '@core/models/customer';
import { Reservation, CreateReservationRequest, ReservationStatus } from '@core/models/reservation';
import { NotificationStore } from '@core/notifications/notification-store';
import { PublicApi } from '@features/public/public-api';
import {
  PublicPricingAdjustment,
  PublicPricingPreviewResponse,
  PublicSpace
} from '@features/public/public-models';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-admin-reservations-page',
  imports: [FormsModule],
  templateUrl: './admin-reservations-page.html',
  styleUrl: './admin-reservations-page.scss',
})
export class AdminReservationsPage {
  private readonly reservationsApi = inject(ReservationsApi);
  private readonly customersApi = inject(CustomersApi);
  private readonly publicApi = inject(PublicApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly authStore = inject(AuthStore);

  private pricingPreviewTimeoutId: ReturnType<typeof setTimeout> | null = null;

  readonly reservations = signal<Reservation[]>([]);
  readonly customers = signal<Customer[]>([]);
  readonly spaces = signal<PublicSpace[]>([]);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isCancelling = signal(false);
  readonly isCompleting = signal(false);
  readonly isPreviewing = signal(false);

  readonly isFormModalOpen = signal(false);
  readonly reservationToCancel = signal<Reservation | null>(null);
  readonly reservationToComplete = signal<Reservation | null>(null);
  readonly pricingPreview = signal<PublicPricingPreviewResponse | null>(null);

  readonly wasSubmitted = signal(false);

  readonly canManageAll = computed(() =>
    this.authStore.hasAnyRole(['Admin', 'Staff'])
  );

  form: CreateReservationRequest = this.createEmptyForm();

  ngOnInit(): void {
    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.clearPricingPreviewTimeout();
  }

  loadInitialData(): void {
    this.loadReservations();
    this.loadSpaces();

    if (this.canManageAll()) {
      this.loadCustomers();
    }
  }

  loadReservations(): void {
    this.isLoading.set(true);

    this.reservationsApi
      .list()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: reservations => this.reservations.set(reservations),
        error: () => this.notificationStore.error('No se pudieron cargar las reservas.')
      });
  }

  loadSpaces(): void {
    this.publicApi.listSpaces().subscribe({
      next: spaces => {
        this.spaces.set(spaces);

        if (!this.form.spaceId && spaces.length > 0) {
          this.form.spaceId = spaces[0].id;
        }
      },
      error: () => this.notificationStore.error('No se pudieron cargar los espacios.')
    });
  }

  loadCustomers(): void {
    this.customersApi.list().subscribe({
      next: customers => {
        this.customers.set(customers);

        if (!this.form.customerId && customers.length > 0) {
          this.form.customerId = customers[0].id;
        }
      },
      error: () => this.notificationStore.error('No se pudieron cargar los clientes.')
    });
  }

  openCreateModal(): void {
    this.form = this.createEmptyForm();

    const firstSpace = this.spaces()[0];
    const firstCustomer = this.customers()[0];

    if (firstSpace) {
      this.form.spaceId = firstSpace.id;
    }

    if (this.canManageAll() && firstCustomer) {
      this.form.customerId = firstCustomer.id;
    }

    this.clearPricingPreviewTimeout();
    this.pricingPreview.set(null);
    this.wasSubmitted.set(false);
    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    if (this.isSaving()) {
      return;
    }

    this.resetFormModal();
  }

  onReservationInputChanged(): void {
    this.pricingPreview.set(null);

    this.clearPricingPreviewTimeout();

    this.pricingPreviewTimeoutId = setTimeout(() => {
      if (this.canAutoPreviewPrice()) {
        this.runPricingPreview(false);
      }
    }, 650);
  }

  previewPricing(): void {
    this.wasSubmitted.set(true);
    this.runPricingPreview(true);
  }

  createReservation(): void {
    this.wasSubmitted.set(true);

    if (!this.validateForm()) {
      return;
    }

    if (!this.pricingPreview()) {
      this.notificationStore.warning('Revisa el precio estimado antes de crear la reserva.');
      return;
    }

    this.isSaving.set(true);

    const request: CreateReservationRequest = {
      spaceId: this.form.spaceId,
      customerId: this.canManageAll()
        ? this.form.customerId
        : this.authStore.customerId() ?? '',
      startTime: this.toPeruOffsetDateTime(this.form.startTime),
      endTime: this.toPeruOffsetDateTime(this.form.endTime)
    };

    this.reservationsApi
      .create(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.notificationStore.success('Reserva creada correctamente.');
          this.resetFormModal();
          this.loadReservations();
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

  openCancelModal(reservation: Reservation): void {
    this.reservationToCancel.set(reservation);
  }

  closeCancelModal(): void {
    if (this.isCancelling()) {
      return;
    }

    this.reservationToCancel.set(null);
  }

  confirmCancelReservation(): void {
    const reservation = this.reservationToCancel();

    if (!reservation) {
      return;
    }

    this.isCancelling.set(true);

    this.reservationsApi
      .cancel(reservation.id)
      .pipe(finalize(() => this.isCancelling.set(false)))
      .subscribe({
        next: () => {
          this.notificationStore.success('Reserva cancelada correctamente.');
          this.reservationToCancel.set(null);
          this.loadReservations();
        },
        error: error => {
          if (error.status === 400) {
            this.notificationStore.error('No se puede cancelar esta reserva.');
            return;
          }

          this.notificationStore.error('No se pudo cancelar la reserva.');
        }
      });
  }

  openCompleteModal(reservation: Reservation): void {
    this.reservationToComplete.set(reservation);
  }

  closeCompleteModal(): void {
    if (this.isCompleting()) {
      return;
    }

    this.reservationToComplete.set(null);
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

  confirmCompleteReservation(): void {
    const reservation = this.reservationToComplete();

    if (!reservation) {
      return;
    }

    this.isCompleting.set(true);

    this.reservationsApi
      .complete(reservation.id)
      .pipe(finalize(() => this.isCompleting.set(false)))
      .subscribe({
        next: () => {
          this.notificationStore.success('Reserva completada correctamente.');
          this.reservationToComplete.set(null);
          this.loadReservations();
        },
        error: error => {
          if (error.status === 400) {
            this.notificationStore.error('No se puede completar esta reserva.');
            return;
          }

          this.notificationStore.error('No se pudo completar la reserva.');
        }
      });
  }

  canCancel(reservation: Reservation): boolean {
    return reservation.status !== 'Cancelled' && reservation.status !== 'Completed';
  }

  canComplete(reservation: Reservation): boolean {
    return (
      this.canManageAll() &&
      reservation.status !== 'Cancelled' &&
      reservation.status !== 'Completed'
    );
  }

  getStatusLabel(status: ReservationStatus): string {
    const labels: Record<ReservationStatus, string> = {
      Pending: 'Pendiente',
      Confirmed: 'Confirmada',
      Cancelled: 'Cancelada',
      Completed: 'Completada'
    };

    return labels[status];
  }

  getSpaceName(spaceId: string): string {
    return this.spaces().find(space => space.id === spaceId)?.name ?? 'Espacio no disponible';
  }

  getCustomerName(customerId: string): string {
    if (!this.canManageAll()) {
      return 'Mi reserva';
    }

    return this.customers().find(customer => customer.id === customerId)?.fullName ?? 'Cliente no disponible';
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(value);
  }

  isSpaceInvalid(): boolean {
    return this.wasSubmitted() && !this.form.spaceId;
  }

  isCustomerInvalid(): boolean {
    return this.wasSubmitted() && this.canManageAll() && !this.form.customerId;
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

  private runPricingPreview(showValidationFeedback: boolean): void {
    if (showValidationFeedback) {
      this.wasSubmitted.set(true);
    }

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

  private canAutoPreviewPrice(): boolean {
    return (
      !!this.form.spaceId &&
      !!this.form.startTime &&
      !!this.form.endTime &&
      this.hasAllowedTimeStep(this.form.startTime) &&
      this.hasAllowedTimeStep(this.form.endTime) &&
      new Date(this.form.startTime) < new Date(this.form.endTime)
    );
  }

  private validateForm(): boolean {
    if (this.canManageAll() && !this.form.customerId) {
      this.notificationStore.warning('Selecciona un cliente.');
      return false;
    }

    if (!this.canManageAll() && !this.authStore.customerId()) {
      this.notificationStore.warning('Tu usuario no tiene un cliente asociado.');
      return false;
    }

    return this.validateSchedule(true);
  }

  private validateSchedule(showFeedback = true): boolean {
    if (!this.form.spaceId) {
      if (showFeedback) {
        this.notificationStore.warning('Selecciona un espacio.');
      }

      return false;
    }

    if (!this.form.startTime || !this.form.endTime) {
      if (showFeedback) {
        this.notificationStore.warning('Ingresa fecha y hora de inicio y fin.');
      }

      return false;
    }

    if (
      !this.hasAllowedTimeStep(this.form.startTime) ||
      !this.hasAllowedTimeStep(this.form.endTime)
    ) {
      if (showFeedback) {
        this.notificationStore.warning('Las reservas deben usar horarios en bloques de 30 minutos.');
      }

      return false;
    }

    if (new Date(this.form.startTime) >= new Date(this.form.endTime)) {
      if (showFeedback) {
        this.notificationStore.warning('La hora de inicio debe ser menor que la hora de fin.');
      }

      return false;
    }

    return true;
  }

  private resetFormModal(): void {
    this.clearPricingPreviewTimeout();
    this.isFormModalOpen.set(false);
    this.wasSubmitted.set(false);
    this.pricingPreview.set(null);
    this.form = this.createEmptyForm();
  }

  private createEmptyForm(): CreateReservationRequest {
    return {
      spaceId: '',
      customerId: '',
      startTime: '',
      endTime: ''
    };
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

  private clearPricingPreviewTimeout(): void {
    if (this.pricingPreviewTimeoutId) {
      clearTimeout(this.pricingPreviewTimeoutId);
      this.pricingPreviewTimeoutId = null;
    }
  }

  getSelectedSpaceHourlyRate(): number | null {
    const space = this.spaces().find(space => space.id === this.form.spaceId);
    return space?.baseHourlyRate ?? null;
  }

  getReservationDurationLabel(): string {
    if (!this.form.startTime || !this.form.endTime) {
      return 'Pendiente';
    }

    const start = new Date(this.form.startTime);
    const end = new Date(this.form.endTime);

    if (start >= end) {
      return 'Pendiente';
    }

    const durationInHours = (end.getTime() - start.getTime()) / 1000 / 60 / 60;

    if (Number.isInteger(durationInHours)) {
      return `${durationInHours} h`;
    }

    return `${durationInHours.toFixed(1)} h`;
  }
}
