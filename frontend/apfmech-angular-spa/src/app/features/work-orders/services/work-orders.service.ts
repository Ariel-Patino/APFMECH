import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateWorkOrderRequest,
  WorkOrderResponse,
} from '../../../core/models/work-order.models';

@Injectable({
  providedIn: 'root',
})
export class WorkOrdersService {
  private readonly workOrdersApiUrl = '/api/workorders';

  constructor(private readonly httpClient: HttpClient) {}

  getAll(): Observable<WorkOrderResponse[]> {
    return this.httpClient.get<WorkOrderResponse[]>(this.workOrdersApiUrl);
  }

  getById(id: string): Observable<WorkOrderResponse> {
    return this.httpClient.get<WorkOrderResponse>(`${this.workOrdersApiUrl}/${id}`);
  }

  create(request: CreateWorkOrderRequest): Observable<WorkOrderResponse> {
    return this.httpClient.post<WorkOrderResponse>(this.workOrdersApiUrl, request);
  }

  assignMechanic(id: string, mechanicId: string): Observable<WorkOrderResponse> {
    return this.httpClient.put<WorkOrderResponse>(
      `${this.workOrdersApiUrl}/${id}/assign`,
      { mechanicId },
    );
  }

  complete(id: string): Observable<WorkOrderResponse> {
    return this.httpClient.put<WorkOrderResponse>(
      `${this.workOrdersApiUrl}/${id}/complete`,
      {},
    );
  }
}