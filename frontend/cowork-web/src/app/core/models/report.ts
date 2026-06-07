export interface SpaceOccupancyReport {
  spaceId: string;
  spaceName: string;
  occupancyRatePercentage: number;
  income: number;
}

export interface ReportsResponse {
  from: string;
  to: string;
  totalIncome: number;
  mostDemandedHour: string | null;
  spaces: SpaceOccupancyReport[];
}