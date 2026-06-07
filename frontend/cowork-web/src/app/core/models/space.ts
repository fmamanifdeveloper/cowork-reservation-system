export type SpaceStatus = 'Active' | 'Maintenance';

export interface Space {
  id: string;
  name: string;
  capacity: number;
  baseHourlyRate: number;
  openingTime: string;
  closingTime: string;
  status: SpaceStatus;
}