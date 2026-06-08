export interface ReportsDashboard {
  dateFrom: string;
  dateTo: string;
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
  occupancyRatePercent: number;
}

export interface HourlyDemandItem {
  hour: number;
  reservationCount: number;
}