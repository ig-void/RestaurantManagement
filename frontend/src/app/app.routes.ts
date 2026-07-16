import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'customer/browse',
    loadComponent: () => import('./features/customer/browse-restaurants/browse-restaurants.component').then(m => m.BrowseRestaurantsComponent),
    canActivate: [authGuard(['Customer'])]
  },
  {
    path: 'customer/reservations',
    loadComponent: () => import('./features/customer/my-reservations/my-reservations.component').then(m => m.MyReservationsComponent),
    canActivate: [authGuard(['Customer'])]
  },
  {
    path: 'manager/restaurants',
    loadComponent: () => import('./features/manager/restaurants-admin/restaurants-admin.component').then(m => m.RestaurantsAdminComponent),
    canActivate: [authGuard(['RestaurantManager'])]
  },
  {
    path: 'manager/tables',
    loadComponent: () => import('./features/manager/tables-admin/tables-admin.component').then(m => m.TablesAdminComponent),
    canActivate: [authGuard(['RestaurantManager'])]
  },
  {
    path: 'manager/reservations',
    loadComponent: () => import('./features/manager/reservations-admin/reservations-admin.component').then(m => m.ReservationsAdminComponent),
    canActivate: [authGuard(['RestaurantManager'])]
  },
  {
    path: 'forbidden',
    loadComponent: () => import('./features/forbidden/forbidden.component').then(m => m.ForbiddenComponent)
  },
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'login'
  },
  {
    path: '**',
    redirectTo: 'login'
  }
];

