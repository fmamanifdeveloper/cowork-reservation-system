import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'info' | 'warning';

export interface AppNotification {
    type: NotificationType;
    message: string;
}

@Injectable({
    providedIn: 'root'
})
export class NotificationStore {
    private readonly notificationState = signal<AppNotification | null>(null);

    readonly notification = this.notificationState.asReadonly();

    show(type: NotificationType, message: string): void {
        this.notificationState.set({ type, message });

        window.setTimeout(() => {
            this.clear();
        }, 3500);
    }

    success(message: string): void {
        this.show('success', message);
    }

    error(message: string): void {
        this.show('error', message);
    }

    info(message: string): void {
        this.show('info', message);
    }

    warning(message: string): void {
        this.show('warning', message);
    }

    clear(): void {
        this.notificationState.set(null);
    }
}