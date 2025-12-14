using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Models.Accounting;
using ReactLiveSoldProject.ServerBL.Models.Audit;
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
using ReactLiveSoldProject.ServerBL.Models.Payments;
using ReactLiveSoldProject.ServerBL.Models.Configuration;

namespace ReactLiveSoldProject.ServerBL.Base
{
    public class LiveSoldDbContext : DbContext
    {
        public LiveSoldDbContext(DbContextOptions<LiveSoldDbContext> options) : base(options)
        {
        }

        // BLOQUE 1: PLATAFORMA SAAS Y AUTENTICACIÓN
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<OrganizationMember> OrganizationMembers { get; set; }

        // BLOQUE 2: CONTACTOS, CLIENTES, PROVEEDORES Y BILLETERA
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Receipt> Receipts { get; set; }
        public DbSet<ReceiptItem> ReceiptItems { get; set; }


        // BLOQUE 3: INVENTARIO
        public DbSet<Location> Locations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }
        public DbSet<InventoryAudit> InventoryAudits { get; set; }
        public DbSet<InventoryAuditItem> InventoryAuditItems { get; set; }

        // BLOQUE 4: VENTAS
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        // BLOQUE 5: IMPUESTOS
        public DbSet<TaxRate> TaxRates { get; set; }

        // BLOQUE 6: CONTABILIDAD
        public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<JournalEntryLine> JournalEntryLines { get; set; }
        public DbSet<OrganizationAccountConfiguration> OrganizationAccountConfigurations { get; set; }

