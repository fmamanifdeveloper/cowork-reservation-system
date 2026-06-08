import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SpacesApi } from '@core/api/spaces-api';
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

  readonly spaces = signal<Space[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);

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

  save(): void {
    if (!this.validateForm()) {
      return;
    }

    this.isSaving.set(true);

    const request: CreateSpaceRequest = {
      ...this.form,
      capacity: Number(this.form.capacity),
      baseHourlyRate: Number(this.form.baseHourlyRate)
    };

    const action = this.editingId
      ? this.spacesApi.update(this.editingId, request)
      : this.spacesApi.create(request);

    action.pipe(finalize(() => this.isSaving.set(false))).subscribe({
      next: () => {
        this.notificationStore.success(
          this.editingId ? 'Espacio actualizado correctamente.' : 'Espacio creado correctamente.'
        );

        this.cancelEdit();
        this.loadSpaces();
      },
      error: error => {
        if (error.status === 409) {
          this.notificationStore.error('Ya existe un espacio con ese nombre.');
          return;
        }

        this.notificationStore.error('No se pudo guardar el espacio.');
      }
    });
  }

  edit(space: Space): void {
    this.editingId = space.id;

    this.form = {
      name: space.name,
      capacity: space.capacity,
      baseHourlyRate: space.baseHourlyRate,
      openingTime: space.openingTime,
      closingTime: space.closingTime,
      timeZoneId: space.timeZoneId,
      status: space.status
    };

    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  delete(space: Space): void {
    const confirmed = window.confirm(`¿Eliminar el espacio "${space.name}"?`);

    if (!confirmed) {
      return;
    }

    this.spacesApi.delete(space.id).subscribe({
      next: () => {
        this.notificationStore.success('Espacio eliminado correctamente.');
        this.loadSpaces();
      },
      error: () => this.notificationStore.error('No se pudo eliminar el espacio.')
    });
  }

  cancelEdit(): void {
    this.editingId = null;
    this.form = this.createEmptyForm();
  }

  getStatusLabel(status: SpaceStatus): string {
    const labels: Record<SpaceStatus, string> = {
      Active: 'Activo',
      Maintenance: 'Mantenimiento',
      Inactive: 'Inactivo'
    };

    return labels[status];
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

    return true;
  }

  private createEmptyForm(): CreateSpaceRequest {
    return {
      name: '',
      capacity: 1,
      baseHourlyRate: 50,
      openingTime: '08:00:00',
      closingTime: '20:00:00',
      timeZoneId: 'America/Lima',
      status: 'Active'
    };
  }
}
