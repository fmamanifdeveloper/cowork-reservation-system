import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationStore } from '@core/notifications/notification-store';
import { PublicApi } from '../public-api';
import { PublicSpace } from '../public-models';

@Component({
  selector: 'app-public-spaces-page',
  imports: [RouterLink],
  templateUrl: './public-spaces-page.html',
  styleUrl: './public-spaces-page.scss',
})
export class PublicSpacesPage {
  private readonly publicApi = inject(PublicApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly spaces = signal<PublicSpace[]>([]);
  readonly isLoading = signal(false);

  readonly availableSpacesCount = computed(() =>
    this.spaces().filter(space => space.status === 'Active').length
  );

  readonly totalCapacity = computed(() =>
    this.spaces().reduce((total, space) => total + space.capacity, 0)
  );

  ngOnInit(): void {
    this.loadSpaces();
  }

  loadSpaces(): void {
    this.isLoading.set(true);

    this.publicApi.listSpaces().subscribe({
      next: spaces => {
        this.spaces.set(spaces);
        this.isLoading.set(false);
      },
      error: () => {
        this.notificationStore.error('No se pudieron cargar los espacios disponibles.');
        this.isLoading.set(false);
      }
    });
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      Active: 'Disponible',
      Maintenance: 'Mantenimiento',
      Inactive: 'No disponible'
    };

    return labels[status] ?? status;
  }

  getCapacityLabel(capacity: number): string {
    return capacity === 1 ? '1 persona' : `${capacity} personas`;
  }

  formatHourlyRate(value: number): string {
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

  isAvailable(space: PublicSpace): boolean {
    return space.status === 'Active';
  }
}
