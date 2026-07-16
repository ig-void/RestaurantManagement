import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-forbidden',
  standalone: true,
  templateUrl: './forbidden.component.html'
})
export class ForbiddenComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  goToDashboard(): void {
    const user = this.authService.currentUser();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    if (user.role === 'RestaurantManager') {
      this.router.navigate(['/manager/restaurants']);
    } else {
      this.router.navigate(['/customer/browse']);
    }
  }
}
