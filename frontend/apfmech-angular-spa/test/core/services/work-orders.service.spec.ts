import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';

import {
  AssignMechanicRequest,
  CreateWorkOrderRequest,
  WorkOrderResponse,
} from '../../../src/app/core/models/work-order.models';
import { WorkOrdersService } from '../../../src/app/core/services/work-orders.service';

describe('WorkOrdersService', () => {
  let service: WorkOrdersService;
  let httpMock: HttpTestingController;

  const workOrderId = '4f2fb4f2-80e2-4ddb-9e88-b9a456103d12';
  const expected: WorkOrderResponse = {
    id: workOrderId,
    trackingNumber: 'WO-000123',
    description: 'Replace hydraulic seal',
    status: 'Open',
    assignedMechanicId: null,
    assignedMechanicFullName: null,
    createdAtUtc: '2026-07-20T08:30:00Z',
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        WorkOrdersService,
      ],
    });

    service = TestBed.inject(WorkOrdersService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should POST /api/workorders', () => {
    const payload: CreateWorkOrderRequest = {
      description: 'Replace hydraulic seal',
    };

    service.create(payload).subscribe((workOrder) => {
      expect(workOrder).toEqual(expected);
    });

    const req = httpMock.expectOne('/api/workorders');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(expected);
  });

  it('should GET /api/workorders', () => {
    service.getAll().subscribe((workOrders) => {
      expect(workOrders).toEqual([expected]);
    });

    const req = httpMock.expectOne('/api/workorders');
    expect(req.request.method).toBe('GET');
    expect(req.request.body).toBeNull();
    req.flush([expected]);
  });

  it('should GET /api/workorders/:id', () => {
    service.getById(workOrderId).subscribe((workOrder) => {
      expect(workOrder).toEqual(expected);
    });

    const req = httpMock.expectOne(`/api/workorders/${workOrderId}`);
    expect(req.request.method).toBe('GET');
    expect(req.request.body).toBeNull();
    req.flush(expected);
  });

  it('should PUT /api/workorders/:id/assign', () => {
    const payload: AssignMechanicRequest = {
      mechanicId: 'mech-42',
    };

    service.assignMechanic(workOrderId, payload).subscribe((workOrder) => {
      expect(workOrder).toEqual({
        ...expected,
        assignedMechanicId: payload.mechanicId,
        assignedMechanicFullName: null,
      });
    });

    const req = httpMock.expectOne(`/api/workorders/${workOrderId}/assign`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush({
      ...expected,
      assignedMechanicId: payload.mechanicId,
      assignedMechanicFullName: null,
    });
  });

  it('should PUT /api/workorders/:id/complete', () => {
    service.complete(workOrderId).subscribe((workOrder) => {
      expect(workOrder).toEqual({
        ...expected,
        status: 'Completed',
      });
    });

    const req = httpMock.expectOne(`/api/workorders/${workOrderId}/complete`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({});
    req.flush({
      ...expected,
      status: 'Completed',
    });
  });
});