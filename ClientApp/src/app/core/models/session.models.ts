import { AuthenticatedUser } from './auth.models';

export interface LookupResponse {
  currentUser: AuthenticatedUser | null;
  isAuthenticated: boolean;
}

export interface AccountPreferenceState {
  orderUpdates: boolean;
  wishListAlerts: boolean;
  restockNotices: boolean;
}