        // BLOQUE 7: COMPRAS Y PAGOS
        public DbSet<PaymentTerms> PaymentTerms { get; set; }
        public DbSet<VendorBankAccount> VendorBankAccounts { get; set; }
        public DbSet<CompanyBankAccount> CompanyBankAccounts { get; set; }
        public DbSet<ProductVendor> ProductVendors { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<VendorInvoice> VendorInvoices { get; set; }
        public DbSet<StockBatch> StockBatches { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentApplication> PaymentApplications { get; set; }
        public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }

        // BLOQUE 8: AUDITORÍA Y NOTIFICACIONES
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // BLOQUE 9: SERIE NO - SETUP
        public DbSet<NoSerie> NoSeries { get; set; }
        public DbSet<NoSerieLine> NoSerieLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Configuración existente ---

            // --- BLOQUE 1: PLATAFORMA SAAS Y AUTENTICACIÓN ---

            modelBuilder.Entity<Organization>(e =>
            {
                e.ToTable("Organizations");
                e.HasKey(o => o.Id);
                e.Property(o => o.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(o => o.Name).HasColumnName("name").IsRequired();
                e.Property(o => o.Slug).HasColumnName("slug").IsRequired();
                e.Property(o => o.LogoUrl).HasColumnName("logo_url");
                e.Property(o => o.PrimaryContactEmail).HasColumnName("primary_contact_email").IsRequired();
                e.Property(o => o.PlanType)
                    .HasColumnName("plan_type")
                    .HasConversion<string>()
                    .IsRequired()
                    .HasDefaultValue(PlanType.Free)
                    .HasSentinel(PlanType.None);
                e.Property(o => o.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
                e.Property(o => o.CustomizationSettings).HasColumnName("custom_settings").IsRequired().HasDefaultValue(true);
                e.Property(o => o.TaxEnabled).HasColumnName("tax_enabled").HasDefaultValue(false);
                e.Property(o => o.TaxSystemType).HasColumnName("tax_system_type").HasConversion<string>().HasDefaultValue(TaxSystemType.None);
                e.Property(o => o.TaxDisplayName).HasColumnName("tax_display_name").HasMaxLength(50);
                e.Property(o => o.TaxApplicationMode).HasColumnName("tax_application_mode").HasConversion<string>().HasDefaultValue(TaxApplicationMode.TaxIncluded);
                e.Property(o => o.DefaultTaxRateId).HasColumnName("default_tax_rate_id");
                e.Property(o => o.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(o => o.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                // Índice único para el slug (necesario para rutas del portal)
                e.HasIndex(o => o.Slug).IsUnique();
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.HasKey(u => u.Id);
                e.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(u => u.FirstName).HasColumnName("first_name");
                e.Property(u => u.LastName).HasColumnName("last_name");
                e.Property(u => u.Email).HasColumnName("email").IsRequired();
                e.HasIndex(u => u.Email).IsUnique();
                e.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
                e.Property(u => u.IsSuperAdmin).HasColumnName("is_super_admin").IsRequired().HasDefaultValue(false);
                e.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(u => u.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");
            });

            modelBuilder.Entity<OrganizationMember>(e =>
            {
                e.ToTable("OrganizationMembers");
                e.HasKey(om => om.Id);
                e.Property(om => om.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(om => om.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(om => om.UserId).HasColumnName("user_id").IsRequired();
                e.Property(om => om.Role).HasColumnName("role").HasConversion<string>().IsRequired().HasDefaultValue(UserRole.Seller);
                e.Property(om => om.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(om => new { om.OrganizationId, om.UserId }).IsUnique();

                e.HasOne(om => om.Organization)
                    .WithMany(o => o.Members)
                    .HasForeignKey(om => om.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL

                e.HasOne(om => om.User)
                    .WithMany(u => u.OrganizationLinks)
                    .HasForeignKey(om => om.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL
            });

            // --- BLOQUE 2: CONTACTOS, CLIENTES, PROVEEDORES Y BILLETERA ---

            modelBuilder.Entity<Contact>(e =>
            {
                e.ToTable("Contacts");
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(c => c.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(c => c.FirstName).HasColumnName("first_name");
                e.Property(c => c.LastName).HasColumnName("last_name");
                e.Property(c => c.Email).HasColumnName("email").IsRequired();
                e.Property(c => c.Phone).HasColumnName("phone");
                e.Property(c => c.Address).HasColumnName("address");
                e.Property(c => c.City).HasColumnName("city");
                e.Property(c => c.State).HasColumnName("state");
                e.Property(c => c.PostalCode).HasColumnName("postal_code");
                e.Property(c => c.Country).HasColumnName("country");
                e.Property(c => c.Company).HasColumnName("company");
                e.Property(c => c.JobTitle).HasColumnName("job_title");
                e.Property(c => c.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
                e.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(c => new { c.OrganizationId, c.Email }).IsUnique();
                e.HasIndex(c => new { c.OrganizationId, c.Phone }).IsUnique().HasFilter("\"phone\" IS NOT NULL");

                e.HasOne(c => c.Organization)
                    .WithMany()
                    .HasForeignKey(c => c.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Customer>(e =>
            {
                e.ToTable("Customers");
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(c => c.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(c => c.ContactId).HasColumnName("contact_id").IsRequired();
                e.Property(c => c.PasswordHash).HasColumnName("password_hash").IsRequired();
                e.Property(c => c.AssignedSellerId).HasColumnName("assigned_seller_id");
                e.Property(c => c.Notes).HasColumnName("notes");
                e.Property(c => c.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
                e.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(c => c.ContactId).IsUnique();

                e.HasOne(c => c.Organization)
                    .WithMany(o => o.Customers)
                    .HasForeignKey(c => c.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(c => c.Contact)
                    .WithMany()
                    .HasForeignKey(c => c.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(c => c.AssignedSeller)
                    .WithMany(u => u.AssignedCustomers)
                    .HasForeignKey(c => c.AssignedSellerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Vendor>(e =>
            {
                e.ToTable("Vendors");
                e.HasKey(v => v.Id);
                e.Property(v => v.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(v => v.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(v => v.ContactId).HasColumnName("contact_id").IsRequired();
                e.Property(v => v.AssignedBuyerId).HasColumnName("assigned_buyer_id");
                e.Property(v => v.VendorCode).HasColumnName("vendor_code");
                e.Property(v => v.Notes).HasColumnName("notes");
                e.Property(v => v.PaymentTerms).HasColumnName("payment_terms");
                e.Property(v => v.CreditLimit).HasColumnName("credit_limit").HasColumnType("decimal(10, 2)").IsRequired().HasDefaultValue(0.00m);
                e.Property(v => v.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
                e.Property(v => v.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(v => v.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(v => v.ContactId).IsUnique();
                e.HasIndex(v => new { v.OrganizationId, v.VendorCode }).IsUnique().HasFilter("\"vendor_code\" IS NOT NULL");

                e.HasOne(v => v.Organization)
                    .WithMany()
                    .HasForeignKey(v => v.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(v => v.Contact)
                    .WithMany()
                    .HasForeignKey(v => v.ContactId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(v => v.AssignedBuyer)
                    .WithMany()
                    .HasForeignKey(v => v.AssignedBuyerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Wallet>(e =>
            {
                e.ToTable("Wallets");
                e.HasKey(w => w.Id);
                e.Property(w => w.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(w => w.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(w => w.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(w => w.Balance).HasColumnName("balance").HasColumnType("decimal(10, 2)").IsRequired().HasDefaultValue(0.00m);
                e.Property(w => w.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(w => w.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(w => w.CustomerId).IsUnique(); // Relación 1-a-1

                e.HasOne(w => w.Organization)
                    .WithMany() // Sin navegación inversa desde Organization
                    .HasForeignKey(w => w.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(w => w.Customer)
                    .WithOne(c => c.Wallet)
                    .HasForeignKey<Wallet>(w => w.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL
            });

            modelBuilder.Entity<WalletTransaction>(e =>
            {
                e.ToTable("WalletTransactions");
                e.HasKey(wt => wt.Id);
                e.Property(wt => wt.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(wt => wt.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(wt => wt.WalletId).HasColumnName("wallet_id").IsRequired();
                e.Property(wt => wt.Type).HasColumnName("type").HasConversion<string>().IsRequired();
                e.Property(wt => wt.Amount).HasColumnName("amount").HasColumnType("decimal(10, 2)").IsRequired();
                e.Property(wt => wt.RelatedSalesOrderId).HasColumnName("related_sales_order_id");
                e.Property(wt => wt.AuthorizedByUserId).HasColumnName("authorized_by_user_id");
                e.Property(wt => wt.Notes).HasColumnName("notes");
                e.Property(wt => wt.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(wt => wt.PostedAt).HasColumnName("posted_at").IsRequired();
                e.Property(wt => wt.PostedByUserId).HasColumnName("posted_by_user_id").IsRequired();
                e.Property(wt => wt.IsPosted).HasColumnName("is_posted").IsRequired().HasDefaultValue(false);

                e.HasOne(wt => wt.Organization)
                    .WithMany()
                    .HasForeignKey(wt => wt.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(wt => wt.Wallet)
                    .WithMany(w => w.Transactions)
                    .HasForeignKey(wt => wt.WalletId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(wt => wt.AuthorizedByUser)
                    .WithMany(u => u.AuthorizedTransactions)
                    .HasForeignKey(wt => wt.AuthorizedByUserId)
                    .OnDelete(DeleteBehavior.SetNull); // Como en el SQL

                // Relación con SalesOrder (configurada en SalesOrder)
            });

            modelBuilder.Entity<Receipt>(e =>
            {
                e.ToTable("Receipts");
                e.HasKey(r => r.Id);
                e.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(r => r.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(r => r.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(r => r.WalletTransactionId).HasColumnName("wallet_transaction_id"); // Nullable
                e.Property(r => r.Type).HasColumnName("type").HasConversion<string>().IsRequired();
                e.Property(r => r.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10, 2)").IsRequired();
                e.Property(r => r.Notes).HasColumnName("notes");
                e.Property(r => r.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
                e.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(r => r.IsPosted).HasColumnName("is_posted").IsRequired().HasDefaultValue(false);
                e.Property(r => r.PostedAt).HasColumnName("posted_at");
                e.Property(r => r.PostedByUserId).HasColumnName("posted_by_user_id");
                e.Property(r => r.IsRejected).HasColumnName("is_rejected").IsRequired().HasDefaultValue(false);
                e.Property(r => r.RejectedAt).HasColumnName("rejected_at");
                e.Property(r => r.RejectedByUserId).HasColumnName("rejected_by_user_id");

                e.HasIndex(r => r.WalletTransactionId).IsUnique().HasFilter("\"wallet_transaction_id\" IS NOT NULL");

                e.HasOne(r => r.Organization)
                    .WithMany()
                    .HasForeignKey(r => r.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.Customer)
                    .WithMany()
                    .HasForeignKey(r => r.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.WalletTransaction)
                    .WithOne(wt => wt.Receipt)
                    .HasForeignKey<Receipt>(r => r.WalletTransactionId)
                    .OnDelete(DeleteBehavior.SetNull); // Use SetNull as it's nullable

                e.HasOne(r => r.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.PostedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.PostedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(r => r.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ReceiptItem>(e =>
            {
                e.ToTable("ReceiptItems");
                e.HasKey(ri => ri.Id);
                e.Property(ri => ri.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(ri => ri.ReceiptId).HasColumnName("receipt_id").IsRequired();
                e.Property(ri => ri.Description).HasColumnName("description").IsRequired();
                e.Property(ri => ri.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10, 2)").IsRequired();
                e.Property(ri => ri.Quantity).HasColumnName("quantity").IsRequired();
                e.Property(ri => ri.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10, 2)").IsRequired();

                e.HasOne(ri => ri.Receipt)
                    .WithMany(r => r.Items)
                    .HasForeignKey(ri => ri.ReceiptId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- BLOQUE 3: INVENTARIO ---

            modelBuilder.Entity<Tag>(e =>
            {
                e.ToTable("Tags");
                e.HasKey(t => t.Id);
                e.Property(t => t.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(t => t.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(t => t.Name).HasColumnName("name").IsRequired();

                e.HasIndex(t => new { t.OrganizationId, t.Name }).IsUnique();

                e.HasOne(t => t.Organization)
                    .WithMany(o => o.Tags)
                    .HasForeignKey(t => t.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("Products");
                e.HasKey(p => p.Id);
                e.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(p => p.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(p => p.Name).HasColumnName("name").IsRequired();
                e.Property(p => p.Description).HasColumnName("description");
                e.Property(p => p.ProductType).HasColumnName("product_type").HasConversion<string>().IsRequired().HasDefaultValue(ProductType.Simple);
                e.Property(p => p.IsPublished).HasColumnName("is_published").IsRequired().HasDefaultValue(true);
                e.Property(p => p.IsTaxExempt).HasColumnName("is_tax_exempt").HasDefaultValue(false);
                e.Property(p => p.ImageUrl).HasColumnName("image_url").IsRequired(false);
                e.Property(p => p.BasePrice).HasColumnName("base_price").HasDefaultValue(0);
                e.Property(p => p.CategoryId).HasColumnName("category_id");
                e.Property(pv => pv.WholesalePrice).HasColumnName("wholesale_price").HasColumnType("decimal(10, 2)").IsRequired(false);
                e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasOne(p => p.Organization)
                    .WithMany(o => o.Products)
                    .HasForeignKey(p => p.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Location>(e =>
            {
                e.ToTable("Locations");
                e.HasKey(l => l.Id);
                e.Property(l => l.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(l => l.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(l => l.Name).HasColumnName("name").IsRequired();
                e.Property(l => l.Description).HasColumnName("description");
                e.Property(l => l.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(l => l.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(l => new { l.OrganizationId, l.Name }).IsUnique();

                e.HasOne(l => l.Organization)
                    .WithMany()
                    .HasForeignKey(l => l.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(c => c.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(c => c.Name).HasColumnName("name").IsRequired();
                e.Property(c => c.Description).HasColumnName("description");
                e.Property(c => c.ParentId).HasColumnName("parent_id");
                e.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(c => new { c.OrganizationId, c.Name }).IsUnique();

                e.HasOne(c => c.Organization)
                    .WithMany()
                    .HasForeignKey(c => c.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(c => c.Parent)
                    .WithMany(c => c.Children)
                    .HasForeignKey(c => c.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<ProductTag>(e =>
            {
                e.ToTable("ProductTags");
                e.HasKey(pt => new { pt.ProductId, pt.TagId }); // Clave compuesta
                e.Property(pt => pt.ProductId).HasColumnName("product_id");
                e.Property(pt => pt.TagId).HasColumnName("tag_id");

                e.HasOne(pt => pt.Product)
                    .WithMany(p => p.TagLinks)
                    .HasForeignKey(pt => pt.ProductId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL

                e.HasOne(pt => pt.Tag)
                    .WithMany(t => t.ProductLinks)
                    .HasForeignKey(pt => pt.TagId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL
            });

            modelBuilder.Entity<ProductVariant>(e =>
            {
                e.ToTable("ProductVariants");
                e.HasKey(pv => pv.Id);
                e.Property(pv => pv.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(pv => pv.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(pv => pv.ProductId).HasColumnName("product_id").IsRequired();
                e.Property(pv => pv.Sku).HasColumnName("sku");
                e.Property(pv => pv.Price).HasColumnName("price").HasColumnType("decimal(10, 2)").IsRequired().HasDefaultValue(0.00m);
                e.Property(pv => pv.WholesalePrice).HasColumnName("wholesale_price").HasColumnType("decimal(10, 2)").IsRequired(false);
                e.Property(pv => pv.StockQuantity).HasColumnName("stock_quantity").IsRequired().HasDefaultValue(0);
                e.Property(pv => pv.Attributes).HasColumnName("attributes").HasColumnType("jsonb");
                e.Property(pv => pv.ImageUrl).HasColumnName("image_url");
                e.Property(pv => pv.Size).HasColumnName("size").IsRequired(false);
                e.Property(pv => pv.Color).HasColumnName("color").IsRequired(false);
                e.Property(pv => pv.ImageUrl).HasColumnName("image_url");
                e.Property(pv => pv.AverageCost).HasColumnName("average_cost");
                e.Property(pv => pv.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(pv => pv.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(pv => new { pv.OrganizationId, pv.Sku }).IsUnique().HasFilter("\"sku\" IS NOT NULL"); // Índice único solo si SKU no es nulo

                e.HasOne(pv => pv.Organization)
                    .WithMany()
                    .HasForeignKey(pv => pv.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(pv => pv.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(pv => pv.ProductId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL
            });

            modelBuilder.Entity<StockMovement>(e =>
            {
                e.ToTable("StockMovements");
                e.HasKey(sm => sm.Id);
                e.Property(sm => sm.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(sm => sm.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(sm => sm.ProductVariantId).HasColumnName("product_variant_id").IsRequired();
                e.Property(sm => sm.MovementType).HasColumnName("movement_type").HasConversion<string>().IsRequired();
                e.Property(sm => sm.Quantity).HasColumnName("quantity").IsRequired();
                e.Property(sm => sm.StockBefore).HasColumnName("stock_before").IsRequired();
                e.Property(sm => sm.StockAfter).HasColumnName("stock_after").IsRequired();
                e.Property(sm => sm.RelatedSalesOrderId).HasColumnName("related_sales_order_id");
                e.Property(sm => sm.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
                e.Property(sm => sm.Notes).HasColumnName("notes");
                e.Property(sm => sm.Reference).HasColumnName("reference");
                e.Property(sm => sm.IsPosted).HasColumnName("is_posted").IsRequired().HasDefaultValue(false);
                e.Property(sm => sm.PostedByUserId).HasColumnName("posted_by_user_id");
                e.Property(sm => sm.PostedAt).HasColumnName("posted_at");
                e.Property(sm => sm.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(10, 2)");
                e.Property(sm => sm.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(sm => sm.IsRejected).HasColumnName("is_rejected").IsRequired().HasDefaultValue(false);
                e.Property(sm => sm.RejectedAt).HasColumnName("rejected_at");
                e.Property(sm => sm.RejectedByUserId).HasColumnName("rejected_by_user_id");
                e.Property(sm => sm.SourceLocationId).HasColumnName("source_location_id");
                e.Property(sm => sm.DestinationLocationId).HasColumnName("destination_location_id");

                // Índices para consultas frecuentes
                e.HasIndex(sm => sm.ProductVariantId);
                e.HasIndex(sm => sm.RelatedSalesOrderId);
                e.HasIndex(sm => sm.CreatedAt);

                e.HasOne(sm => sm.Organization)
                    .WithMany()
                    .HasForeignKey(sm => sm.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.ProductVariant)
                    .WithMany()
                    .HasForeignKey(sm => sm.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.RelatedSalesOrder)
                    .WithMany()
                    .HasForeignKey(sm => sm.RelatedSalesOrderId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(sm => sm.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.PostedByUser)
                    .WithMany()
                    .HasForeignKey(sm => sm.PostedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.RejectedByUser)
                    .WithMany()
                    .HasForeignKey(sm => sm.RejectedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(sm => sm.SourceLocation)
                    .WithMany()
                    .HasForeignKey(sm => sm.SourceLocationId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(sm => sm.DestinationLocation)
                    .WithMany()
                    .HasForeignKey(sm => sm.DestinationLocationId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- BLOQUE 3.1: AUDITORÍA DE INVENTARIO ---

            modelBuilder.Entity<InventoryAudit>(e =>
            {
                e.ToTable("InventoryAudits");
                e.HasKey(ia => ia.Id);
                e.Property(ia => ia.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(ia => ia.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(ia => ia.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
                e.Property(ia => ia.Description).HasColumnName("description").HasMaxLength(500);
                e.Property(ia => ia.Status).HasColumnName("status").HasConversion<string>().IsRequired().HasDefaultValue(InventoryAuditStatus.Draft);
                e.Property(ia => ia.SnapshotTakenAt).HasColumnName("snapshot_taken_at").IsRequired();
                e.Property(ia => ia.StartedAt).HasColumnName("started_at");
                e.Property(ia => ia.CompletedAt).HasColumnName("completed_at");
                e.Property(ia => ia.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
                e.Property(ia => ia.CompletedByUserId).HasColumnName("completed_by_user_id");
                e.Property(ia => ia.TotalVariants).HasColumnName("total_variants").IsRequired().HasDefaultValue(0);
                e.Property(ia => ia.CountedVariants).HasColumnName("counted_variants").IsRequired().HasDefaultValue(0);
                e.Property(ia => ia.TotalVariance).HasColumnName("total_variance").IsRequired().HasDefaultValue(0);
                e.Property(ia => ia.TotalVarianceValue).HasColumnName("total_variance_value").HasColumnType("decimal(10, 2)").HasDefaultValue(0.00m);
                e.Property(ia => ia.Notes).HasColumnName("notes").HasMaxLength(1000);
                e.Property(ia => ia.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(ia => ia.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(ia => ia.OrganizationId);
                e.HasIndex(ia => ia.Status);

                e.HasOne(ia => ia.Organization)
                    .WithMany()
                    .HasForeignKey(ia => ia.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(ia => ia.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(ia => ia.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(ia => ia.CompletedByUser)
                    .WithMany()
                    .HasForeignKey(ia => ia.CompletedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<InventoryAuditItem>(e =>
            {
                e.ToTable("InventoryAuditItems");
                e.HasKey(iai => iai.Id);
                e.Property(iai => iai.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(iai => iai.InventoryAuditId).HasColumnName("inventory_audit_id").IsRequired();
                e.Property(iai => iai.ProductVariantId).HasColumnName("product_variant_id").IsRequired();
                e.Property(iai => iai.TheoreticalStock).HasColumnName("theoretical_stock").IsRequired();
                e.Property(iai => iai.CountedStock).HasColumnName("counted_stock");
                e.Property(iai => iai.Variance).HasColumnName("variance");
                e.Property(iai => iai.VarianceValue).HasColumnName("variance_value").HasColumnType("decimal(10, 2)");
                e.Property(iai => iai.SnapshotAverageCost).HasColumnName("snapshot_average_cost").HasColumnType("decimal(10, 2)").IsRequired();
                e.Property(iai => iai.CountedByUserId).HasColumnName("counted_by_user_id");
                e.Property(iai => iai.CountedAt).HasColumnName("counted_at");
                e.Property(iai => iai.AdjustmentMovementId).HasColumnName("adjustment_movement_id");
                e.Property(iai => iai.Notes).HasColumnName("notes").HasMaxLength(500);
                e.Property(iai => iai.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(iai => iai.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(iai => iai.InventoryAuditId);
                e.HasIndex(iai => iai.ProductVariantId);
                e.HasIndex(iai => new { iai.InventoryAuditId, iai.ProductVariantId }).IsUnique();

                e.HasOne(iai => iai.InventoryAudit)
                    .WithMany(ia => ia.Items)
                    .HasForeignKey(iai => iai.InventoryAuditId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(iai => iai.ProductVariant)
                    .WithMany()
                    .HasForeignKey(iai => iai.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(iai => iai.CountedByUser)
                    .WithMany()
                    .HasForeignKey(iai => iai.CountedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(iai => iai.AdjustmentMovement)
                    .WithMany()
                    .HasForeignKey(iai => iai.AdjustmentMovementId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- BLOQUE 4: VENTAS ---

            modelBuilder.Entity<SalesOrder>(e =>
            {
                e.ToTable("SalesOrders");
                e.HasKey(so => so.Id);
                e.Property(so => so.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(so => so.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(so => so.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(so => so.CreatedByUserId).HasColumnName("created_by_user_id");
                e.Property(so => so.Status).HasColumnName("status").HasConversion<string>().IsRequired().HasDefaultValue(OrderStatus.Draft);
                e.Property(so => so.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10, 2)").IsRequired().HasDefaultValue(0.00m);
                e.Property(so => so.SubtotalAmount).HasColumnName("subtotal_amount").HasColumnType("decimal(10, 2)").HasDefaultValue(0.00m);
                e.Property(so => so.TotalTaxAmount).HasColumnName("total_tax_amount").HasColumnType("decimal(10, 2)").HasDefaultValue(0.00m);
                e.Property(so => so.Notes).HasColumnName("notes");
                e.Property(so => so.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(so => so.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasOne(so => so.Organization)
                    .WithMany(o => o.SalesOrders)
                    .HasForeignKey(so => so.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(so => so.Customer)
                    .WithMany(c => c.SalesOrders)
                    .HasForeignKey(so => so.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(so => so.CreatedByUser)
                    .WithMany(u => u.CreatedSalesOrders)
                    .HasForeignKey(so => so.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull); // Como en el SQL

                e.HasMany(so => so.WalletTransactions)
                    .WithOne(wt => wt.RelatedSalesOrder)
                    .HasForeignKey(wt => wt.RelatedSalesOrderId)
                    .OnDelete(DeleteBehavior.SetNull); // Si se borra la orden, la transacción de billetera queda (para auditoría)
            });

            modelBuilder.Entity<SalesOrderItem>(e =>
            {
                e.ToTable("SalesOrderItems");
                e.HasKey(oi => oi.Id);
                e.Property(oi => oi.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(oi => oi.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(oi => oi.SalesOrderId).HasColumnName("sales_order_id").IsRequired();
                e.Property(oi => oi.ProductVariantId).HasColumnName("product_variant_id").IsRequired();
                e.Property(oi => oi.Quantity).HasColumnName("quantity").IsRequired().HasDefaultValue(1);
                e.Property(oi => oi.OriginalPrice).HasColumnName("original_price").HasColumnType("decimal(10, 2)").IsRequired();
                e.Property(oi => oi.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10, 2)").IsRequired();
                e.Property(oi => oi.ItemDescription).HasColumnName("item_description");
                e.Property(oi => oi.UnitCost).HasColumnName("unit_cost");
                e.Property(oi => oi.TaxRateId).HasColumnName("tax_rate_id");
                e.Property(oi => oi.TaxRate).HasColumnName("tax_rate").HasColumnType("decimal(5, 4)").HasDefaultValue(0.00m);
                e.Property(oi => oi.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(10, 2)").HasDefaultValue(0.00m);
                e.Property(oi => oi.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10, 2)").HasDefaultValue(0.00m);
                e.Property(oi => oi.Total).HasColumnName("total").HasColumnType("decimal(10, 2)").HasDefaultValue(0.00m);

                e.HasOne(oi => oi.Organization)
                    .WithMany()
                    .HasForeignKey(oi => oi.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(oi => oi.SalesOrder)
                    .WithMany(so => so.Items)
                    .HasForeignKey(oi => oi.SalesOrderId)
                    .OnDelete(DeleteBehavior.Cascade); // Como en el SQL

                e.HasOne(oi => oi.ProductVariant)
                    .WithMany(pv => pv.SalesOrderItems)
                    .HasForeignKey(oi => oi.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(oi => oi.TaxRateEntity)
                    .WithMany()
                    .HasForeignKey(oi => oi.TaxRateId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- BLOQUE 5: AUDITORÍA ---

            modelBuilder.Entity<AuditLog>(e =>
            {
                e.ToTable("AuditLog");
                e.HasKey(al => al.Id);
                e.Property(al => al.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(al => al.OrganizationId).HasColumnName("organization_id");
                e.Property(al => al.UserId).HasColumnName("user_id");
                e.Property(al => al.ActionType).HasColumnName("action_type").HasConversion<string>().IsRequired();
                e.Property(al => al.TargetTable).HasColumnName("target_table").IsRequired();
                e.Property(al => al.TargetRecordId).HasColumnName("target_record_id");
                e.Property(al => al.Changes).HasColumnName("changes").HasColumnType("jsonb");
                e.Property(al => al.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");


                e.HasIndex(al => al.OrganizationId);
                e.HasIndex(al => al.UserId);
                e.HasIndex(al => new { al.TargetTable, al.TargetRecordId });

                e.HasOne(al => al.Organization)
                    .WithMany(o => o.AuditLogs)
                    .HasForeignKey(al => al.OrganizationId)
                    .OnDelete(DeleteBehavior.SetNull); // Como en el SQL

                e.HasOne(al => al.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.SetNull); // Como en el SQL
            });

            // --- BLOQUE 6: NOTIFICACIONES ---

            modelBuilder.Entity<Notification>(e =>
            {
                e.HasKey(al => al.Id);

                e.Property(al => al.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(al => al.UserId).HasColumnName("user_id").IsRequired();
                e.Property(al => al.Title).HasColumnName("title").IsRequired();
                e.Property(al => al.Message).HasColumnName("message").IsRequired();
                e.Property(al => al.Type).HasColumnName("type").HasConversion<string>().IsRequired();
                e.Property(al => al.IsRead).HasColumnName("is_read").IsRequired().HasDefaultValue(false);
                e.Property(al => al.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(al => al.UserId);

                e.HasOne(al => al.User)
                    .WithMany(u => u.Notificacions)
                    .HasForeignKey(al => al.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // --- BLOQUE 7: IMPUESTOS ---

            modelBuilder.Entity<TaxRate>(e =>
            {
                e.ToTable("tax_rates");
                e.HasKey(tr => tr.Id);
                e.Property(tr => tr.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(tr => tr.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(tr => tr.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
                e.Property(tr => tr.Rate).HasColumnName("rate").HasColumnType("decimal(5, 4)").IsRequired();
                e.Property(tr => tr.IsDefault).HasColumnName("is_default").HasDefaultValue(false);
                e.Property(tr => tr.IsActive).HasColumnName("is_active").HasDefaultValue(true);
                e.Property(tr => tr.Description).HasColumnName("description").HasMaxLength(500);
                e.Property(tr => tr.EffectiveFrom).HasColumnName("effective_from");
                e.Property(tr => tr.EffectiveTo).HasColumnName("effective_to");
                e.Property(tr => tr.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(tr => tr.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(tr => tr.OrganizationId);
                e.HasIndex(tr => new { tr.OrganizationId, tr.IsDefault });

                e.HasOne(tr => tr.Organization)
                    .WithMany(o => o.TaxRates)
                    .HasForeignKey(tr => tr.OrganizationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // --- BLOQUE 8: CONTABILIDAD ---

            modelBuilder.Entity<ChartOfAccount>(e =>
            {
                e.ToTable("ChartOfAccounts");
                e.HasKey(ca => ca.Id);
                e.Property(ca => ca.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(ca => ca.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(ca => ca.AccountCode).HasColumnName("account_code").HasMaxLength(20).IsRequired();
                e.Property(ca => ca.AccountName).HasColumnName("account_name").HasMaxLength(255).IsRequired();
                e.Property(ca => ca.AccountType).HasColumnName("account_type").HasConversion<string>().IsRequired();
                e.Property(ca => ca.SystemAccountType).HasColumnName("system_account_type").HasConversion<string>();
                e.Property(ca => ca.Description).HasColumnName("description");
                e.Property(ca => ca.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
                e.Property(ca => ca.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(ca => ca.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(ca => new { ca.OrganizationId, ca.AccountCode }).IsUnique();
                e.HasIndex(ca => new { ca.OrganizationId, ca.AccountName }).IsUnique();
                e.HasIndex(ca => new { ca.OrganizationId, ca.SystemAccountType }).IsUnique().HasFilter("\"system_account_type\" IS NOT NULL");

                e.HasOne<Organization>()
                    .WithMany()
                    .HasForeignKey(ca => ca.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<JournalEntry>(e =>
            {
                e.ToTable("JournalEntries");
                e.HasKey(je => je.Id);
                e.Property(je => je.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(je => je.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(je => je.EntryDate).HasColumnName("entry_date").IsRequired();
                e.Property(je => je.Description).HasColumnName("description").HasMaxLength(255).IsRequired();
                e.Property(je => je.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(100);
                e.Property(je => je.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(je => je.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(je => je.OrganizationId);
                e.HasIndex(je => je.EntryDate);

                e.HasOne<Organization>()
                    .WithMany()
                    .HasForeignKey(je => je.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<JournalEntryLine>(e =>
            {
                e.ToTable("JournalEntryLines");
                e.HasKey(jel => jel.Id);
                e.Property(jel => jel.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(jel => jel.JournalEntryId).HasColumnName("journal_entry_id").IsRequired();
                e.Property(jel => jel.AccountId).HasColumnName("account_id").IsRequired();
                e.Property(jel => jel.DebitAmount).HasColumnName("debit").HasColumnType("decimal(18, 2)").IsRequired().HasDefaultValue(0.00m);
                e.Property(jel => jel.CreditAmount).HasColumnName("credit").HasColumnType("decimal(18, 2)").IsRequired().HasDefaultValue(0.00m);
                e.Property(jel => jel.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(jel => jel.JournalEntryId);
                e.HasIndex(jel => jel.AccountId);

                e.HasOne(jel => jel.JournalEntry)
                    .WithMany(je => je.JournalEntryLines)
                    .HasForeignKey(jel => jel.JournalEntryId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(jel => jel.Account)
                    .WithMany()
                    .HasForeignKey(jel => jel.AccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- BLOQUE 9: COMPRAS (PURCHASE ORDERS) ---

            modelBuilder.Entity<PurchaseOrder>(e =>
            {
                e.ToTable("PurchaseOrders");
                e.HasKey(po => po.Id);
                e.Property(po => po.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(po => po.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(po => po.PONumber).HasColumnName("po_number").HasMaxLength(50).IsRequired();
                e.Property(po => po.VendorId).HasColumnName("vendor_id").IsRequired();
                e.Property(po => po.OrderDate).HasColumnName("order_date").IsRequired();
                e.Property(po => po.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
                e.Property(po => po.Status).HasColumnName("status").HasConversion<string>().IsRequired();
                e.Property(po => po.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                e.Property(po => po.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                e.Property(po => po.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                e.Property(po => po.Currency).HasColumnName("currency").HasMaxLength(3).HasDefaultValue("MXN");
                e.Property(po => po.ExchangeRate).HasColumnName("exchange_rate").HasColumnType("decimal(18, 6)").HasDefaultValue(1.0m);
                e.Property(po => po.PaymentTermsId).HasColumnName("payment_terms_id");
                e.Property(po => po.Notes).HasColumnName("notes").HasMaxLength(2000);
                e.Property(po => po.CreatedBy).HasColumnName("created_by").IsRequired();
                e.Property(po => po.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(po => po.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(po => po.OrganizationId);
                e.HasIndex(po => po.VendorId);
                e.HasIndex(po => new { po.OrganizationId, po.PONumber }).IsUnique();

                e.HasOne(po => po.Organization)
                    .WithMany()
                    .HasForeignKey(po => po.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(po => po.Vendor)
                    .WithMany()
                    .HasForeignKey(po => po.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(po => po.PaymentTerms)
                    .WithMany()
                    .HasForeignKey(po => po.PaymentTermsId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(po => po.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(po => po.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PurchaseOrderItem>(e =>
            {
                e.ToTable("PurchaseOrderItems");
                e.HasKey(poi => poi.Id);
                e.Property(poi => poi.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(poi => poi.PurchaseOrderId).HasColumnName("purchase_order_id").IsRequired();
                e.Property(poi => poi.LineNumber).HasColumnName("line_number").IsRequired();
                e.Property(poi => poi.ProductId).HasColumnName("product_id").IsRequired();
                e.Property(poi => poi.ProductVariantId).HasColumnName("product_variant_id");
                e.Property(poi => poi.Description).HasColumnName("description").HasMaxLength(500);
                e.Property(poi => poi.Quantity).HasColumnName("quantity").IsRequired();
                e.Property(poi => poi.UnitCost).HasColumnName("unit_cost").HasColumnType("decimal(18, 2)").IsRequired();
                e.Property(poi => poi.DiscountPercentage).HasColumnName("discount_percentage").HasColumnType("decimal(5, 2)").HasDefaultValue(0);
                e.Property(poi => poi.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                e.Property(poi => poi.TaxRate).HasColumnName("tax_rate").HasColumnType("decimal(5, 2)").HasDefaultValue(0);
                e.Property(poi => poi.TaxAmount).HasColumnName("tax_amount").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                e.Property(poi => poi.LineTotal).HasColumnName("line_total").HasColumnType("decimal(18, 2)").HasDefaultValue(0);
                e.Property(poi => poi.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(poi => poi.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(poi => poi.PurchaseOrderId);
                e.HasIndex(poi => new { poi.PurchaseOrderId, poi.LineNumber }).IsUnique();

                e.HasOne(poi => poi.PurchaseOrder)
                    .WithMany(po => po.Items)
                    .HasForeignKey(poi => poi.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(poi => poi.Product)
                    .WithMany()
                    .HasForeignKey(poi => poi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(poi => poi.ProductVariant)
                    .WithMany()
                    .HasForeignKey(poi => poi.ProductVariantId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // --- CONFIGURACIÓN MÓDULO DE COMPRAS ---

            // PurchaseOrder - Configurar la FK para CreatedBy -> CreatedByUserId
            modelBuilder.Entity<PurchaseOrder>(e =>
            {
                e.HasOne(po => po.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(po => po.CreatedBy)
                    .HasConstraintName("FK_PurchaseOrders_Users_CreatedByUserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PurchaseReceipt - Configurar la FK para ReceivedBy -> ReceivedByUserId
            modelBuilder.Entity<PurchaseReceipt>(e =>
            {
                e.HasOne(pr => pr.ReceivedByUser)
                    .WithMany()
                    .HasForeignKey(pr => pr.ReceivedBy)
                    .HasConstraintName("FK_PurchaseReceipts_Users_ReceivedByUserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CompanyBankAccount - Configurar relación con ChartOfAccount
            modelBuilder.Entity<CompanyBankAccount>(e =>
            {
                e.ToTable("company_bank_accounts");
                e.Property(cba => cba.Id).HasColumnName("id");
                e.Property(cba => cba.OrganizationId).HasColumnName("organization_id");
                e.Property(cba => cba.BankName).HasColumnName("bank_name");
                e.Property(cba => cba.AccountNumber).HasColumnName("account_number");
                e.Property(cba => cba.Currency).HasColumnName("currency");
                e.Property(cba => cba.CurrentBalance).HasColumnName("current_balance");
                e.Property(cba => cba.GLAccountId).HasColumnName("gl_account_id");
                e.Property(cba => cba.IsActive).HasColumnName("is_active");
                e.Property(cba => cba.CreatedAt).HasColumnName("created_at");
                e.Property(cba => cba.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(cba => cba.GLAccount)
                    .WithMany()
                    .HasForeignKey(cba => cba.GLAccountId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // VendorBankAccount - Configurar relación con Vendor
            modelBuilder.Entity<VendorBankAccount>(e =>
            {
                e.ToTable("vendor_bank_accounts");
                e.Property(vba => vba.Id).HasColumnName("id");
                e.Property(vba => vba.VendorId).HasColumnName("vendor_id");
                e.Property(vba => vba.BankName).HasColumnName("bank_name");
                e.Property(vba => vba.AccountNumber).HasColumnName("account_number");
                e.Property(vba => vba.AccountHolderName).HasColumnName("account_holder_name");
                e.Property(vba => vba.CLABE_IBAN).HasColumnName("clabe_iban");
                e.Property(vba => vba.AccountType).HasColumnName("account_type");
                e.Property(vba => vba.IsPrimary).HasColumnName("is_primary");
                e.Property(vba => vba.IsActive).HasColumnName("is_active");
                e.Property(vba => vba.CreatedAt).HasColumnName("created_at");
                e.Property(vba => vba.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(vba => vba.Vendor)
                    .WithMany(v => v.BankAccounts)
                    .HasForeignKey(vba => vba.VendorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Payment - Configurar relaciones
            modelBuilder.Entity<Payment>(e =>
            {
                e.ToTable("payments");
                e.Property(p => p.Id).HasColumnName("id");
                e.Property(p => p.OrganizationId).HasColumnName("organization_id");
                e.Property(p => p.PaymentNumber).HasColumnName("payment_number");
                e.Property(p => p.PaymentDate).HasColumnName("payment_date");
                e.Property(p => p.VendorId).HasColumnName("vendor_id");
                e.Property(p => p.PaymentMethod).HasColumnName("payment_method").HasConversion<string>();
                e.Property(p => p.CompanyBankAccountId).HasColumnName("company_bank_account_id");
                e.Property(p => p.VendorBankAccountId).HasColumnName("vendor_bank_account_id");
                e.Property(p => p.AmountPaid).HasColumnName("amount_paid");
                e.Property(p => p.Currency).HasColumnName("currency");
                e.Property(p => p.ExchangeRate).HasColumnName("exchange_rate");
                e.Property(p => p.ReferenceNumber).HasColumnName("reference_number");
                e.Property(p => p.Notes).HasColumnName("notes");
                e.Property(p => p.Status).HasColumnName("status").HasConversion<string>();
                e.Property(p => p.PaymentJournalEntryId).HasColumnName("payment_journal_entry_id");
                e.Property(p => p.CreatedBy).HasColumnName("created_by_user_id");
                e.Property(p => p.CreatedAt).HasColumnName("created_at");
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at");

                e.HasOne(p => p.Vendor)
                    .WithMany()
                    .HasForeignKey(p => p.VendorId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.CompanyBankAccount)
                    .WithMany()
                    .HasForeignKey(p => p.CompanyBankAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.VendorBankAccount)
                    .WithMany()
                    .HasForeignKey(p => p.VendorBankAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(p => p.CreatedBy)
                    .HasConstraintName("FK_Payments_Users_CreatedByUserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PaymentApplication - Configurar relaciones
            modelBuilder.Entity<PaymentApplication>(e =>
            {
                e.ToTable("payment_applications");
                e.Property(pa => pa.Id).HasColumnName("id");
                e.Property(pa => pa.PaymentId).HasColumnName("payment_id");
                e.Property(pa => pa.VendorInvoiceId).HasColumnName("vendor_invoice_id");
                e.Property(pa => pa.AmountApplied).HasColumnName("amount_applied");
                e.Property(pa => pa.CreatedAt).HasColumnName("created_at");

                e.HasOne(pa => pa.Payment)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(pa => pa.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(pa => pa.VendorInvoice)
                    .WithMany()
                    .HasForeignKey(pa => pa.VendorInvoiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SerieNo
            modelBuilder.Entity<NoSerie>(e =>
            {
                e.ToTable("no_series");
                e.Property(n => n.Id).HasColumnName("id").IsRequired();
                e.Property(n => n.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(n => n.Code).HasColumnName("code").IsRequired();
                e.Property(n => n.Description).HasColumnName("description").IsRequired();
                e.Property(n => n.DefaultNos).HasColumnName("default_nos").IsRequired();
                e.Property(n => n.ManualNos).HasColumnName("manual_nos").IsRequired();
                e.Property(n => n.DateOrder).HasColumnName("date_order").IsRequired();
                e.Property(n => n.DocumentType).HasColumnName("document_type").IsRequired();
                e.Property(n => n.CreatedAt).HasColumnName("created_at").IsRequired();
                e.Property(n => n.UpdatedAt).HasColumnName("updated_at").IsRequired();
            });

            modelBuilder.Entity<NoSerieLine>(e =>
            {
                e.ToTable("no_serie_lines");
                e.Property(n => n.Id).HasColumnName("id").IsRequired();
                e.Property(n => n.StartingDate).HasColumnName("starting_date").IsRequired();
                e.Property(n => n.StartingNo).HasColumnName("starting_no").IsRequired();
                e.Property(n => n.EndingNo).HasColumnName("ending_no").IsRequired();
                e.Property(n => n.LastNoUsed).HasColumnName("last_no_used").IsRequired();
                e.Property(n => n.IncrementBy).HasColumnName("manual_nos").IsRequired().HasDefaultValue(1);
                e.Property(n => n.WarningNo).HasColumnName("warning_no");
                e.Property(n => n.Open).HasColumnName("open");

                e.HasOne(n => n.NoSerie)
                    .WithMany(nl => nl.NoSerieLines)
                    .HasForeignKey(n => n.NoSerieId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
