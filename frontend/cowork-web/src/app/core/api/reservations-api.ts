import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { CreateReservationRequest, Reservation } from '@core/models/reservation';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class ReservationsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/reservations`;

  list() {
    return this.http.get<Reservation[]>(this.baseUrl);
  }

  create(request: CreateReservationRequest) {
    return this.http.post<Reservation>(this.baseUrl, request);
  }

  cancel(id: string) {
    return this.http.post<Reservation>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
