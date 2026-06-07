import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Space } from '@core/models/space';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class SpacesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/spaces`;

  list() {
    return this.http.get<Space[]>(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get<Space>(`${this.baseUrl}/${id}`);
  }
}
