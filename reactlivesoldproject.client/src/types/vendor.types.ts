import { Contact } from "./contact.types";

// Vendor Types
export interface Vendor {
  id: string;
  organizationId: string;
  contactId: string;
  contact?: Contact;
  assignedBuyerId?: string;
  assignedBuyerName?: string;
  vendorCode?: string;
  notes?: string;
  paymentTerms?: string;
  creditLimit?: number;
  createdAt: string;
  updatedAt: string;
  isActive: boolean;
}

export interface CreateVendorDto {
  contactId: string;
  assignedBuyerId?: string;
  vendorCode?: string;
  notes?: string;
  paymentTerms?: string;
  creditLimit: number;
  isActive: boolean;
}

export interface UpdateVendorDto {
  contactId?: string;
  assignedBuyerId?: string;
  vendorCode?: string;
  notes?: string;
  paymentTerms?: string;
  creditLimit?: number;
  isActive?: boolean;
}
