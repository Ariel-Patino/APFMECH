import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  AssignMechanicRequest,
  CreateWorkOrderRequest,
  WorkOrderResponse,
} from '../models/work-order.models';

@Injectable({
  providedIn: 'root',
})
export class WorkOrdersService {
  private readonly apiUrl = '/api/workorders';

  constructor(private readonly httpClient: HttpClient) {}

  create(payload: CreateWorkOrderRequest): Observable<WorkOrderResponse> {
    return this.httpClient.post<WorkOrderResponse>(this.apiUrl, payload);
  }

  getAll(): Observable<WorkOrderResponse[]> {
    return this.httpClient.get<WorkOrderResponse[]>(this.apiUrl);
  }

  getById(id: string): Observable<WorkOrderResponse> {
    return this.httpClient.get<WorkOrderResponse>(`${this.apiUrl}/${id}`);
  }

  assignMechanic(
    id: string,
    payload: AssignMechanicRequest,
  ): Observable<WorkOrderResponse> {
    return this.httpClient.put<WorkOrderResponse>(
      `${this.apiUrl}/${id}/assign`,
      payload,
    );
  }

  complete(id: string): Observable<WorkOrderResponse> {
    return this.httpClient.put<WorkOrderResponse>(
      `${this.apiUrl}/${id}/complete`,
      {},
    );
  }
}