import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SpacesApi } from '@core/api/spaces-api';
import { ApiErrorTranslator } from '@core/errors/api-error-translator';
import { Space, CreateSpaceRequest } from '@core/models/space';
import { NotificationStore } from '@core/notifications/notification-store';
import { SpaceStatus } from '@features/public/public-models';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-admin-spaces-page',
  imports: [FormsModule],
  templateUrl: './admin-spaces-page.html',
  styleUrl: './admin-spaces-page.scss',
})
export class AdminSpacesPage {
  private readonly spacesApi = inject(SpacesApi);
  private readonly notificationStore = inject(NotificationStore);
  private readonly apiErrorTranslator = inject(ApiErrorTranslator);

  readonly spaces = signal<Space[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isDeleting = signal(false);
  readonly isFormModalOpen = signal(false);
  readonly spaceToDelete = signal<Space | null>(null);
  readonly wasSubmitted = signal(false);

  editingId: string | null = null;

  form: CreateSpaceRequest = this.createEmptyForm();

  readonly statuses: SpaceStatus[] = ['Active', 'Maintenance', 'Inactive'];

  ngOnInit(): void {
    this.loadSpaces();
  }

  loadSpaces(): void {
    this.isLoading.set(true);

    this.spacesApi
      .list()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: spaces => this.spaces.set(spaces),
        error: () => this.notificationStore.error('No se pudieron cargar los espacios.')
      });
  }

  openCreateModal(): void {
    this.editingId = null;
    this.form = this.createEmptyForm();
    this.wasSubmitted.set(false);
    this.isFormModalOpen.set(true);
  }

  openEditModal(space: Space): void {
    this.editingId = space.id;
    this.wasSubmitted.set(false);

    this.form = {
      name: space.name,
      capacity: space.capacity,
      baseHourlyRate: space.baseHourlyRate,
      openingTime: this.toTimeInputValue(space.openingTime),
      closingTime: this.toTimeInputValue(space.closingTime),
      timeZoneId: space.timeZoneId,
      status: space.status
    };

    this.isFormModalOpen.set(true);
  }

  closeFormModal(): void {
    if (this.isSaving()) {
      return;
    }

    this.resetFormModal();
  }

  save(): void {
    this.wasSubmitted.set(true);

    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);

    const request: CreateSpaceRequest = {
      name: this.form.name.trim(),
      capacity: Number(this.form.capacity),
      baseHourlyRate: Number(this.form.baseHourlyRate),
      openingTime: this.toApiTimeValue(this.form.openingTime),
      closingTime: this.toApiTimeValue(this.form.closingTime),
      timeZoneId: this.form.timeZoneId.trim() || 'America/Lima',
      status: this.form.status
    };

    const action = this.editingId
      ? this.spacesApi.update(this.editingId, request)
      : this.spacesApi.create(request);

    action.pipe(finalize(() => this.isSaving.set(false))).subscribe({
      next: () => {
        this.notificationStore.success(
          this.editingId ? 'Espacio actualizado correctamente.' : 'Espacio creado correctamente.'
        );

        this.resetFormModal();
        this.loadSpaces();
      },
      error: error => {
        this.notificationStore.error(this.apiErrorTranslator.translate(error));
      }
    });
  }

  openDeleteModal(space: Space): void {
    this.spaceToDelete.set(space);
  }

  closeDeleteModal(): void {
    if (this.isDeleting()) {
      return;
    }

    this.spaceToDelete.set(null);
  }

  confirmDelete(): void {
    const space = this.spaceToDelete();

    if (!space) {
      return;
    }

    this.isDeleting.set(true);

    this.spacesApi
      .delete(space.id)
      .pipe(finalize(() => this.isDeleting.set(false)))
      .subscribe({
        next: () => {
          this.notificationStore.success('Espacio eliminado correctamente.');
          this.spaceToDelete.set(null);
          this.loadSpaces();
        },
        error: () => this.notificationStore.error('No se pudo eliminar el espacio.')
      });
  }

  getStatusLabel(status: SpaceStatus): string {
    const labels: Record<SpaceStatus, string> = {
      Active: 'Activo',
      Maintenance: 'Mantenimiento',
      Inactive: 'Inactivo'
    };

    return labels[status];
  }

  isNameInvalid(): boolean {
    return this.wasSubmitted() && !this.form.name.trim();
  }

  isCapacityInvalid(): boolean {
    return this.wasSubmitted() && Number(this.form.capacity) <= 0;
  }

  isBaseHourlyRateInvalid(): boolean {
    return this.wasSubmitted() && Number(this.form.baseHourlyRate) <= 0;
  }

  isOpeningTimeInvalid(): boolean {
    return this.wasSubmitted() && !this.form.openingTime;
  }

  isClosingTimeInvalid(): boolean {
    return this.wasSubmitted() && !this.form.closingTime;
  }

  isScheduleInvalid(): boolean {
    return (
      this.wasSubmitted() &&
      !!this.form.openingTime &&
      !!this.form.closingTime &&
      this.form.openingTime >= this.form.closingTime
    );
  }

  isTimeZoneInvalid(): boolean {
    return this.wasSubmitted() && !this.form.timeZoneId.trim();
  }

  private validateForm(): boolean {
    if (!this.form.name.trim()) {
      this.notificationStore.warning('Ingresa el nombre del espacio.');
      return false;
    }

    if (Number(this.form.capacity) <= 0) {
      this.notificationStore.warning('La capacidad debe ser mayor a cero.');
      return false;
    }

    if (Number(this.form.baseHourlyRate) <= 0) {
      this.notificationStore.warning('La tarifa debe ser mayor a cero.');
      return false;
    }

    if (!this.form.openingTime || !this.form.closingTime) {
      this.notificationStore.warning('Ingresa el horario de apertura y cierre.');
      return false;
    }

    if (this.form.openingTime >= this.form.closingTime) {
      this.notificationStore.warning('La hora de apertura debe ser menor que la hora de cierre.');
      return false;
    }

    if (!this.form.timeZoneId.trim()) {
      this.notificationStore.warning('Ingresa la zona horaria.');
      return false;
    }

    return true;
  }

  private resetFormModal(): void {
    this.isFormModalOpen.set(false);
    this.editingId = null;
    this.wasSubmitted.set(false);
    this.form = this.createEmptyForm();
  }

  private createEmptyForm(): CreateSpaceRequest {
    return {
      name: '',
      capacity: 1,
      baseHourlyRate: 50,
      openingTime: '08:00',
      closingTime: '20:00',
      timeZoneId: 'America/Lima',
      status: 'Active'
    };
  }

  private toApiTimeValue(value: string): string {
    return value.length === 5 ? `${value}:00` : value;
  }

  private toTimeInputValue(value: string): string {
    return value.length >= 5 ? value.substring(0, 5) : value;
  }
}
