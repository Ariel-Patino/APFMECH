import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import {
  CreateWorkOrderRequest,
  WorkOrderResponse,
} from '../../../../src/app/core/models/work-order.models';
import { WorkOrdersService } from '../../../../src/app/features/work-orders/services/work-orders.service';

describe('WorkOrdersService', () => {
  let service: WorkOrdersService;
  let httpMock: HttpTestingController;

  const workOrderId = '4f2fb4f2-80e2-4ddb-9e88-b9a456103d12';
  const mechanicId = '9f1ce29d-98d4-4fd7-a7e4-0f8cf86bbf74';
  const expected: WorkOrderResponse = {
    id: workOrderId,
    trackingNumber: 'WO-20260720-ABC123',
    description: 'Replace hydraulic seal',
    status: 'Pending',
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

  it('should GET /api/workorders', () => {
    service.getAll().subscribe((workOrders) => {
      expect(workOrders).toEqual([expected]);
    });

    const req = httpMock.expectOne('/api/workorders');
    expect(req.request.method).toBe('GET');
    expect(req.request.body).toBeNull();
    req.flush([expected]);
  });

  it('should GET /api/workorders/{id}', () => {
    service.getById(workOrderId).subscribe((workOrder) => {
      expect(workOrder).toEqual(expected);
    });

    const req = httpMock.expectOne(`/api/workorders/${workOrderId}`);
    expect(req.request.method).toBe('GET');
    expect(req.request.body).toBeNull();
    req.flush(expected);
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

  it('should PUT /api/workorders/{id}/assign', () => {
    service.assignMechanic(workOrderId, mechanicId).subscribe((workOrder) => {
      expect(workOrder).toEqual({
        ...expected,
        assignedMechanicId: mechanicId,
        assignedMechanicFullName: 'Alex Turner',
        status: 'InProgress',
      });
    });

    const req = httpMock.expectOne(`/api/workorders/${workOrderId}/assign`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ mechanicId });
    req.flush({
      ...expected,
      assignedMechanicId: mechanicId,
      assignedMechanicFullName: 'Alex Turner',
      status: 'InProgress',
    });
  });

  it('should PUT /api/workorders/{id}/complete', () => {
    service.complete(workOrderId).subscribe((workOrder) => {
      expect(workOrder).toEqual({
        ...expected,
        assignedMechanicId: mechanicId,
        assignedMechanicFullName: 'Alex Turner',
        status: 'Completed',
      });
    });

    const req = httpMock.expectOne(`/api/workorders/${workOrderId}/complete`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({});
    req.flush({
      ...expected,
      assignedMechanicId: mechanicId,
      assignedMechanicFullName: 'Alex Turner',
      status: 'Completed',
    });
  });
});