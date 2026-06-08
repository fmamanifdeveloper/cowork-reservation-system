import { Component, inject, signal } from '@angular/core';
import { AuditLogsApi } from '@core/api/audit-logs-api';
import { AuditLog } from '@core/models/audit-log';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-audit-logs-page',
  imports: [],
  templateUrl: './audit-logs-page.html',
  styleUrl: './audit-logs-page.scss',
})
export class AuditLogsPage {
  private readonly auditLogsApi = inject(AuditLogsApi);
  private readonly notificationStore = inject(NotificationStore);

  readonly auditLogs = signal<AuditLog[]>([]);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadAuditLogs();
  }

  loadAuditLogs(): void {
    this.isLoading.set(true);

    this.auditLogsApi
      .list()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: logs => this.auditLogs.set(logs),
        error: () => this.notificationStore.error('No se pudo cargar la auditoría.')
      });
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat('es-PE', {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }
}
