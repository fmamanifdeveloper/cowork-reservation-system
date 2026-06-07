import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { PricingPreviewRequest, PricingPreviewResponse } from '@core/models/pricing';
import { environment } from '@env/environment';

@Injectable({
  providedIn: 'root',
})
export class PricingApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/pricing`;

  preview(request: PricingPreviewRequest) {
    return this.http.post<PricingPreviewResponse>(`${this.baseUrl}/preview`, request);
  }
}
