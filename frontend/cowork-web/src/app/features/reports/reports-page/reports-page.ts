import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReportApi } from '@core/api/report-api';
import { HourlyDemandItem, ReportsDashboard, SpaceReportItem } from '@core/models/report';
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

  readonly filters = signal({
    from: this.getDefaultFromDate(),
    to: this.getDefaultToDate()
  });

  readonly maxReservationCountBySpace = computed(() => {
    const spaces = this.report()?.spaces ?? [];
    return Math.max(...spaces.map(space => space.reservationCount), 1);
  });

  readonly maxRevenueBySpace = computed(() => {
    const spaces = this.report()?.spaces ?? [];
    return Math.max(...spaces.map(space => space.revenue), 1);
  });

  readonly maxHourlyDemand = computed(() => {
    const hourlyDemand = this.report()?.hourlyDemand ?? [];
    return Math.max(...hourlyDemand.map(item => item.reservationCount), 1);
  });

  ngOnInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    const { from, to } = this.filters();

    if (from && to && from > to) {
      this.notificationStore.warning('La fecha inicial debe ser menor o igual que la fecha final.');
      return;
    }

    this.isLoading.set(true);

    this.reportApi
      .getDashboard(from, to)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: report => this.report.set(report),
        error: () => this.notificationStore.error('No se pudo cargar el reporte.')
      });
  }

  updateFrom(value: string): void {
    this.filters.update(current => ({
      ...current,
      from: value
    }));
  }

  updateTo(value: string): void {
    this.filters.update(current => ({
      ...current,
      to: value
    }));
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('es-PE', {
      style: 'currency',
      currency: 'PEN',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2
    }).format(value);
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  formatHour(hour: number | null): string {
    if (hour === null || hour === undefined) {
      return 'Sin datos';
    }

    return `${hour.toString().padStart(2, '0')}:00`;
  }

  getReservationBarWidth(space: SpaceReportItem): string {
    const percent = space.reservationCount / this.maxReservationCountBySpace() * 100;
    return `${Math.max(percent, 3)}%`;
  }

  getRevenueBarWidth(space: SpaceReportItem): string {
    const percent = space.revenue / this.maxRevenueBySpace() * 100;
    return `${Math.max(percent, 3)}%`;
  }

  getOccupancyBarWidth(space: SpaceReportItem): string {
    return `${Math.min(Math.max(space.occupancyRatePercent, 0), 100)}%`;
  }

  getHourlyDemandBarWidth(item: HourlyDemandItem): string {
    const percent = item.reservationCount / this.maxHourlyDemand() * 100;
    return `${Math.max(percent, 3)}%`;
  }

  private getDefaultFromDate(): string {
    const date = new Date();
    date.setDate(date.getDate() - 30);

    return this.toInputDate(date);
  }

  private getDefaultToDate(): string {
    return this.toInputDate(new Date());
  }

  private toInputDate(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }
}
