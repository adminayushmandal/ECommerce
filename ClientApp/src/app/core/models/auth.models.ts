export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface SignupRequest {
  displayName: string;
  email: string;
  password: string;
  phoneNumber?: string | null;
}

export interface AuthenticatedUser {
  id: string;
  displayName: string;
  emailConfirmed: boolean;
  phoneNumberConfirmed: boolean;
  email: string;
  phoneNumber: string;
  isActive: boolean;
  roles: string[];
}
