export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';

export interface Reservation {
  id: string;
  reservationCode: string;
  spaceId: string;
  customerId: string;
  startTime: string;
  endTime: string;
  status: ReservationStatus;
  baseAmount: number;
  finalAmount: number;
  refundAmount: number | null;
  pricingBreakdown: string;
  createdAt: string;
  updatedAt: string | null;
  cancelledAt: string | null;
  completedAt: string | null;
}

export interface CreateReservationRequest {
  spaceId: string;
  customerId: string;
  startTime: string;
  endTime: string;
}
