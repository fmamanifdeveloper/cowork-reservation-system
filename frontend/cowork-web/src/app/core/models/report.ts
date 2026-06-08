export interface ReportsDashboard {
  from: string;
  to: string;
  totalReservations: number;
  pendingReservations: number;
  confirmedReservations: number;
  cancelledReservations: number;
  completedReservations: number;
  totalRevenue: number;
  totalRefundAmount: number;
  mostReservedSpaceName: string | null;
  mostDemandedHour: number | null;
  spaces: SpaceReportItem[];
  hourlyDemand: HourlyDemandItem[];
}

export interface SpaceReportItem {
  spaceId: string;
  spaceName: string;
  reservationCount: number;
  revenue: number;
}

export interface HourlyDemandItem {
  hour: number;
  reservationCount: number;
}