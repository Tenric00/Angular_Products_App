/*
  ToastService is a service that provides a way to show toast messages to the user.
*/

import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export type ToastLevel = 'success' | 'error' | 'info';

export interface ToastMessage {
  text: string;
  level: ToastLevel;
  ttlMs?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private messagesSubject = new Subject<ToastMessage>();
  messages$: Observable<ToastMessage> = this.messagesSubject.asObservable();

  show(text: string, level: ToastLevel = 'info', ttlMs = 3000) {
    this.messagesSubject.next({ text, level, ttlMs });
  }

  success(text: string, ttlMs = 3000) {
    this.show(text, 'success', ttlMs);
  }

  error(text: string, ttlMs = 5000) {
    this.show(text, 'error', ttlMs);
  }
}
