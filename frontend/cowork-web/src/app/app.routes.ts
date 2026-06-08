import { Routes } from '@angular/router';
import { authGuard } from '@core/guards/auth-guard';
import { roleGuard } from '@core/guards/role-guard';
import { LoginPage } from '@features/auth/login-page/login-page';
import { DashboardPage } from './features/dashboard/dashboard-page/dashboard-page';
import { AdminLayout } from './layouts/admin-layout/admin-layout';
import { PublicLayout } from './layouts/public-layout/public-layout';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'public',
        pathMatch: 'full'
    },
    {
        path: 'auth/login',
        component: LoginPage
    },
    {
        path: 'forbidden',
        loadComponent: () =>
            import('./features/forbidden/forbidden-page/forbidden-page').then(m => m.ForbiddenPage)
    },
    {
        path: 'public',
        component: PublicLayout,
        children: [
            {
                path: '',
                redirectTo: 'spaces',
                pathMatch: 'full'
            },
            {
                path: 'spaces',
                loadComponent: () =>
                    import('./features/public/public-spaces-page/public-spaces-page').then(m => m.PublicSpacesPage)
            },
            {
                path: 'reservation',
                loadComponent: () =>
                    import('./features/public/public-reservation-page/public-reservation-page').then(m => m.PublicReservationPage)
            }
        ]
    },
    {
        path: 'admin',
        component: AdminLayout,
        canActivate: [authGuard],
        children: [
            {
                path: '',
                redirectTo: 'dashboard',
                pathMatch: 'full'
            },
            {
                path: 'dashboard',
                component: DashboardPage,
                canActivate: [roleGuard],
                data: {
                    roles: ['Admin', 'Staff']
                }
            },
            {
                path: 'spaces',
                loadComponent: () =>
                    import('./features/spaces/admin-spaces-page/admin-spaces-page').then(m => m.AdminSpacesPage),
                canActivate: [roleGuard],
                data: {
                    roles: ['Admin', 'Staff']
                }
            },
            {
                path: 'customers',
                loadComponent: () =>
                    import('./features/customers/admin-customers-page/admin-customers-page').then(m => m.AdminCustomersPage),
                canActivate: [roleGuard],
                data: {
                    roles: ['Admin', 'Staff']
                }
            },
            {
                path: 'reservations',
                loadComponent: () =>
                    import('./features/reservations/admin-reservations-page/admin-reservations-page')
                        .then(m => m.AdminReservationsPage),
                canActivate: [roleGuard],
                data: {
                    roles: ['Admin', 'Staff', 'Customer']
                }
            },
            {
                path: 'reports',
                loadComponent: () =>
                    import('./features/reports/reports-page/reports-page').then(m => m.ReportsPage),
                canActivate: [roleGuard],
                data: {
                    roles: ['Admin', 'Staff']
                }
            },
            {
                path: 'audit-logs',
                loadComponent: () =>
                    import('./features/audit-logs/audit-logs-page/audit-logs-page').then(m => m.AuditLogsPage),
                canActivate: [roleGuard],
                data: {
                    roles: ['Admin', 'Staff']
                }
            }
        ]
    },
    {
        path: '**',
        redirectTo: 'public'
    }
];
