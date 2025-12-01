using AutoMapper;
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
using ReactLiveSoldProject.ServerBL.Infrastructure.Services;
using ReactLiveSoldProject.ServerBL.Models.Configuration;
using ReactLiveSoldProject.ServerBL.DTOs.Configuration;

namespace ReactLiveSoldProject.ServerBL.Base
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map User
            CreateMap<UserProfileDto, User>();
            CreateMap<User, UserProfileDto>();
            CreateMap<CreateUserDto, User>();

            // Map OrganizationMember to UserProfileDto
            // Este mapeo toma datos del User pero el Role del OrganizationMember
            CreateMap<OrganizationMember, UserProfileDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.User.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.OrganizationId, opt => opt.MapFrom(src => src.OrganizationId))
                .ForMember(dest => dest.IsSuperAdmin, opt => opt.MapFrom(src => src.User.IsSuperAdmin));

            // Product Mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.ProductType.ToString()))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.TagLinks.Select(pt => pt.Tag)))
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src.Variants))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TagLinks, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore());
            CreateMap<Product, UpdateProductDto>();
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TagLinks, opt => opt.Ignore())
                .ForMember(dest => dest.Variants, opt => opt.Ignore());


            // ProductVariant Mappings
            CreateMap<ProductVariant, ProductVariantDto>();
            CreateMap<ProductVariantDto, ProductVariant>();
            CreateMap<CreateProductVariantDto, ProductVariant>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Size, opt => opt.Ignore())
                .ForMember(dest => dest.Color, opt => opt.Ignore())
                .ForMember(dest => dest.Product, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.SalesOrderItems, opt => opt.Ignore());
            CreateMap<ProductVariant, CreateProductVariantDto>();
            CreateMap<ProductVariant, UpdateProductVariantDto>();
            CreateMap<UpdateProductVariantDto, ProductVariant>();
            CreateMap<VariantProductDto, ProductVariant>();
            CreateMap<ProductVariant, VariantProductDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => src.Product.IsPublished))
                .ForMember(dest => dest.ProductType, opt => opt.MapFrom(src => src.Product.ProductType.ToString()))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price > 0 ? src.Price : src.Product.BasePrice))
                .ForMember(dest => dest.WholesalePrice, opt => opt.MapFrom(src => src.WholesalePrice > 0 ? src.WholesalePrice : src.Product.WholesalePrice));

            // Tag Mappings
            CreateMap<Tag, TagDto>();

            // Location Mappings
            CreateMap<Location, LocationDto>();
            CreateMap<LocationDto, Location>();
            CreateMap<Location, CreateLocationDto>();
            CreateMap<CreateLocationDto, Location>();
            CreateMap<Location, UpdateLocationDto>();
            CreateMap<UpdateLocationDto, Location>();

            // Category Mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Category, UpdateCategoryDto>();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<Category, CreateCategoryDto>();

            // Contacts
            CreateMap<Contact, ContactDto>();
            CreateMap<ContactDto, Contact>();
            CreateMap<CreateContactDto, Contact>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore());
            CreateMap<UpdateContactDto, Contact>()
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore());

            // Customers
            CreateMap<CustomerDto, Customer>();
            CreateMap<Customer, CustomerDto>();
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
            CreateMap<Customer, CreateCustomerDto>();
            CreateMap<Customer, UpdateCustomerDto>();

            // Vendors
            CreateMap<Vendor, VendorDto>();
            CreateMap<VendorDto, Vendor>();
            CreateMap<CreateVendorDto, Vendor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedBuyer, opt => opt.Ignore());
            CreateMap<UpdateVendorDto, Vendor>()
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore())
                .ForMember(dest => dest.Contact, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedBuyer, opt => opt.Ignore());

            // Wallet
            CreateMap<Wallet, WalletDto>();
            CreateMap<WalletDto, Wallet>();
            CreateMap<WalletTransaction, WalletTransactionDto>()
                .ForMember(dest => dest.AuthorizedByUserName, opt => opt.MapFrom(src =>
                    src.AuthorizedByUser != null ? $"{src.AuthorizedByUser.FirstName} {src.AuthorizedByUser.LastName}" : null))
                .ForMember(dest => dest.PostedByUserName, opt => opt.MapFrom(src =>
                    src.PostedByUser != null ? $"{src.PostedByUser.FirstName} {src.PostedByUser.LastName}" : null));
            CreateMap<WalletTransactionDto, WalletTransaction>();
            CreateMap<CreateWalletTransactionDto, WalletTransaction>();
            CreateMap<WalletTransaction, CreateWalletTransactionDto>();

            // StockMovement Mappings
            CreateMap<StockMovement, StockMovementDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductVariant.Product.Name))
                .ForMember(dest => dest.VariantSku, opt => opt.MapFrom(src => src.ProductVariant.Sku))
                .ForMember(dest => dest.MovementType, opt => opt.MapFrom(src => src.MovementType.ToString()))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}" : null))
                .ForMember(dest => dest.PostedByUserName, opt => opt.MapFrom(src =>
                    src.PostedByUser != null ? $"{src.PostedByUser.FirstName} {src.PostedByUser.LastName}" : null))
                .ForMember(dest => dest.RejectedByUserName, opt => opt.MapFrom(src =>
                    src.RejectedByUser != null ? $"{src.RejectedByUser.FirstName} {src.RejectedByUser.LastName}" : null));

            CreateMap<CreateStockMovementDto, StockMovement>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.StockBefore, opt => opt.Ignore())
                .ForMember(dest => dest.StockAfter, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsPosted, opt => opt.Ignore())
                .ForMember(dest => dest.PostedAt, opt => opt.Ignore())
                .ForMember(dest => dest.PostedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.IsRejected, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedAt, opt => opt.Ignore())
                .ForMember(dest => dest.RejectedByUserId, opt => opt.Ignore());

            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString().ToLower()));

            // Tax Mappings
            CreateMap<TaxRate, TaxRateDto>();
            CreateMap<TaxRateDto, TaxRate>();
            CreateMap<CreateTaxRateDto, TaxRate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore());
            CreateMap<UpdateTaxRateDto, TaxRate>()
                .ForMember(dest => dest.OrganizationId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Organization, opt => opt.Ignore());

            // Accounting Mappings
            CreateMap<ChartOfAccount, ChartOfAccountDto>()
                .ForMember(dest => dest.AccountType, opt => opt.MapFrom(src => src.AccountType));
            CreateMap<CreateChartOfAccountDto, ChartOfAccount>();
            CreateMap<UpdateChartOfAccountDto, ChartOfAccount>()
                 .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<JournalEntry, JournalEntryDto>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.JournalEntryLines));
            CreateMap<CreateJournalEntryDto, JournalEntry>()
                .ForMember(dest => dest.JournalEntryLines, opt => opt.MapFrom(src => src.Lines));

            CreateMap<JournalEntryLine, JournalEntryLineDto>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account.AccountName))
                .ForMember(dest => dest.AccountCode, opt => opt.MapFrom(src => src.Account.AccountCode))
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src => src.DebitAmount))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src => src.CreditAmount));
            CreateMap<CreateJournalEntryLineDto, JournalEntryLine>()
                .ForMember(dest => dest.DebitAmount, opt => opt.MapFrom(src => src.Debit))
                .ForMember(dest => dest.CreditAmount, opt => opt.MapFrom(src => src.Credit));

            // Purchase Order Mappings
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src =>
                    src.CreatedByUser != null
                        ? $"{src.CreatedByUser.FirstName} {src.CreatedByUser.LastName}".Trim()
                        : null))
                .ForMember(dest => dest.PaymentTermsDescription, opt => opt.MapFrom(src =>
                    src.PaymentTerms != null ? src.PaymentTerms.Description : null));
            CreateMap<CreatePurchaseOrderDto, PurchaseOrder>();

            // Purchase Order Item Mappings
            CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.VariantName, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Sku : null));
            CreateMap<CreatePurchaseOrderItemDto, PurchaseOrderItem>();

            // Vendor Invoice Mappings
            CreateMap<VendorInvoice, VendorInvoiceDto>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null))
                .ForMember(dest => dest.PurchaseReceiptNumber, opt => opt.MapFrom(src => src.PurchaseReceipt != null ? src.PurchaseReceipt.ReceiptNumber : null));
            CreateMap<CreateVendorInvoiceDto, VendorInvoice>();

            // Payment Terms Mappings
            CreateMap<PaymentTerms, PaymentTermsDto>();
            CreateMap<CreatePaymentTermsDto, PaymentTerms>();

            // Company Bank Account Mappings
            CreateMap<CompanyBankAccount, CompanyBankAccountDto>()
                .ForMember(dest => dest.GLAccountName, opt => opt.MapFrom(src => src.GLAccount != null ? src.GLAccount.AccountName : null));
            CreateMap<CreateCompanyBankAccountDto, CompanyBankAccount>();

            // Vendor Bank Account Mappings
            CreateMap<VendorBankAccount, VendorBankAccountDto>();
            CreateMap<CreateVendorBankAccountDto, VendorBankAccount>();

            // Purchase Receipt Mappings
            CreateMap<PurchaseReceipt, PurchaseReceiptDto>()
                .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.PONumber : null))
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null))
                .ForMember(dest => dest.WarehouseLocationName, opt => opt.MapFrom(src => src.WarehouseLocation != null ? src.WarehouseLocation.Name : null))
                .ForMember(dest => dest.ReceivedByName, opt => opt.MapFrom(src =>
                    src.ReceivedByUser != null
                        ? $"{src.ReceivedByUser.FirstName} {src.ReceivedByUser.LastName}".Trim()
                        : null));
            CreateMap<CreatePurchaseReceiptDto, PurchaseReceipt>();

            // Purchase Item Mappings
            CreateMap<PurchaseItem, PurchaseItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.VariantName, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Sku : null));
            CreateMap<CreatePurchaseItemDto, PurchaseItem>();

            // ProductVendor Mappings
            CreateMap<ProductVendor, ProductVendorDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>
                    src.Vendor != null && src.Vendor.Contact != null
                        ? (src.Vendor.Contact.Company ?? $"{src.Vendor.Contact.FirstName} {src.Vendor.Contact.LastName}".Trim())
                        : null));
            CreateMap<CreateProductVendorDto, ProductVendor>();
            CreateMap<UpdateProductVendorDto, ProductVendor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<NoSerie, NoSerieDto>()
                .ForMember(dest => dest.NoSerieLines, opt => opt.MapFrom(src => src.NoSerieLines));
            CreateMap<NoSerieDto, NoSerie>()
                .ForMember(dest => dest.NoSerieLines, opt => opt.MapFrom(src => src.NoSerieLines));
            CreateMap<NoSerieLine, NoSerieLineDto>();
            CreateMap<NoSerieLineDto, NoSerieLine>();
        }
    }
}
