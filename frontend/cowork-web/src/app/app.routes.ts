import { Routes } from '@angular/router';
import { ReportsDashboard } from '@features/reports/reports-dashboard/reports-dashboard';
import { ReservationCreate } from '@features/reservations/reservation-create/reservation-create';
import { SpacesList } from '@features/spaces/spaces-list/spaces-list';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'spaces',
        pathMatch: 'full'
    },
    {
        path: 'spaces',
        component: SpacesList
    },
    {
        path: 'reservations/create',
        component: ReservationCreate
    },
    {
        path: 'reports',
        component: ReportsDashboard
    },
    {
        path: '**',
        redirectTo: 'spaces'
    }
];
