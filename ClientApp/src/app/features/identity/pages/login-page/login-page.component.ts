import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { CheckboxModule } from 'primeng/checkbox';
import { MessageModule } from 'primeng/message';

import { AuthApiService } from '../../../../core/services/auth-api.service';
import { AccountSessionService } from '../../../../core/services/account-session.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, CheckboxModule, MessageModule],
  templateUrl: './login-page.component.html',
  styleUrl: './login-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly accountSession = inject(AccountSessionService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly redirectTo = this.route.snapshot.queryParamMap.get('redirectTo');
  private readonly checkoutIntent = this.route.snapshot.queryParamMap.get('intent') === 'checkout';

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(
    this.route.snapshot.queryParamMap.get('registered')
      ? 'Account created successfully. Sign in to continue your shopping journey.'
      : this.checkoutIntent
        ? 'Sign in to continue toward secure checkout. Your guest cart stays on this device until you return.'
      : null
  );
  protected readonly signupQueryParams: Record<string, string> = {
    ...(this.checkoutIntent ? { intent: 'checkout' } : {}),
    ...(this.getSafeRedirectTo() ? { redirectTo: this.getSafeRedirectTo()! } : {}),
  };

  protected readonly trustSignals = [
    'Saved carts and account-aware checkout',
    'Secure cookie session with the ASP.NET backend',
    'Clear validation and fast recovery from errors',
  ];

  protected readonly loginForm = this.fb.nonNullable.group({
    email: [
      this.route.snapshot.queryParamMap.get('email') ?? '',
      [Validators.required, Validators.email],
    ],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rememberMe: [true],
  });

  protected async submit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    try {
      const { email, password, rememberMe } = this.loginForm.getRawValue();
      await firstValueFrom(this.authApi.login({ email, password, rememberMe }));
      this.accountSession.refresh(true);
      await this.router.navigateByUrl(this.getSafeRedirectTo() ?? '/');
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Unable to sign you in right now.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected isInvalid(fieldName: 'email' | 'password'): boolean {
    const control = this.loginForm.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  private getSafeRedirectTo(): string | null {
    return this.redirectTo?.startsWith('/') ? this.redirectTo : null;
  }
}
