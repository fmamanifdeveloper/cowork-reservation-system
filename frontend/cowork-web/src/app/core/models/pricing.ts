export interface PricingPreviewRequest {
  spaceId: string;
  startTime: string;
  endTime: string;
}

export interface PricingAdjustment {
  rule: string;
  percentage: number;
  description: string;
  amountBefore: number;
  amountAfter: number;
}

export interface PricingPreviewResponse {
  baseAmount: number;
  finalAmount: number;
  adjustments: PricingAdjustment[];
}