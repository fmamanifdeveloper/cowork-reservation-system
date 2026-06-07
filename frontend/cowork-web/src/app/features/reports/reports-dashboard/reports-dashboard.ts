import { CommonModule } from '@angular/common';
import { Component, ElementRef, inject, signal, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReportApi } from '@core/api/report-api';
import { ReportsResponse } from '@core/models/report';
import { Chart } from 'chart.js';

@Component({
  selector: 'app-reports-dashboard',
  imports: [CommonModule, FormsModule],
  templateUrl: './reports-dashboard.html',
  styleUrl: './reports-dashboard.scss',
})
export class ReportsDashboard {
  private readonly reportApi = inject(ReportApi);
  private chart: Chart | null = null;

  @ViewChild('incomeChart')
  private readonly incomeChart?: ElementRef<HTMLCanvasElement>;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly report = signal<ReportsResponse | null>(null);

  from = '2026-06-01T00:00';
  to = '2026-06-30T23:59';

  ngAfterViewInit(): void {
    this.loadReport();
  }

  loadReport(): void {
    this.loading.set(true);
    this.error.set(null);

    this.reportApi.get(this.toApiDateTime(this.from), this.toApiDateTime(this.to)).subscribe({
      next: (report) => {
        this.report.set(report);
        this.loading.set(false);

        setTimeout(() => this.renderChart(report));
      },
      error: (error) => {
        this.error.set(error.error?.detail ?? 'No se pudo cargar el reporte.');
        this.loading.set(false);
      }
    });
  }

  private renderChart(report: ReportsResponse): void {
    const canvas = this.incomeChart?.nativeElement;

    if (!canvas) {
      return;
    }

    this.chart?.destroy();

    this.chart = new Chart(canvas, {
      type: 'bar',
      data: {
        labels: report.spaces.map(space => space.spaceName),
        datasets: [
          {
            label: 'Ingresos por espacio',
            data: report.spaces.map(space => space.income)
          }
        ]
      },
      options: {
        responsive: true,
        plugins: {
          legend: {
            display: true
          }
        },
        scales: {
          y: {
            beginAtZero: true
          }
        }
      }
    });
  }

  private toApiDateTime(value: string): string {
    return value.length === 16 ? `${value}:00Z` : value;
  }
}