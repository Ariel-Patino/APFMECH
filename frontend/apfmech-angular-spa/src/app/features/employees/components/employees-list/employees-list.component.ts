import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  computed,
  inject,
  signal,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { EmployeeDto } from '../../../../core/models/employee.models';
import { HttpErrorMessageService } from '../../../../core/services/http-error-message.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { EmployeesService } from '../../services/employees.service';

@Component({
  selector: 'app-employees-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './employees-list.component.html',
  host: { class: 'block' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmployeesListComponent implements OnInit {
  private readonly employeesService = inject(EmployeesService);
  private readonly notificationService = inject(NotificationService);
  private readonly httpErrorMessageService = inject(HttpErrorMessageService);

  readonly employees = signal<EmployeeDto[]>([]);
  readonly loading = signal(false);
  readonly operationInProgressByEmployeeId = signal<Record<string, boolean>>({});
  readonly hasEmployees = computed(() => this.employees().length > 0);

  ngOnInit(): void {
    this.loadEmployees();
  }

  employeeFullName(employee: EmployeeDto): string {
    return `${employee.firstName} ${employee.lastName}`;
  }

  employeeStatus(employee: EmployeeDto): 'Active' | 'Inactive' {
    return employee.isActive ? 'Active' : 'Inactive';
  }

  disableEmployee(employeeId: string): void {
    this.setOperationInProgress(employeeId, true);

    this.employeesService
      .disable(employeeId)
      .pipe(finalize(() => this.setOperationInProgress(employeeId, false)))
      .subscribe({
        next: (updatedEmployee) => {
          this.replaceEmployee(updatedEmployee);
          this.notificationService.success(
            'Employee disabled',
            `${this.employeeFullName(updatedEmployee)} is now inactive.`,
          );
        },
        error: (error: unknown) => {
          this.notificationService.error(
            'Disable failed',
            this.extractErrorMessage(error, 'Unable to disable the selected employee.'),
          );
        },
      });
  }

  deleteEmployee(employeeId: string): void {
    const confirmed = window.confirm('Delete this employee permanently (GDPR hard delete)?');
    if (!confirmed) {
      return;
    }

    this.setOperationInProgress(employeeId, true);

    this.employeesService
      .delete(employeeId)
      .pipe(finalize(() => this.setOperationInProgress(employeeId, false)))
      .subscribe({
        next: () => {
          this.removeEmployee(employeeId);
          this.notificationService.success('Employee deleted', 'The employee record was permanently removed.');
        },
        error: (error: unknown) => {
          this.notificationService.error(
            'Delete failed',
            this.extractErrorMessage(error, 'Unable to delete the selected employee.'),
          );
        },
      });
  }

  isOperationInProgress(employeeId: string): boolean {
    return this.operationInProgressByEmployeeId()[employeeId] === true;
  }

  private loadEmployees(): void {
    this.loading.set(true);

    this.employeesService
      .getAll()
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe((employees) => this.employees.set(employees));
  }

  private replaceEmployee(updatedEmployee: EmployeeDto): void {
    this.employees.update((currentEmployees) =>
      currentEmployees.map((employee) =>
        employee.id === updatedEmployee.id ? updatedEmployee : employee,
      ),
    );
  }

  private removeEmployee(employeeId: string): void {
    this.employees.update((currentEmployees) =>
      currentEmployees.filter((employee) => employee.id !== employeeId),
    );
  }

  private setOperationInProgress(employeeId: string, inProgress: boolean): void {
    this.operationInProgressByEmployeeId.update((state) => ({
      ...state,
      [employeeId]: inProgress,
    }));
  }

  private extractErrorMessage(error: unknown, fallbackMessage: string): string {
    return this.httpErrorMessageService.extractMessage(error, fallbackMessage);
  }
}