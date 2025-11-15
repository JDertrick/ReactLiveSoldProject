// Organization Types
export interface CustomizationSettings {
  primaryColor?: string;
  secondaryColor?: string;
  accentColor?: string;
  sidebarBg?: string;
  sidebarText?: string;
  sidebarActiveBg?: string;
  sidebarActiveText?: string;
}

export interface Organization {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  primaryContactEmail: string;
  planType: string;
  isActive: boolean;
  customizationSettings?: string; // JSON string
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

export interface UpdateOrganizationDto {
  name: string;
  logoUrl?: string;
  primaryContactEmail: string;
  planType?: string;
}

export interface UpdateOrganizationSettingsDto {
  name: string;
  logoUrl?: string;
  primaryContactEmail: string;
  customizationSettings?: string;
}
