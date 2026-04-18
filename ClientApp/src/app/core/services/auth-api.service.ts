import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import { AuthenticatedUser, LoginRequest, SignupRequest } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly usersApiUrl = '/api/Users';

  login(request: LoginRequest): Observable<void> {
    const params = request.rememberMe
      ? new HttpParams().set('useCookies', 'true')
      : new HttpParams().set('useSessionCookies', 'true');

    return this.http
      .post<void>(
        `${this.usersApiUrl}/login`,
        {
          email: request.email,
          password: request.password,
        },
        {
          params,
          withCredentials: true,
        }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  signup(request: SignupRequest): Observable<AuthenticatedUser> {
    return this.http
      .post<AuthenticatedUser>(
        `${this.usersApiUrl}/register-profile`,
        {
          displayName: request.displayName,
          email: request.email,
          password: request.password,
          phoneNumber: request.phoneNumber ?? null,
        },
        {
          withCredentials: true,
        }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  logout(): Observable<void> {
    return this.http
      .post<void>(
        `${this.usersApiUrl}/logout`,
        {},
        {
          withCredentials: true,
        }
      )
      .pipe(catchError((error) => this.handleError(error)));
  }

  private handleError(error: HttpErrorResponse) {
    const fallbackMessage = 'Something went wrong. Please try again.';
    const message =
      (typeof error.error === 'string' && error.error) ||
      error.error?.detail ||
      error.error?.title ||
      error.message ||
      fallbackMessage;

    return throwError(() => new Error(message));
  }
}
