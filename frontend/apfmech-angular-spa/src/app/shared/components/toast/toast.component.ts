import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { NotificationService, ToastMessage } from '../../../core/services/notification.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastComponent {
  private readonly notificationService = inject(NotificationService);

  readonly messages = this.notificationService.toastMessages;

  dismiss(messageId: string): void {
    this.notificationService.dismiss(messageId);
  }

  messageClass(message: ToastMessage): string {
    return message.type;
  }
}