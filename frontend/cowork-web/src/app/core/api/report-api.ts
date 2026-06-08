import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';
import { ReportsDashboard } from '@core/models/report';

@Injectable({
  providedIn: 'root',
})
export class ReportApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/reports`;

  getDashboard(from?: string, to?: string) {
    const params: Record<string, string> = {};

    if (from) {
      params['from'] = from;
    }

    if (to) {
      params['to'] = to;
    }

    return this.http.get<ReportsDashboard>(this.baseUrl, { params });
  }
}
