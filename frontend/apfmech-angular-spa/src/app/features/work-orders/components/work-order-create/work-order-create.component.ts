import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Output,
  inject,
  signal,
} from '@angular/core';
import {
  NonNullableFormBuilder,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { finalize } from 'rxjs';
import { WorkOrderResponse } from '../../../../core/models/work-order.models';
import { WorkOrdersService } from '../../services/work-orders.service';

@Component({
  selector: 'app-work-order-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './work-order-create.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderCreateComponent {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly workOrdersService = inject(WorkOrdersService);

  @Output() readonly created = new EventEmitter<WorkOrderResponse>();

  readonly submitting = signal(false);
  readonly form = this.formBuilder.group({
    description: ['', Validators.required],
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    this.workOrdersService
      .create(this.form.getRawValue())
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe((workOrder) => {
        this.created.emit(workOrder);
        this.form.reset({ description: '' });
      });
  }

  showDescriptionRequired(): boolean {
    const descriptionControl = this.form.controls.description;
    return descriptionControl.touched && descriptionControl.hasError('required');
  }
}