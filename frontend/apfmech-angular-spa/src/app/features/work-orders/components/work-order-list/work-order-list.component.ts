import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmployeeDto } from '../../../../core/models/employee.models';
import { WorkOrderResponse } from '../../../../core/models/work-order.models';
import { EmployeesService } from '../../../employees/services/employees.service';
import { WorkOrdersService } from '../../services/work-orders.service';
import { WorkOrderCreateComponent } from '../work-order-create/work-order-create.component';
import { HttpErrorMessageService } from '../../../../core/services/http-error-message.service';
import { NotificationService } from '../../../../core/services/notification.service';

//TODO Improve Tab Component and implement pagination on backend
export type WorkOrderTab = 'Pending' | 'InProgress' | 'Completed' | 'All';

@Component({
  selector: 'app-work-order-list',
  standalone: true,
  imports: [CommonModule, RouterLink, WorkOrderCreateComponent],
  templateUrl: './work-order-list.component.html',
  host: { class: 'block min-h-dvh' },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderListComponent implements OnInit {
  private readonly workOrdersService = inject(WorkOrdersService);
  private readonly employeesService = inject(EmployeesService);
  private readonly notificationService = inject(NotificationService);
  private readonly httpErrorMessageService = inject(HttpErrorMessageService);

  readonly workOrders = signal<WorkOrderResponse[]>([]);
  readonly employees = signal<EmployeeDto[]>([]);
  readonly selectedMechanics = signal<Record<string, string>>({});

  readonly activeTab = signal<WorkOrderTab>('Pending');
  readonly filteredWorkOrders = computed(() => {
      const tab = this.activeTab();
      const list = this.workOrders();

      if (tab === 'All') {
        return list;
      }

      return list.filter((order) => order.status === tab);
  });

  ngOnInit(): void {
    this.loadWorkOrders();
    this.loadEmployees();
  }
  setTab(tab: WorkOrderTab): void {
    this.activeTab.set(tab);
  }
  onWorkOrderCreated(workOrder: WorkOrderResponse): void {
    this.workOrders.update((current) => [workOrder, ...current]);
  }

  onMechanicSelectionChange(workOrderId: string, mechanicId: string): void {
    this.selectedMechanics.update((state) => ({
      ...state,
      [workOrderId]: mechanicId,
    }));
  }

  assignMechanic(workOrderId: string): void {
    const mechanicId = this.selectedMechanics()[workOrderId];
    if (!mechanicId) {
      return;
    }

    this.workOrdersService
      .assignMechanic(workOrderId, mechanicId)
      .subscribe({
        next: (workOrder) => {
          this.replaceWorkOrder(workOrder);
          this.notificationService.success(
            'Mechanic assigned',
            this.buildAssignmentSuccessMessage(workOrderId, workOrder.assignedMechanicFullName),
          );
        },
        error: (error: unknown) => {
          this.notificationService.error(
            'Assignment failed',
            this.extractErrorMessage(error, 'Unable to assign the selected mechanic.'),
          );
        },
      });
  }

  completeWorkOrder(workOrderId: string): void {
    const workOrder = this.workOrders().find((item) => item.id === workOrderId);
    if (workOrder?.status === 'Completed') {
      return;
    }

    this.workOrdersService
      .complete(workOrderId)
      .subscribe({
        next: (updatedWorkOrder) => {
          this.replaceWorkOrder(updatedWorkOrder);
          this.notificationService.success(
            'Work order completed',
            `Work order ${updatedWorkOrder.trackingNumber} was completed successfully.`,
          );
        },
        error: (error: unknown) => {
          this.notificationService.error(
            'Completion failed',
            this.extractErrorMessage(error, 'Only work orders in progress can be completed.'),
          );
        },
      });
  }

  employeeFullName(employee: EmployeeDto): string {
    return `${employee.firstName} ${employee.lastName}`;
  }

  assignedMechanicLabel(workOrder: WorkOrderResponse): string {
    return workOrder.assignedMechanicFullName ?? 'None';
  }

  assignmentActionLabel(workOrder: WorkOrderResponse): string {
    return workOrder.assignedMechanicId ? 'Re-Assign mechanic' : 'Assign mechanic';
  }

  isWorkOrderCompleted(workOrder: WorkOrderResponse): boolean {
    return workOrder.status === 'Completed';
  }

  private loadWorkOrders(): void {
    this.workOrdersService
      .getAll()
      .subscribe((workOrders) => this.workOrders.set(workOrders));
  }

  private loadEmployees(): void {
    this.employeesService
      .getAll()
      .subscribe((employees) => this.employees.set(employees.filter((employee) => employee.isActive)));
  }

  private replaceWorkOrder(updatedWorkOrder: WorkOrderResponse): void {
    this.workOrders.update((current) =>
      current.map((workOrder) =>
        workOrder.id === updatedWorkOrder.id ? updatedWorkOrder : workOrder,
      ),
    );
  }

  private buildAssignmentSuccessMessage(workOrderId: string, assignedMechanicFullName: string | null): string {
    const workOrder = this.workOrders().find((item) => item.id === workOrderId);
    const mechanicName = assignedMechanicFullName ?? 'Selected mechanic';

    return workOrder
      ? `${mechanicName} was assigned to work order ${workOrder.trackingNumber}.`
      : `${mechanicName} was assigned successfully.`;
  }

  private extractErrorMessage(error: unknown, fallbackMessage: string): string {
    return this.httpErrorMessageService.extractMessage(error, fallbackMessage);
  }
}