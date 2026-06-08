import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from './auth-store';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
    const authStore = inject(AuthStore);
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
                router.navigateByUrl('/auth/login');
            }

            return throwError(() => error);
        })
    );
};