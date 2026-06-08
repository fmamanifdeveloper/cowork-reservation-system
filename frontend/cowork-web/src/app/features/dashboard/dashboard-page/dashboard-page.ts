import { Component, inject, signal } from '@angular/core';
import { ReportApi } from '@core/api/report-api';
import { ReportsDashboard } from '@core/models/report';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-dashboard-page',
  imports: [],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  private readonly reportApi = inject(ReportApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly dashboard = signal<ReportsDashboard | null>(null);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading.set(true);

    this.reportApi
      .getDashboard()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: dashboard => this.dashboard.set(dashboard),
        error: () => this.notificationStore.error('No se pudo cargar el dashboard.')
      });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN'
    }).format(value);
  }

  formatHour(hour: number | null): string {
    if (hour === null) {
      return 'Sin datos';
    }

    return `${hour.toString().padStart(2, '0')}:00`;
  }
}
