import { Component, inject } from '@angular/core';
import { NotificationStore } from '@core/notifications/notification-store';

@Component({
  selector: 'app-notification-banner',
  standalone: true,
  imports: [],
  templateUrl: './notification-banner.html',
  styleUrl: './notification-banner.scss',
})
export class NotificationBanner {
  readonly notificationStore: NotificationStore = inject(NotificationStore);
}
