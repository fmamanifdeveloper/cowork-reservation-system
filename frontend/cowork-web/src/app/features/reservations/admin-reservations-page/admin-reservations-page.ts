import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CustomersApi } from '@core/api/customers-api';
import { ReservationsApi } from '@core/api/reservations-api';
import { AuthStore } from '@core/auth/auth-store';
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

  createReservation(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);

    const request: CreateReservationRequest = {
      spaceId: this.form.spaceId,
      customerId: this.canManageAll()
        ? this.form.customerId
        : this.authStore.customerId() ?? this.form.customerId,
      startTime: this.toPeruOffsetDateTime(this.form.startTime),
      endTime: this.toPeruOffsetDateTime(this.form.endTime)
    };

    this.reservationsApi
      .create(request)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.notificationStore.success('Reserva creada correctamente.');
          this.form = this.createEmptyForm();
          this.loadInitialData();
        },
        error: error => {
          if (error.status === 409) {
            this.notificationStore.error('El espacio ya está reservado en ese horario.');
            return;
          }

          if (error.status === 400) {
            this.notificationStore.error(error.error?.message ?? 'La reserva no cumple las reglas requeridas.');
            return;
          }

          this.notificationStore.error('No se pudo crear la reserva.');
        }
      });
  }

  cancelReservation(reservation: Reservation): void {
    const confirmed = window.confirm(
      `¿Cancelar la reserva ${reservation.reservationCode}?`
    );

    if (!confirmed) {
      return;
    }

    this.reservationsApi.cancel(reservation.id).subscribe({
      next: () => {
        this.notificationStore.success('Reserva cancelada correctamente.');
        this.loadReservations();
      },
      error: () => this.notificationStore.error('No se pudo cancelar la reserva.')
    });
  }

  completeReservation(reservation: Reservation): void {
    const confirmed = window.confirm(
      `¿Completar la reserva ${reservation.reservationCode}?`
    );

    if (!confirmed) {
      return;
    }

    this.reservationsApi.complete(reservation.id).subscribe({
      next: () => {
        this.notificationStore.success('Reserva completada correctamente.');
        this.loadReservations();
      },
      error: () => this.notificationStore.error('No se pudo completar la reserva.')
    });
  }

  canCancel(reservation: Reservation): boolean {
    return reservation.status !== 'Cancelled' && reservation.status !== 'Completed';
  }

  canComplete(reservation: Reservation): boolean {
    return this.canManageAll() && reservation.status !== 'Cancelled' && reservation.status !== 'Completed';
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

  private validateForm(): boolean {
    if (!this.form.spaceId) {
      this.notificationStore.warning('Selecciona un espacio.');
      return false;
    }

    if (this.canManageAll() && !this.form.customerId) {
      this.notificationStore.warning('Selecciona un cliente.');
      return false;
    }

    if (!this.form.startTime || !this.form.endTime) {
      this.notificationStore.warning('Ingresa fecha y hora de inicio y fin.');
      return false;
    }

    if (new Date(this.form.startTime) >= new Date(this.form.endTime)) {
      this.notificationStore.warning('La hora de inicio debe ser menor que la hora de fin.');
      return false;
    }

    return true;
  }

  private createEmptyForm(): CreateReservationRequest {
    return {
      spaceId: '',
      customerId: '',
      startTime: '',
      endTime: ''
    };
  }

  private toPeruOffsetDateTime(value: string): string {
    return value.length === 16 ? `${value}:00-05:00` : `${value}-05:00`;
  }
}
