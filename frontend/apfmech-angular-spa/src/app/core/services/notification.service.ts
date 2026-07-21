import { Injectable, signal } from '@angular/core';

export type ToastType = 'success' | 'error' | 'info';

export interface ToastMessage {
  id: string;
  type: ToastType;
  title: string;
  description: string;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly toastMessagesSignal = signal<ToastMessage[]>([]);

  readonly toastMessages = this.toastMessagesSignal.asReadonly();

  success(title: string, description: string): void {
    this.addToast('success', title, description);
  }

  error(title: string, description: string): void {
    this.addToast('error', title, description);
  }

  info(title: string, description: string): void {
    this.addToast('info', title, description);
  }

  dismiss(id: string): void {
    this.toastMessagesSignal.update((messages) => messages.filter((message) => message.id !== id));
  }

  private addToast(type: ToastType, title: string, description: string): void {
    const id = crypto.randomUUID();

    this.toastMessagesSignal.update((messages) => [
      ...messages,
      {
        id,
        type,
        title,
        description,
      },
    ]);

    window.setTimeout(() => this.dismiss(id), 5000);
  }
}