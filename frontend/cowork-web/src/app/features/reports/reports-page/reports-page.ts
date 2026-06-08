import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReportApi } from '@core/api/report-api';
import { ReportsDashboard } from '@core/models/report';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-reports-page',
  imports: [FormsModule],
  templateUrl: './reports-page.html',
  styleUrl: './reports-page.scss',
})
export class ReportsPage {
  private readonly reportApi = inject(ReportApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly report = signal<ReportsDashboard | null>(null);
  readonly isLoading = signal(false);

  from = '';
  to = '';

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.isLoading.set(true);

    this.reportApi
      .getDashboard(this.toIsoOrUndefined(this.from), this.toIsoOrUndefined(this.to))
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: report => this.report.set(report),
        error: () => this.notificationStore.error('No se pudo cargar el reporte.')
      });
  }

  clearFilters(): void {
    this.from = '';
    this.to = '';
    this.loadReport();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN'
    }).format(value);
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  private toIsoOrUndefined(value: string): string | undefined {
    if (!value) {
      return undefined;
    }

    return new Date(value).toISOString();
  }
}
