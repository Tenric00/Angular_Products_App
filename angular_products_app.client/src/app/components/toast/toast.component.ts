/*
  toast.component.ts is for displaying toast messages to the user.
  It subscribes to the ToastService to receive messages and displays them in the UI.
  Each message is displayed for a specified time-to-live (TTL) before being removed from
  the list of messages.
*/
import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from '../../services/toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.css']
})
export class ToastComponent implements OnInit, OnDestroy {
  messages: ToastMessage[] = [];
  sub!: Subscription;

  constructor(private svc: ToastService) {}

  ngOnInit(): void {
    this.sub = this.svc.messages$.subscribe(m => {
      this.messages = [...this.messages, m];
      const ttl = m.ttlMs ?? 3000;
      setTimeout(() => {
        this.messages = this.messages.filter(x => x !== m);
      }, ttl);
    });
  }

  ngOnDestroy(): void {
    this.sub.unsubscribe();
  }
}
