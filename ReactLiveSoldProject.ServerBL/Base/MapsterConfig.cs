using Mapster;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.DTOs.Accounting;
using ReactLiveSoldProject.ServerBL.DTOs.Purchases;
using ReactLiveSoldProject.ServerBL.DTOs.Banking;
using ReactLiveSoldProject.ServerBL.DTOs.Vendors;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.Contacts;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Notifications;
using ReactLiveSoldProject.ServerBL.Models.Sales;
using ReactLiveSoldProject.ServerBL.Models.Taxes;
using ReactLiveSoldProject.ServerBL.Models.Vendors;
using ReactLiveSoldProject.ServerBL.Models.Purchases;
using ReactLiveSoldProject.ServerBL.Models.Banking;
using ReactLiveSoldProject.ServerBL.Models.Configuration;
using ReactLiveSoldProject.ServerBL.DTOs.Configuration;

namespace ReactLiveSoldProject.ServerBL.Base
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // User Mappings
            TypeAdapterConfig<UserProfileDto, User>.NewConfig();
            TypeAdapterConfig<User, UserProfileDto>.NewConfig();
            TypeAdapterConfig<CreateUserDto, User>.NewConfig();

            // OrganizationMember to UserProfileDto
            TypeAdapterConfig<OrganizationMember, UserProfileDto>
                .NewConfig()
                .Map(dest => dest.Id, src => src.User.Id)
                .Map(dest => dest.Email, src => src.User.Email)
                .Map(dest => dest.FirstName, src => src.User.FirstName)
                .Map(dest => dest.LastName, src => src.User.LastName)
                .Map(dest => dest.Role, src => src.Role.ToString())
                .Map(dest => dest.OrganizationId, src => src.OrganizationId)
                .Map(dest => dest.IsSuperAdmin, src => src.User.IsSuperAdmin);

            // Product Mappings
            TypeAdapterConfig<Product, ProductDto>
                .NewConfig()
                .Map(dest => dest.ProductType, src => src.ProductType.ToString())
                .Map(dest => dest.Tags, src => src.TagLinks.Select(pt => pt.Tag))
                .Map(dest => dest.Variants, src => src.Variants)
                .Map(dest => dest.Category, src => src.Category);

            TypeAdapterConfig<CreateProductDto, Product>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.TagLinks)
                .Ignore(dest => dest.Variants);

            TypeAdapterConfig<Product, UpdateProductDto>.NewConfig();

            TypeAdapterConfig<UpdateProductDto, Product>
                .NewConfig()
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.TagLinks)
                .Ignore(dest => dest.Variants);

            // ProductVariant Mappings
            TypeAdapterConfig<ProductVariant, ProductVariantDto>.NewConfig();
            TypeAdapterConfig<ProductVariantDto, ProductVariant>.NewConfig();

            TypeAdapterConfig<CreateProductVariantDto, ProductVariant>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.ProductId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Size)
                .Ignore(dest => dest.Color)
                .Ignore(dest => dest.Product)
                .Ignore(dest => dest.Organization)
                .Ignore(dest => dest.SalesOrderItems);

            TypeAdapterConfig<ProductVariant, CreateProductVariantDto>.NewConfig();
            TypeAdapterConfig<ProductVariant, UpdateProductVariantDto>.NewConfig();
            TypeAdapterConfig<UpdateProductVariantDto, ProductVariant>.NewConfig();
            TypeAdapterConfig<VariantProductDto, ProductVariant>.NewConfig();

            TypeAdapterConfig<ProductVariant, VariantProductDto>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product.Name)
                .Map(dest => dest.ProductDescription, src => src.Product.Description)
                .Map(dest => dest.IsPublished, src => src.Product.IsPublished)
                .Map(dest => dest.ProductType, src => src.Product.ProductType.ToString())
                .Map(dest => dest.Price, src => src.Price > 0 ? src.Price : src.Product.BasePrice)
                .Map(dest => dest.WholesalePrice, src => src.WholesalePrice > 0 ? src.WholesalePrice : src.Product.WholesalePrice);

            // Tag Mappings
            TypeAdapterConfig<Tag, TagDto>.NewConfig();

            // Location Mappings
            TypeAdapterConfig<Location, LocationDto>.NewConfig();
            TypeAdapterConfig<LocationDto, Location>.NewConfig();
            TypeAdapterConfig<Location, CreateLocationDto>.NewConfig();
            TypeAdapterConfig<CreateLocationDto, Location>.NewConfig();
            TypeAdapterConfig<Location, UpdateLocationDto>.NewConfig();
            TypeAdapterConfig<UpdateLocationDto, Location>.NewConfig();

            // Category Mappings
            TypeAdapterConfig<Category, CategoryDto>.NewConfig();
            TypeAdapterConfig<CategoryDto, Category>.NewConfig();
            TypeAdapterConfig<UpdateCategoryDto, Category>.NewConfig();
            TypeAdapterConfig<Category, UpdateCategoryDto>.NewConfig();
            TypeAdapterConfig<CreateCategoryDto, Category>.NewConfig();
            TypeAdapterConfig<Category, CreateCategoryDto>.NewConfig();

            // Contact Mappings
            TypeAdapterConfig<Contact, ContactDto>.NewConfig();
            TypeAdapterConfig<ContactDto, Contact>.NewConfig();

            TypeAdapterConfig<CreateContactDto, Contact>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Organization);

            TypeAdapterConfig<UpdateContactDto, Contact>
                .NewConfig()
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Organization);

            // Customer Mappings
            TypeAdapterConfig<CustomerDto, Customer>.NewConfig();
            TypeAdapterConfig<Customer, CustomerDto>.NewConfig();
            TypeAdapterConfig<CreateCustomerDto, Customer>.NewConfig();
            TypeAdapterConfig<UpdateCustomerDto, Customer>.NewConfig();
            TypeAdapterConfig<Customer, CreateCustomerDto>.NewConfig();
            TypeAdapterConfig<Customer, UpdateCustomerDto>.NewConfig();

            // Vendor Mappings
            TypeAdapterConfig<Vendor, VendorDto>.NewConfig();
            TypeAdapterConfig<VendorDto, Vendor>.NewConfig();

            TypeAdapterConfig<CreateVendorDto, Vendor>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Organization)
                .Ignore(dest => dest.Contact)
                .Ignore(dest => dest.AssignedBuyer);

            TypeAdapterConfig<UpdateVendorDto, Vendor>
                .NewConfig()
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Organization)
                .Ignore(dest => dest.Contact)
                .Ignore(dest => dest.AssignedBuyer);

            // Wallet Mappings
            TypeAdapterConfig<Wallet, WalletDto>.NewConfig();
            TypeAdapterConfig<WalletDto, Wallet>.NewConfig();

            TypeAdapterConfig<WalletTransaction, WalletTransactionDto>
                .NewConfig()
                .Map(dest => dest.AuthorizedByUserName, src =>
                    src.AuthorizedByUser != null ? $"{src.AuthorizedByUser.FirstName} {src.AuthorizedByUser.LastName}" : null)
                .Map(dest => dest.PostedByUserName, src =>
                    src.PostedByUser != null ? $"{src.PostedByUser.FirstName} {src.PostedByUser.LastName}" : null);

            TypeAdapterConfig<WalletTransactionDto, WalletTransaction>.NewConfig();
            TypeAdapterConfig<CreateWalletTransactionDto, WalletTransaction>.NewConfig();
            TypeAdapterConfig<WalletTransaction, CreateWalletTransactionDto>.NewConfig();

            // StockMovement Mappings
            TypeAdapterConfig<StockMovement, StockMovementDto>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.ProductVariant.Product.Name)
                .Map(dest => dest.VariantSku, src => src.ProductVariant.Sku)
                .Map(dest => dest.MovementType, src => src.MovementType.ToString())
                .Map(dest => dest.CreatedByUserName, src =>
                    src.CreatedByUser != null ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}" : null)
                .Map(dest => dest.PostedByUserName, src =>
                    src.PostedByUser != null ? $"{src.PostedByUser.FirstName} {src.PostedByUser.LastName}" : null)
                .Map(dest => dest.RejectedByUserName, src =>
                    src.RejectedByUser != null ? $"{src.RejectedByUser.FirstName} {src.RejectedByUser.LastName}" : null);

            TypeAdapterConfig<CreateStockMovementDto, StockMovement>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.StockBefore)
                .Ignore(dest => dest.StockAfter)
                .Ignore(dest => dest.CreatedByUserId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.IsPosted)
                .Ignore(dest => dest.PostedAt)
                .Ignore(dest => dest.PostedByUserId)
                .Ignore(dest => dest.IsRejected)
                .Ignore(dest => dest.RejectedAt)
                .Ignore(dest => dest.RejectedByUserId);

            // Notification Mappings
            TypeAdapterConfig<Notification, NotificationDto>
                .NewConfig()
                .Map(dest => dest.Type, src => src.Type.ToString().ToLower());

            // Tax Mappings
            TypeAdapterConfig<TaxRate, TaxRateDto>.NewConfig();
            TypeAdapterConfig<TaxRateDto, TaxRate>.NewConfig();

            TypeAdapterConfig<CreateTaxRateDto, TaxRate>
                .NewConfig()
                .Ignore(dest => dest.Id)
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Organization);

            TypeAdapterConfig<UpdateTaxRateDto, TaxRate>
                .NewConfig()
                .Ignore(dest => dest.OrganizationId)
                .Ignore(dest => dest.CreatedAt)
                .Ignore(dest => dest.UpdatedAt)
                .Ignore(dest => dest.Organization);

            // Accounting Mappings
            TypeAdapterConfig<ChartOfAccount, ChartOfAccountDto>
                .NewConfig()
                .Map(dest => dest.AccountType, src => src.AccountType);

            TypeAdapterConfig<CreateChartOfAccountDto, ChartOfAccount>.NewConfig();

            TypeAdapterConfig<UpdateChartOfAccountDto, ChartOfAccount>
                .NewConfig()
                .IgnoreNullValues(true);

            TypeAdapterConfig<JournalEntry, JournalEntryDto>
                .NewConfig()
                .Map(dest => dest.Lines, src => src.JournalEntryLines);

            TypeAdapterConfig<CreateJournalEntryDto, JournalEntry>
                .NewConfig()
                .Map(dest => dest.JournalEntryLines, src => src.Lines);

            TypeAdapterConfig<JournalEntryLine, JournalEntryLineDto>
                .NewConfig()
                .Map(dest => dest.AccountName, src => src.Account.AccountName)
                .Map(dest => dest.AccountCode, src => src.Account.AccountCode)
                .Map(dest => dest.Debit, src => src.DebitAmount)
                .Map(dest => dest.Credit, src => src.CreditAmount);

            TypeAdapterConfig<CreateJournalEntryLineDto, JournalEntryLine>
                .NewConfig()
                .Map(dest => dest.DebitAmount, src => src.Debit)
                .Map(dest => dest.CreditAmount, src => src.Credit);

            // Purchase Order Mappings
            TypeAdapterConfig<PurchaseOrder, PurchaseOrderDto>
                .NewConfig()
                .Map(dest => dest.VendorName, src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null)
                .Map(dest => dest.CreatedByName, src =>
                    src.CreatedByUser != null
                        ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}".Trim()
                        : null)
                .Map(dest => dest.PaymentTermsDescription, src =>
                    src.PaymentTerms != null ? src.PaymentTerms.Description : null);

            TypeAdapterConfig<CreatePurchaseOrderDto, PurchaseOrder>.NewConfig();

            // Purchase Order Item Mappings
            TypeAdapterConfig<PurchaseOrderItem, PurchaseOrderItemDto>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
                .Map(dest => dest.VariantName, src => src.ProductVariant != null ? src.ProductVariant.Sku : null);

            TypeAdapterConfig<CreatePurchaseOrderItemDto, PurchaseOrderItem>.NewConfig();

            // Vendor Invoice Mappings
            TypeAdapterConfig<VendorInvoice, VendorInvoiceDto>
                .NewConfig()
                .Map(dest => dest.VendorName, src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null)
                .Map(dest => dest.PurchaseReceiptNumber, src => src.PurchaseReceipt != null ? src.PurchaseReceipt.ReceiptNumber : null);

            TypeAdapterConfig<CreateVendorInvoiceDto, VendorInvoice>.NewConfig();

            // Payment Terms Mappings
            TypeAdapterConfig<PaymentTerms, PaymentTermsDto>.NewConfig();
            TypeAdapterConfig<CreatePaymentTermsDto, PaymentTerms>.NewConfig();

            // Company Bank Account Mappings
            TypeAdapterConfig<CompanyBankAccount, CompanyBankAccountDto>
                .NewConfig()
                .Map(dest => dest.GLAccountName, src => src.GLAccount != null ? src.GLAccount.AccountName : null);

            TypeAdapterConfig<CreateCompanyBankAccountDto, CompanyBankAccount>.NewConfig();

            // Vendor Bank Account Mappings
            TypeAdapterConfig<VendorBankAccount, VendorBankAccountDto>.NewConfig();
            TypeAdapterConfig<CreateVendorBankAccountDto, VendorBankAccount>.NewConfig();

            // Purchase Receipt Mappings
            TypeAdapterConfig<PurchaseReceipt, PurchaseReceiptDto>
                .NewConfig()
                .Map(dest => dest.PurchaseOrderNumber, src => src.PurchaseOrder != null ? src.PurchaseOrder.PONumber : null)
                .Map(dest => dest.VendorName, src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null)
                .Map(dest => dest.WarehouseLocationName, src => src.WarehouseLocation != null ? src.WarehouseLocation.Name : null)
                .Map(dest => dest.ReceivedByName, src =>
                    src.ReceivedByUser != null
                        ? $"{src.ReceivedByUser.FirstName} {src.ReceivedByUser.LastName}".Trim()
                        : null);

            TypeAdapterConfig<CreatePurchaseReceiptDto, PurchaseReceipt>.NewConfig();

            // Purchase Item Mappings
            TypeAdapterConfig<PurchaseItem, PurchaseItemDto>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
                .Map(dest => dest.VariantName, src => src.ProductVariant != null ? src.ProductVariant.Sku : null);

            TypeAdapterConfig<CreatePurchaseItemDto, PurchaseItem>.NewConfig();

            // ProductVendor Mappings
            TypeAdapterConfig<ProductVendor, ProductVendorDto>
                .NewConfig()
                .Map(dest => dest.ProductName, src => src.Product != null ? src.Product.Name : null)
                .Map(dest => dest.VendorName, src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null);

            TypeAdapterConfig<CreateProductVendorDto, ProductVendor>.NewConfig();

            TypeAdapterConfig<UpdateProductVendorDto, ProductVendor>
                .NewConfig()
                .IgnoreNullValues(true);

            // NoSerie Mappings
            TypeAdapterConfig<NoSerie, NoSerieDto>
                .NewConfig()
                .Map(dest => dest.NoSerieLines, src => src.NoSerieLines);

            TypeAdapterConfig<NoSerieDto, NoSerie>
                .NewConfig()
                .Map(dest => dest.NoSerieLines, src => src.NoSerieLines);

            TypeAdapterConfig<NoSerieLine, NoSerieLineDto>.NewConfig();
            TypeAdapterConfig<NoSerieLineDto, NoSerieLine>.NewConfig();
        }
    }
}
