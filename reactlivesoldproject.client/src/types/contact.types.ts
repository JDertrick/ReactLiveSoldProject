export interface Contact {
  id: string;
  organizationId: string;
  firstName?: string;
  lastName?: string;
  email: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  company?: string;
  jobTitle?: string;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

export interface CreateContactDto {
  email: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  company?: string;
  jobTitle?: string;
  isActive?: boolean;
}

export interface UpdateContactDto {
  email?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  address?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  company?: string;
  jobTitle?: string;
  isActive?: boolean;
}
