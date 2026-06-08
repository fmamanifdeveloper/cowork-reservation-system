export interface PublicSpace {
  id: string;
  name: string;
  capacity: number;
  baseHourlyRate: number;
  openingTime: string;
  closingTime: string;
  timeZoneId: string;
  status: string;
}

export interface PublicPricingPreviewRequest {
  spaceId: string;
  startTime: string;
  endTime: string;
}

export interface PublicPricingPreviewResponse {
  baseAmount: number;
  finalAmount: number;
  adjustments: PublicPricingAdjustment[];
}

export interface PublicPricingAdjustment {
  name: string;
  amount: number;
  description?: string | null;
}

export interface CreatePublicReservationRequest {
  spaceId: string;
  customerFullName: string;
  customerEmail: string;
  customerPhone: string;
  customerDocumentNumber: string | null;
  startTime: string;
  endTime: string;
}

export interface PublicReservationResponse {
  id: string;
  reservationCode: string;
  spaceId: string;
  customerId: string;
  startTime: string;
  endTime: string;
  status: string;
  baseAmount: number;
  finalAmount: number;
}