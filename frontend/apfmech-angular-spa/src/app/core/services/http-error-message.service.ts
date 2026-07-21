import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class HttpErrorMessageService {
  extractMessage(error: unknown, fallbackMessage: string): string {
    if (!(error instanceof HttpErrorResponse)) {
      return fallbackMessage;
    }

    const problemDetails = error.error as {
      detail?: string;
      title?: string;
      errors?: Record<string, string[]>;
    } | null;

    if (problemDetails?.detail) {
      return problemDetails.detail;
    }

    const validationErrors = problemDetails?.errors;
    if (validationErrors) {
      const firstMessage = Object.values(validationErrors).flat().find(Boolean);
      if (firstMessage) {
        return firstMessage;
      }
    }

    if (error.message) {
      return error.message;
    }

    return fallbackMessage;
  }
}