import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { EmployeesService } from '../../../../../src/app/features/employees/services/employees.service';
import { EmployeesListComponent } from '../../../../../src/app/features/employees/components/employees-list/employees-list.component';

describe('EmployeesListComponent', () => {
  let fixture: ComponentFixture<EmployeesListComponent>;
  let component: EmployeesListComponent;
  let employeesService: {
    getAll: jest.Mock;
    disable: jest.Mock;
    delete: jest.Mock;
  };

  const employeeId = '1a2b3c4d-1111-2222-3333-444455556666';
  const userId = '6f5e4d3c-9999-8888-7777-666655554444';

  beforeEach(async () => {
    Object.defineProperty(window, 'confirm', {
      configurable: true,
      value: jest.fn(() => true),
    });

    employeesService = {
      getAll: jest.fn().mockReturnValue(
        of([
          {
            id: employeeId,
            userId,
            firstName: 'Alex',
            lastName: 'Turner',
            isActive: true,
            roles: ['Mechanic', 'Inspector'],
          },
        ]),
      ),
      disable: jest.fn().mockReturnValue(
        of({
          id: employeeId,
          userId,
          firstName: 'Alex',
          lastName: 'Turner',
          isActive: false,
          roles: ['Mechanic', 'Inspector'],
        }),
      ),
      delete: jest.fn().mockReturnValue(of(void 0)),
    };

    await TestBed.configureTestingModule({
      imports: [EmployeesListComponent],
      providers: [
        {
          provide: EmployeesService,
          useValue: employeesService,
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeesListComponent);
    component = fixture.componentInstance;
  });

  it('should load employees on component mount and render the list', () => {
    fixture.detectChanges();

    expect(employeesService.getAll).toHaveBeenCalled();

    const row = fixture.nativeElement.querySelector('[data-testid="employee-row"]');
    expect(row).toBeTruthy();
    expect(fixture.nativeElement.textContent).toContain('Alex Turner');
    expect(fixture.nativeElement.textContent).toContain('Mechanic, Inspector');
    expect(fixture.nativeElement.textContent).toContain('Active');
  });

  it('should disable employee when disable button is clicked', () => {
    fixture.detectChanges();

    const disableButton = fixture.nativeElement.querySelector('[data-testid="disable-button"]') as HTMLButtonElement;
    disableButton.click();

    expect(employeesService.disable).toHaveBeenCalledWith(employeeId);
  });

  it('should delete employee when delete button is clicked and confirmed', () => {
    fixture.detectChanges();

    const deleteButton = fixture.nativeElement.querySelector('[data-testid="delete-button"]') as HTMLButtonElement;
    deleteButton.click();

    expect(window.confirm).toHaveBeenCalled();
    expect(employeesService.delete).toHaveBeenCalledWith(employeeId);
  });
});