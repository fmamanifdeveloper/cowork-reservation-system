export type ReservationStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed';

export interface CreateReservationRequest {
  spaceId: string;
  startTime: string;
  endTime: string;
}

export interface Reservation {
  id: string;
  spaceId: string;
  startTime: string;
  endTime: string;
  status: ReservationStatus;
  baseAmount: number;
  finalAmount: number;
  refundAmount: number | null;
  createdAt: string;
  cancelledAt: string | null;
  completedAt: string | null;
}