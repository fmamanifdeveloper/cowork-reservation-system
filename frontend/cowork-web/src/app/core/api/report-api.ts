import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ReportsResponse } from '@core/models/report';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class ReportApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reports`;

  get(from: string, to: string) {
    const params = new HttpParams()
      .set('from', from)
      .set('to', to);

    return this.http.get<ReportsResponse>(this.baseUrl, { params });
  }
}
