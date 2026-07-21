import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';

import { EmployeeDto } from '../../../src/app/core/models/employee.models';
import { EmployeesService } from '../../../src/app/core/services/employees.service';

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
        id: 'emp-1',
        userId: 'user-1',
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
});