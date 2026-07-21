import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EmployeeDto } from '../../../core/models/employee.models';

@Injectable({
  providedIn: 'root',
})
export class EmployeesService {
  private readonly employeesApiUrl = '/api/employees';

  constructor(private readonly httpClient: HttpClient) {}

  getAll(): Observable<EmployeeDto[]> {
    return this.httpClient.get<EmployeeDto[]>(this.employeesApiUrl);
  }

  disable(employeeId: string): Observable<EmployeeDto> {
    return this.httpClient.patch<EmployeeDto>(
      `${this.employeesApiUrl}/${employeeId}/disable`,
      {},
    );
  }

  delete(employeeId: string): Observable<void> {
    return this.httpClient.delete<void>(`${this.employeesApiUrl}/${employeeId}`);
  }
}