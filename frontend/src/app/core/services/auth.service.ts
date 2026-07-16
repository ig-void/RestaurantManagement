import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';

export interface UserSession {
  token: string;
  fullName: string;
  email: string;
  role: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = 'https://localhost:7299/api/auth';

  // Current user state using Signals
  readonly currentUser = signal<UserSession | null>(this.loadUserSession());
  readonly isAuthenticated = computed(() => !!this.currentUser());
  readonly isManager = computed(() => this.currentUser()?.role === 'RestaurantManager');

  private loadUserSession(): UserSession | null {
    const token = localStorage.getItem('token');
    const fullName = localStorage.getItem('fullName');
    const email = localStorage.getItem('email');
    const role = localStorage.getItem('role');

    if (token && fullName && email && role) {
      return { token, fullName, email, role };
    }
    return null;
  }

  login(credentials: { email: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        if (res.success && res.token) {
          localStorage.setItem('token', res.token);
          localStorage.setItem('fullName', res.fullName);
          localStorage.setItem('email', res.email);
          localStorage.setItem('role', res.role);

          this.currentUser.set({
            token: res.token,
            fullName: res.fullName,
            email: res.email,
            role: res.role
          });
        }
      })
    );
  }

  register(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, data);
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('fullName');
    localStorage.removeItem('email');
    localStorage.removeItem('role');

    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }
}
