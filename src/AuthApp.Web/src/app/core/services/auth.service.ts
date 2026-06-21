import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, User } from '../models/auth.model';
import { SKIP_ERROR_INTERCEPTOR } from '../tokens/http-context.tokens';
import { HttpContext } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = environment.apiUrl;

  private readonly _user = signal<User | null>(null);
  private readonly _loading = signal(false);
  private readonly _error = signal<string | null>(null);

  readonly user = this._user.asReadonly();
  readonly loading = this._loading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.role === 'admin');

  getAntiforgeryToken(): Observable<{ token: string }> {
    return this.http.get<{ token: string }>(`${this.apiUrl}/antiforgery/token`, {
      context: new HttpContext().set(SKIP_ERROR_INTERCEPTOR, true),
    });
  }

  login(request: LoginRequest): Observable<User> {
    this._loading.set(true);
    this._error.set(null);

    return this.http.post<User>(`${this.apiUrl}/auth/login`, request).pipe(
      tap({
        next: (user) => {
          this._user.set(user);
          this._loading.set(false);
        },
        error: () => {
          this._loading.set(false);
        },
      }),
    );
  }

  logout(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/auth/logout`, {}).pipe(
      tap(() => this.clearUser()),
    );
  }

  fetchMe(): Observable<User> {
    return this.http
      .get<User>(`${this.apiUrl}/auth/me`, {
        context: new HttpContext().set(SKIP_ERROR_INTERCEPTOR, true),
      })
      .pipe(tap((user) => this._user.set(user)));
  }

  fetchAdminData(): Observable<{ message: string }> {
    return this.http.get<{ message: string }>(`${this.apiUrl}/auth/admin`);
  }

  clearUser(): void {
    this._user.set(null);
    this._error.set(null);
  }

  navigateToLogin(returnUrl?: string): void {
    this.clearUser();
    this.router.navigate(['/login'], {
      queryParams: returnUrl ? { returnUrl } : undefined,
    });
  }
}
