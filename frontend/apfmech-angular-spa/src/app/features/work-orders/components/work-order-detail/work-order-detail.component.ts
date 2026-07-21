import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { WorkOrdersService } from '../../services/work-orders.service';
import { WorkOrderResponse } from '../../../../core/models/work-order.models';

@Component({
  selector: 'app-work-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './work-order-detail.component.html'
})
export class WorkOrderDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private workOrderService = inject(WorkOrdersService);

  workOrder = signal<WorkOrderResponse | null>(null);
  loading = signal<boolean>(true);

  ngOnInit(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.loading.set(true);
        this.workOrderService.getById(id).subscribe({
          next: (data) => {
            this.workOrder.set(data);
            this.loading.set(false);
          },
          error: () => {
            this.workOrder.set(null);
            this.loading.set(false);
          }
        });
      }
    });
  }
}