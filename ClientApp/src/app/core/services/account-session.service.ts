import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';

import { AuthenticatedUser } from '../models/auth.models';
import { LookupResponse } from '../models/session.models';

interface SessionState {
  currentUser: AuthenticatedUser | null;
  loading: boolean;
  loaded: boolean;
}

@Injectable({ providedIn: 'root' })
export class AccountSessionService {
  private readonly http = inject(HttpClient);

  private readonly state = signal<SessionState>({
    currentUser: null,
    loading: false,
    loaded: false,
  });

  readonly currentUser = computed(() => this.state().currentUser);
  readonly loading = computed(() => this.state().loading);
  readonly loaded = computed(() => this.state().loaded);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  refresh(force = false): void {
    const currentState = this.state();
    if (currentState.loading || (currentState.loaded && !force)) {
      return;
    }

    this.state.update((state) => ({
      ...state,
      loading: true,
    }));

    this.http.get<LookupResponse>('/api/Lookup', { withCredentials: true }).subscribe({
      next: (lookup) => {
        this.state.set({
          currentUser: lookup.currentUser,
          loading: false,
          loaded: true,
        });
      },
      error: () => {
        this.state.set({
          currentUser: null,
          loading: false,
          loaded: true,
        });
      },
    });
  }

  markAuthenticated(user: AuthenticatedUser | null): void {
    this.state.set({
      currentUser: user,
      loading: false,
      loaded: true,
    });
  }

  clear(): void {
    this.state.set({
      currentUser: null,
      loading: false,
      loaded: true,
    });
  }
}
