// Organization Types
export interface Organization {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  primaryContactEmail: string;
  planType: string;
  isActive: boolean;
  createdAt: string;
}

export interface OrganizationPublic {
  name: string;
  logoUrl?: string;
}

export interface CreateOrganizationDto {
  name: string;
  slug?: string;
  logoUrl?: string;
  primaryContactEmail: string;
  planType: string;
}
