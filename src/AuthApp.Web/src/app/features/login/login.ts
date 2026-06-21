import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly loading = this.authService.loading;
  readonly serverError = signal<string | null>(null);
  readonly sessionExpired = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  ngOnInit(): void {
    this.authService.getAntiforgeryToken().subscribe();

    const params = this.route.snapshot.queryParamMap;
    if (params.get('sessionExpired') === 'true') {
      this.sessionExpired.set(true);
    }
    if (params.get('returnUrl')) {
      // Si hay returnUrl, el usuario fue redirigido desde una ruta protegida
      this.serverError.set('Debes iniciar sesión para continuar.');
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.serverError.set(null);
    const { email, password } = this.form.getRawValue();

    this.authService.login({ email, password }).subscribe({
      next: (user) => {
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigate([returnUrl || '/dashboard']);
      },
      error: (err) => {
        const message =
          err?.error?.message ?? 'No se pudo iniciar sesión. Intenta de nuevo.';
        this.serverError.set(message);
      },
    });
  }

  getFieldError(fieldName: string): string | null {
    const field = this.form.get(fieldName);
    if (!field || (!field.touched && !field.dirty)) return null;

    if (field.errors?.['required']) {
      return fieldName === 'email' ? 'El correo es obligatorio.' : 'La contraseña es obligatoria.';
    }
    if (field.errors?.['email']) return 'El correo no tiene un formato válido.';
    if (field.errors?.['minlength']) return 'La contraseña debe tener al menos 6 caracteres.';
    return null;
  }
}
