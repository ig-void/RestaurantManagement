import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard = (expectedRoles?: string[]): CanActivateFn => {
  return (route, state) => {
    const router = inject(Router);
    const token = localStorage.getItem('token');
    const role = localStorage.getItem('role');

    if (!token) {
      router.navigate(['/login']);
      return false;
    }

    if (expectedRoles && expectedRoles.length > 0) {
      if (!role || !expectedRoles.includes(role)) {
        router.navigate(['/forbidden']);
        return false;
      }
    }

    return true;
  };
};

