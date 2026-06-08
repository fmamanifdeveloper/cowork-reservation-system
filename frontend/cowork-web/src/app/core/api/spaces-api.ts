import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';

@Injectable({
  providedIn: 'root',
})
export class SpacesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/spaces`;

  list() {
    return this.http.get(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get(`${this.baseUrl}/${id}`);
  }

  create(request: unknown) {
    return this.http.post(this.baseUrl, request);
  }

  update(id: string, request: unknown) {
    return this.http.put(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
