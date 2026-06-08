import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';
import { ReportsDashboard } from '@core/models/report';

@Injectable({
  providedIn: 'root',
})
export class ReportApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/reports`;

  getDashboard(from?: string | null, to?: string | null) {
    let params = new HttpParams();

    if (from) {
      params = params.set('from', `${from}T00:00:00-05:00`);
    }

    if (to) {
      params = params.set('to', `${to}T23:59:59-05:00`);
    }

    return this.http.get<ReportsDashboard>(this.baseUrl, { params });
  }
}
