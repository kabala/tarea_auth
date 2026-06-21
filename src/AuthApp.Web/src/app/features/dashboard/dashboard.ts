import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  imports: [],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly user = this.authService.user;
  readonly isAdmin = this.authService.isAdmin;
  readonly adminMessage = signal<string | null>(null);
  readonly adminError = signal<string | null>(null);
  readonly forbidden = signal(false);
  readonly loggingOut = signal(false);

  ngOnInit(): void {
    const params = this.router.routerState.snapshot.root.queryParamMap;
    if (params.get('forbidden') === 'true') {
      this.forbidden.set(true);
    }

    if (!this.authService.isAuthenticated()) {
      this.authService.fetchMe().subscribe({
        error: () => this.authService.navigateToLogin(),
      });
    }
  }

  loadAdminData(): void {
    this.adminError.set(null);
    this.authService.fetchAdminData().subscribe({
      next: (res) => this.adminMessage.set(res.message),
      error: (err) => {
        const message = err?.error?.message ?? 'No tienes permisos para esta acción.';
        this.adminError.set(message);
      },
    });
  }

  logout(): void {
    this.loggingOut.set(true);
    this.authService.logout().subscribe({
      next: () => {
        this.loggingOut.set(false);
        this.router.navigate(['/login']);
      },
      error: () => {
        this.loggingOut.set(false);
        this.router.navigate(['/login']);
      },
    });
  }
}
