import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';

@Injectable({
  providedIn: 'root',
})
export class PricingApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/public/pricing`;

  preview(request: unknown) {
    return this.http.post(`${this.baseUrl}/preview`, request);
  }
}
