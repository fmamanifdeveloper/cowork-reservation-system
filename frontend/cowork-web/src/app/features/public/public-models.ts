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
    rule: string;
    percentage: number;
    description: string;
    amountBefore: number;
    amountAfter: number;
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

export interface PublicAvailabilityResponse {
    spaceId: string;
    spaceName: string;
    date: string;
    openingTime: string;
    closingTime: string;
    timeZoneId: string;
    slots: PublicAvailabilitySlot[];
    reservedSlots: PublicReservedSlot[];
}

export interface PublicAvailabilitySlot {
    startTime: string;
    endTime: string;
    isAvailable: boolean;
}

export interface PublicReservedSlot {
    startTime: string;
    endTime: string;
}
