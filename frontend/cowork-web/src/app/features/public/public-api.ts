import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import {
    PricingPreviewRequest,
    PricingPreviewResponse,
    PublicCreateReservationRequest,
    PublicReservationResponse,
    PublicSpace
} from './public-models';
import { API_BASE_URL } from '@core/api/api-config';

@Injectable({
    providedIn: 'root'
})
export class PublicApi {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${API_BASE_URL}/public`;

    listSpaces() {
        return this.http.get<PublicSpace[]>(`${this.baseUrl}/spaces`);
    }

    previewPricing(request: PricingPreviewRequest) {
        return this.http.post<PricingPreviewResponse>(`${this.baseUrl}/pricing/preview`, request);
    }

    createReservation(request: PublicCreateReservationRequest) {
        return this.http.post<PublicReservationResponse>(`${this.baseUrl}/reservations`, request);
    }
}