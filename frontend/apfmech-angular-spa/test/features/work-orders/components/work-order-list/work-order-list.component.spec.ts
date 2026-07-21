import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { EmployeesService } from '../../../../../src/app/features/employees/services/employees.service';
import { WorkOrdersService } from '../../../../../src/app/features/work-orders/services/work-orders.service';
import { WorkOrderListComponent } from '../../../../../src/app/features/work-orders/components/work-order-list/work-order-list.component';

describe('WorkOrderListComponent', () => {
  let fixture: ComponentFixture<WorkOrderListComponent>;
  let component: WorkOrderListComponent;
  let workOrdersService: {
    getAll: jest.Mock;
    complete: jest.Mock;
    assignMechanic: jest.Mock;
  };
  let employeesService: {
    getAll: jest.Mock;
  };

  const workOrder = {
    id: '4f2fb4f2-80e2-4ddb-9e88-b9a456103d12',
    trackingNumber: 'WO-20260720-ABC123',
    description: 'Replace hydraulic seal',
    status: 'Pending',
    assignedMechanicId: null,
    assignedMechanicFullName: null,
    createdAtUtc: '2026-07-20T08:30:00Z',
  };

  beforeEach(async () => {
    workOrdersService = {
      getAll: jest.fn().mockReturnValue(of([workOrder])),
      complete: jest.fn().mockReturnValue(
        of({
          ...workOrder,
          status: 'Completed',
        }),
      ),
      assignMechanic: jest.fn().mockReturnValue(
        of({
          ...workOrder,
          assignedMechanicId: '9f1ce29d-98d4-4fd7-a7e4-0f8cf86bbf74',
          assignedMechanicFullName: 'Alex Turner',
          status: 'InProgress',
        }),
      ),
    };

    employeesService = {
      getAll: jest.fn().mockReturnValue(
        of([
          {
            id: '9f1ce29d-98d4-4fd7-a7e4-0f8cf86bbf74',
            userId: '6f5e4d3c-9999-8888-7777-666655554444',
            firstName: 'Alex',
            lastName: 'Turner',
            isActive: true,
            roles: ['Mechanic'],
          },
        ]),
      ),
    };

    await TestBed.configureTestingModule({
      imports: [WorkOrderListComponent],
      providers: [
        provideRouter([]),
        {
          provide: WorkOrdersService,
          useValue: workOrdersService,
        },
        {
          provide: EmployeesService,
          useValue: employeesService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WorkOrderListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should load employees and assign mechanic on user interaction', () => {
    const select = fixture.nativeElement.querySelector('[data-testid="mechanic-select"]') as HTMLSelectElement;
    select.value = select.options[1].value;
    select.dispatchEvent(new Event('change', { bubbles: true }));
    fixture.detectChanges();

    const assignButton = fixture.nativeElement.querySelector('[data-testid="assign-button"]') as HTMLButtonElement;
    assignButton.click();

    expect(employeesService.getAll).toHaveBeenCalled();
    expect(workOrdersService.assignMechanic).toHaveBeenCalledWith(
      '4f2fb4f2-80e2-4ddb-9e88-b9a456103d12',
      '9f1ce29d-98d4-4fd7-a7e4-0f8cf86bbf74',
    );
  });

  it('should call complete when user clicks complete button', () => {
    const completeButton = fixture.nativeElement.querySelector('[data-testid="complete-button"]') as HTMLButtonElement;
    completeButton.click();

    expect(workOrdersService.complete).toHaveBeenCalledWith(
      '4f2fb4f2-80e2-4ddb-9e88-b9a456103d12',
    );
  });
});