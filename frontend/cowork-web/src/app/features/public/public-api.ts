import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { API_BASE_URL } from '@core/api/api-config';
import { CreatePublicReservationRequest, PublicPricingPreviewRequest, PublicPricingPreviewResponse, PublicReservationResponse, PublicSpace } from './public-models';

@Injectable({
    providedIn: 'root'
})
export class PublicApi {
    private readonly http = inject(HttpClient);
    private readonly baseUrl = `${API_BASE_URL}/public`;

    listSpaces() {
        return this.http.get<PublicSpace[]>(`${this.baseUrl}/spaces`);
    }

    previewPricing(request: PublicPricingPreviewRequest) {
        return this.http.post<PublicPricingPreviewResponse>(
            `${this.baseUrl}/pricing/preview`,
            request
        );
    }

    createReservation(request: CreatePublicReservationRequest) {
        return this.http.post<PublicReservationResponse>(
            `${this.baseUrl}/reservations`,
            request
        );
    }
}