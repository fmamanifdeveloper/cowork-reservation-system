import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';
import { Reservation, CreateReservationRequest } from '@core/models/reservation';

@Injectable({
  providedIn: 'root',
})
export class ReservationsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/reservations`;

  list() {
    return this.http.get<Reservation[]>(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get<Reservation>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateReservationRequest) {
    return this.http.post<Reservation>(this.baseUrl, request);
  }

  cancel(id: string) {
    return this.http.post<Reservation>(`${this.baseUrl}/${id}/cancel`, {});
  }

  complete(id: string) {
    return this.http.post<Reservation>(`${this.baseUrl}/${id}/complete`, {});
  }
}
