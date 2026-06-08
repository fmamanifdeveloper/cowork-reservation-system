import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';

export type SpaceStatus = 'Active' | 'Maintenance' | 'Inactive';

export interface Space {
  id: string;
  name: string;
  capacity: number;
  baseHourlyRate: number;
  openingTime: string;
  closingTime: string;
  timeZoneId: string;
  status: SpaceStatus;
}

export interface CreateSpaceRequest {
  name: string;
  capacity: number;
  baseHourlyRate: number;
  openingTime: string;
  closingTime: string;
  timeZoneId: string;
  status: SpaceStatus;
}

export type UpdateSpaceRequest = CreateSpaceRequest;

@Injectable({
  providedIn: 'root',
})
export class SpacesApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${API_BASE_URL}/spaces`;

  list() {
    return this.http.get<Space[]>(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get<Space>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateSpaceRequest) {
    return this.http.post<Space>(this.baseUrl, request);
  }

  update(id: string, request: UpdateSpaceRequest) {
    return this.http.put<Space>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
