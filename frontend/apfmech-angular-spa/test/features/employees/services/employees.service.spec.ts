import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { EmployeeDto } from '../../../../src/app/core/models/employee.models';
import { EmployeesService } from '../../../../src/app/features/employees/services/employees.service';

describe('EmployeesService', () => {
  let service: EmployeesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        EmployeesService,
      ],
    });

    service = TestBed.inject(EmployeesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should GET /api/employees', () => {
    const expected: EmployeeDto[] = [
      {
        id: '1a2b3c4d-1111-2222-3333-444455556666',
        userId: '6f5e4d3c-9999-8888-7777-666655554444',
        firstName: 'Alex',
        lastName: 'Turner',
        isActive: true,
        roles: ['Mechanic'],
      },
    ];

    service.getAll().subscribe((employees) => {
      expect(employees).toEqual(expected);
    });

    const req = httpMock.expectOne('/api/employees');
    expect(req.request.method).toBe('GET');
    expect(req.request.body).toBeNull();
    req.flush(expected);
  });

  it('should PATCH /api/employees/{id}/disable', () => {
    const employeeId = '1a2b3c4d-1111-2222-3333-444455556666';
    const expected: EmployeeDto = {
      id: employeeId,
      userId: '6f5e4d3c-9999-8888-7777-666655554444',
      firstName: 'Alex',
      lastName: 'Turner',
      isActive: false,
      roles: ['Mechanic'],
    };

    service.disable(employeeId).subscribe((employee) => {
      expect(employee).toEqual(expected);
    });

    const req = httpMock.expectOne(`/api/employees/${employeeId}/disable`);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({});
    req.flush(expected);
  });

  it('should DELETE /api/employees/{id}', () => {
    const employeeId = '1a2b3c4d-1111-2222-3333-444455556666';

    service.delete(employeeId).subscribe((response) => {
      expect(response).toBeUndefined();
    });

    const req = httpMock.expectOne(`/api/employees/${employeeId}`);
    expect(req.request.method).toBe('DELETE');
    expect(req.request.body).toBeNull();
    req.flush(null);
  });
});