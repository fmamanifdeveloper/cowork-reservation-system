import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { API_BASE_URL } from './api-config';

export interface Customer {
    id: string;
    fullName: string;
    email: string | null;
    phone: string | null;
    documentNumber: string | null;
}

export interface CreateCustomerRequest {
    fullName: string;
    email: string | null;
    phone: string | null;
    documentNumber: string | null;
}

export type UpdateCustomerRequest = CreateCustomerRequest;

@Injectable({
    providedIn: 'root'
})
export class CustomersApi {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${API_BASE_URL}/customers`;

    list() {
        return this.http.get<Customer[]>(this.baseUrl);
    }

    getById(id: string) {
        return this.http.get<Customer>(`${this.baseUrl}/${id}`);
    }

    create(request: CreateCustomerRequest) {
        return this.http.post<Customer>(this.baseUrl, request);
    }

    update(id: string, request: UpdateCustomerRequest) {
        return this.http.put<Customer>(`${this.baseUrl}/${id}`, request);
    }

    delete(id: string) {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }
}