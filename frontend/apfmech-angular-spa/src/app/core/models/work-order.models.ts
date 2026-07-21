export interface WorkOrderResponse {
  id: string;
  trackingNumber: string;
  description: string;
  status: string;
  assignedMechanicId: string | null;
  assignedMechanicFullName: string | null;
  createdAtUtc: string;
}

export interface CreateWorkOrderRequest {
  description: string;
}

export interface AssignMechanicRequest {
  mechanicId: string;
}