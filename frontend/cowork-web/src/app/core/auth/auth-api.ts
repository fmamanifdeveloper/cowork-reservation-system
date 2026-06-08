import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { LoginRequest, LoginResponse } from './auth-models';
import { API_BASE_URL } from '@core/api/api-config';

@Injectable({
    providedIn: 'root'
})
export class AuthApi {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${API_BASE_URL}/auth`;

    login(request: LoginRequest) {
        return this.http.post<LoginResponse>(`${this.baseUrl}/login`, request);
    }

    me() {
        return this.http.get(`${this.baseUrl}/me`);
    }
}