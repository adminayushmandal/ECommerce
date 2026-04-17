import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';

import { AuthApiService } from '../../../../core/services/auth-api.service';

const passwordMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password && confirmPassword && password !== confirmPassword
    ? { passwordMismatch: true }
    : null;
};

@Component({
  selector: 'app-signup-page',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule],
  templateUrl: './signup-page.component.html',
  styleUrl: './signup-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignupPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authApi = inject(AuthApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly redirectTo = this.route.snapshot.queryParamMap.get('redirectTo');
  private readonly checkoutIntent = this.route.snapshot.queryParamMap.get('intent') === 'checkout';

  protected readonly submitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly loginQueryParams: Record<string, string> = {
    ...(this.checkoutIntent ? { intent: 'checkout' } : {}),
    ...(this.getSafeRedirectTo() ? { redirectTo: this.getSafeRedirectTo()! } : {}),
  };
  protected readonly signupNotes = [
    'Display name is required because your account profile uses it immediately.',
    'Phone number is optional and can be added now for a more complete profile.',
    'After registration, you will be redirected to the login flow with your email prefilled.',
  ];

  protected readonly signupForm = this.fb.nonNullable.group(
    {
      displayName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.maxLength(20)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]],
    },
    { validators: passwordMatchValidator }
  );

  protected async submit(): Promise<void> {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    this.submitting.set(true);

    try {
      const { displayName, email, phoneNumber, password } = this.signupForm.getRawValue();

      await firstValueFrom(
        this.authApi.signup({
          displayName,
          email,
          password,
          phoneNumber,
        })
      );

      await this.router.navigate(['/identity/login'], {
        queryParams: {
          registered: 1,
          email,
          ...(this.checkoutIntent ? { intent: 'checkout' } : {}),
          ...(this.getSafeRedirectTo() ? { redirectTo: this.getSafeRedirectTo()! } : {}),
        },
      });
    } catch (error) {
      this.errorMessage.set(error instanceof Error ? error.message : 'Unable to create your account right now.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected isInvalid(fieldName: 'displayName' | 'email' | 'phoneNumber' | 'password' | 'confirmPassword'): boolean {
    const control = this.signupForm.controls[fieldName];
    return control.invalid && (control.dirty || control.touched);
  }

  protected hasPasswordMismatch(): boolean {
    return !!this.signupForm.errors?.['passwordMismatch'] &&
      (this.signupForm.controls.confirmPassword.dirty || this.signupForm.controls.confirmPassword.touched);
  }

  private getSafeRedirectTo(): string | null {
    return this.redirectTo?.startsWith('/') ? this.redirectTo : null;
  }
}
