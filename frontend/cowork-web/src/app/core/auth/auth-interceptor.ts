import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from './auth-store';
import { NotificationStore } from '@core/notifications/notification-store';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
    const authStore = inject(AuthStore);
    const notificationStore = inject(NotificationStore);
    const router = inject(Router);

    const token = authStore.token();

    const authenticatedRequest = token
        ? request.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
        })
        : request;

    return next(authenticatedRequest).pipe(
        catchError(error => {
            if (error.status === 401) {
                authStore.logout();
                notificationStore.warning('Tu sesión expiró. Inicia sesión nuevamente.');
                router.navigateByUrl('/auth/login');
            }

            if (error.status === 403) {
                notificationStore.warning('No tienes permisos para realizar esta acción.');
                router.navigateByUrl('/forbidden');
            }

            return throwError(() => error);
        })
    );
};