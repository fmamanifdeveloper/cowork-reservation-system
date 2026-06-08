import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthStore } from '@core/auth/auth-store';
import { UserRole } from '@core/auth/auth-models';

export const roleGuard: CanActivateFn = route => {
    const authStore = inject(AuthStore);
    const router = inject(Router);

    const allowedRoles = route.data['roles'] as UserRole[] | undefined;

    if (!allowedRoles || allowedRoles.length === 0) {
        return true;
    }

    if (authStore.hasAnyRole(allowedRoles)) {
        return true;
    }

    return router.createUrlTree(['/auth/login']);
};