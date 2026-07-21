import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { EmployeeDto } from '../models/employee.models';

@Injectable({
  providedIn: 'root',
})
export class EmployeesService {
  private readonly apiUrl = '/api/employees';

  constructor(private readonly httpClient: HttpClient) {}

  getAll(): Observable<EmployeeDto[]> {
    return this.httpClient.get<EmployeeDto[]>(this.apiUrl);
  }
}