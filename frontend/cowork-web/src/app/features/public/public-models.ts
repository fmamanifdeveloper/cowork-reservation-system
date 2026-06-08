export type SpaceStatus = 'Active' | 'Maintenance' | 'Inactive';
export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';

export interface PublicSpace {
    id: string;
    name: string;
    capacity: number;
    baseHourlyRate: number;
    openingTime: string;
    closingTime: string;
    timeZoneId: string;
    status: SpaceStatus;
}

export interface PricingPreviewRequest {
    spaceId: string;
    startTime: string;
    endTime: string;
}

export interface PricingPreviewResponse {
    baseAmount: number;
    finalAmount: number;
    adjustments: PricingAdjustment[];
}

export interface PricingAdjustment {
    ruleName: string;
    percentage: number;
    amountBefore: number;
    amountAfter: number;
}

export interface PublicCreateReservationRequest {
    spaceId: string;
    customerFullName: string;
    customerEmail: string | null;
    customerPhone: string | null;
    customerDocumentNumber: string | null;
    startTime: string;
    endTime: string;
}

export interface PublicReservationResponse {
    reservationId: string;
    reservationCode: string;
    customerId: string;
    spaceId: string;
    startTime: string;
    endTime: string;
    status: ReservationStatus;
    baseAmount: number;
    finalAmount: number;
    pricingBreakdown: string;
}