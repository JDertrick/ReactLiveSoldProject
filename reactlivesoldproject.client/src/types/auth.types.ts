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
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    role: string;
    organizationId?: string;
    organizationSlug?: string;
    organizationName?: string;
    organizationLogoUrl?: string;
    isSuperAdmin: boolean;
  };
}

export interface UserProfile {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  role: string;
  organizationId?: string;
  organizationSlug?: string;
  organizationName?: string;
  organizationLogoUrl?: string;
}

export interface CustomerProfile {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  organizationId: string;
  walletBalance: number;
}
