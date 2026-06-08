import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomersApi } from '@core/api/customers-api';
import { ReservationsApi } from '@core/api/reservations-api';
import { AuthStore } from '@core/auth/auth-store';
import { ApiErrorTranslator } from '@core/errors/api-error-translator';
import { Customer } from '@core/models/customer';
import { Reservation, CreateReservationRequest } from '@core/models/reservation';
import { NotificationStore } from '@core/notifications/notification-store';
import { PublicApi } from '@features/public/public-api';
import { PublicSpace, ReservationStatus } from '@features/public/public-models';
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

  readonly reservations = signal<Reservation[]>([]);
  readonly customers = signal<Customer[]>([]);
  readonly spaces = signal<PublicSpace[]>([]);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isCancelling = signal(false);
  readonly isCompleting = signal(false);

  readonly isFormModalOpen = signal(false);
  readonly reservationToCancel = signal<Reservation | null>(null);
  readonly reservationToComplete = signal<Reservation | null>(null);
  readonly wasSubmitted = signal(false);

  readonly canManageAll = computed(() =>
    this.authStore.hasAnyRole(['Admin', 'Staff'])
  );

  form: CreateReservationRequest = this.createEmptyForm();

  ngOnInit(): void {
    this.loadInitialData();
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

    this.wasSubmitted.set(false);
    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    if (this.isSaving()) {
      return;
    }

    this.resetFormModal();
  }

  createReservation(): void {
    this.wasSubmitted.set(true);

    if (!this.validateForm()) {
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
      currency: 'PEN'
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

  private validateForm(): boolean {
    if (!this.form.spaceId) {
      this.notificationStore.warning('Selecciona un espacio.');
      return false;
    }

    if (this.canManageAll() && !this.form.customerId) {
      this.notificationStore.warning('Selecciona un cliente.');
      return false;
    }

    if (!this.canManageAll() && !this.authStore.customerId()) {
      this.notificationStore.warning('Tu usuario no tiene un cliente asociado.');
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

  private resetFormModal(): void {
    this.isFormModalOpen.set(false);
    this.wasSubmitted.set(false);
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
}
