import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';

@Injectable({
  providedIn: 'root',
})
export class ReservationsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/reservations`;

  list() {
    return this.http.get(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get(`${this.baseUrl}/${id}`);
  }

  create(request: unknown) {
    return this.http.post(this.baseUrl, request);
  }

  cancel(id: string) {
    return this.http.post(`${this.baseUrl}/${id}/cancel`, {});
  }

  complete(id: string) {
    return this.http.post(`${this.baseUrl}/${id}/complete`, {});
  }
}
