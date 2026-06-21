import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, retry, throwError, timer } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { SKIP_ERROR_INTERCEPTOR } from '../tokens/http-context.tokens';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    retry({
      count: 1,
      delay: (error: HttpErrorResponse) => {
        if (isTransient(error)) {
          return timer(1000);
        }
        return throwError(() => error);
      },
    }),
    catchError((error: HttpErrorResponse) => {
      const skipHandling = req.context.get(SKIP_ERROR_INTERCEPTOR);

      if (error.status === 401 && !skipHandling) {
        const isLoginRequest = req.url.includes('/auth/login');
        if (!isLoginRequest) {
          authService.navigateToLogin();
        }
      }

      if (error.status === 403 && !skipHandling) {
        router.navigate(['/dashboard'], { queryParams: { forbidden: 'true' } });
      }

      return throwError(() => error);
    }),
  );
};

function isTransient(error: HttpErrorResponse): boolean {
  return error.status === 0 || error.status >= 500;
}
