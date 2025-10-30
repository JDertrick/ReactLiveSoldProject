// Auth Types
export interface LoginRequest {
  email: string;
  password: string;
}

export interface CustomerPortalLoginRequest {
  email: string;
  password: string;
  organizationSlug: string;
}

export interface LoginResponse {
  token: string;
  email: string;
  role: string;
  organizationId?: string;
  name?: string;
}

export interface UserProfile {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role: string;
  organizationId?: string;
  organizationName?: string;
}

export interface CustomerProfile {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  organizationId: string;
  walletBalance: number;
}
