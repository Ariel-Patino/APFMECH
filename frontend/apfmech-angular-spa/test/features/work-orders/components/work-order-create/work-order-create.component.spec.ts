import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { WorkOrdersService } from '../../../../../src/app/features/work-orders/services/work-orders.service';
import { WorkOrderCreateComponent } from '../../../../../src/app/features/work-orders/components/work-order-create/work-order-create.component';

describe('WorkOrderCreateComponent', () => {
  let fixture: ComponentFixture<WorkOrderCreateComponent>;
  let component: WorkOrderCreateComponent;
  let workOrdersService: {
    create: jest.Mock;
  };

  beforeEach(async () => {
    workOrdersService = {
      create: jest.fn().mockReturnValue(
        of({
          id: '4f2fb4f2-80e2-4ddb-9e88-b9a456103d12',
          trackingNumber: 'WO-20260720-ABC123',
          description: 'Replace hydraulic seal',
          status: 'Pending',
          assignedMechanicId: null,
          assignedMechanicFullName: null,
          createdAtUtc: '2026-07-20T08:30:00Z',
        }),
      ),
    };

    await TestBed.configureTestingModule({
      imports: [WorkOrderCreateComponent],
      providers: [
        {
          provide: WorkOrdersService,
          useValue: workOrdersService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WorkOrderCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should require description field', () => {
    component.form.controls.description.setValue('');

    const submitButton = fixture.nativeElement.querySelector('[data-testid="create-submit"]') as HTMLButtonElement;
    submitButton.click();

    expect(component.form.controls.description.hasError('required')).toBe(true);
    expect(workOrdersService.create).not.toHaveBeenCalled();
  });

  it('should call WorkOrdersService.create on submit with valid form value', () => {
    component.form.controls.description.setValue('Replace hydraulic seal');
    fixture.detectChanges();

    const submitButton = fixture.nativeElement.querySelector('[data-testid="create-submit"]') as HTMLButtonElement;
    submitButton.click();

    expect(workOrdersService.create).toHaveBeenCalledWith({
      description: 'Replace hydraulic seal',
    });
  });
});