export type SpaceStatus = 'Active' | 'Maintenance' | 'Inactive';

export interface Space {
  id: string;
  name: string;
  capacity: number;
  baseHourlyRate: number;
  openingTime: string;
  closingTime: string;
  timeZoneId: string;
  status: SpaceStatus;
}

export interface CreateSpaceRequest {
  name: string;
  capacity: number;
  baseHourlyRate: number;
  openingTime: string;
  closingTime: string;
  timeZoneId: string;
  status: SpaceStatus;
}

export type UpdateSpaceRequest = CreateSpaceRequest;