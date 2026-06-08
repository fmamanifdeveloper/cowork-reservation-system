import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthApi } from '@core/auth/auth-api';
import { AuthStore } from '@core/auth/auth-store';
import { NotificationStore } from '@core/notifications/notification-store';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-login-page',
  imports: [FormsModule],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
})
export class LoginPage {
  private readonly authApi = inject(AuthApi);
  private readonly authStore = inject(AuthStore);
  private readonly notificationStore = inject(NotificationStore);
  private readonly router = inject(Router);

  readonly isLoading = signal(false);

  username = 'admin';
  password = 'Admin123!';

  login(): void {
    if (!this.username.trim() || !this.password.trim()) {
      this.notificationStore.warning('Ingresa usuario y contraseña.');
      return;
    }

    this.isLoading.set(true);

    this.authApi
      .login({
        username: this.username.trim(),
        password: this.password
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: response => {
          this.authStore.setSession(response);
          this.notificationStore.success('Sesión iniciada correctamente.');

          if (response.user.role === 'Admin' || response.user.role === 'Staff') {
            this.router.navigateByUrl('/admin/dashboard');
            return;
          }

          this.router.navigateByUrl('/admin/reservations');
        },
        error: () => {
          this.notificationStore.error('Usuario o contraseña incorrectos.');
        }
      });
  }
}
