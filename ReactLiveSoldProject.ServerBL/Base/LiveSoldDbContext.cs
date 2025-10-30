using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Models.Audit;
using ReactLiveSoldProject.ServerBL.Models.Authentication;
using ReactLiveSoldProject.ServerBL.Models.CustomerWallet;
using ReactLiveSoldProject.ServerBL.Models.Inventory;
using ReactLiveSoldProject.ServerBL.Models.Sales;

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

        // BLOQUE 2: CLIENTES Y BILLETERA
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        // BLOQUE 3: INVENTARIO
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }

        // BLOQUE 4: VENTAS
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderItem> SalesOrderItems { get; set; }

        // BLOQUE 5: AUDITORÍA
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Configuración para usar snake_case en la base de datos
            // (Opcional, pero recomendado si vienes de PostgreSQL)
            // foreach (var entity in modelBuilder.Model.GetEntityTypes())
            // {
            //     entity.SetTableName(entity.GetTableName().ToSnakeCase());
            //     foreach (var property in entity.GetProperties())
            //     {
            //         property.SetColumnName(property.GetColumnName().ToSnakeCase());
            //     }
            // }

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
                e.Property(o => o.PlanType).HasColumnName("plan_type").HasConversion<string>().IsRequired().HasDefaultValue(PlanType.Standard);
                e.Property(o => o.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
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

            // --- BLOQUE 2: CLIENTES Y BILLETERA ---

            modelBuilder.Entity<Customer>(e =>
            {
                e.ToTable("Customers");
                e.HasKey(c => c.Id);
                e.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(c => c.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(c => c.FirstName).HasColumnName("first_name");
                e.Property(c => c.LastName).HasColumnName("last_name");
                e.Property(c => c.Email).HasColumnName("email").IsRequired();
                e.Property(c => c.Phone).HasColumnName("phone");
                e.Property(c => c.PasswordHash).HasColumnName("password_hash").IsRequired();
                e.Property(c => c.AssignedSellerId).HasColumnName("assigned_seller_id");
                e.Property(c => c.Notes).HasColumnName("notes");
                e.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasIndex(c => new { c.OrganizationId, c.Email }).IsUnique();
                e.HasIndex(c => new { c.OrganizationId, c.Phone }).IsUnique().HasFilter("\"phone\" IS NOT NULL");

                e.HasOne(c => c.Organization)
                    .WithMany(o => o.Customers)
                    .HasForeignKey(c => c.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL

                e.HasOne(c => c.AssignedSeller)
                    .WithMany(u => u.AssignedCustomers)
                    .HasForeignKey(c => c.AssignedSellerId)
                    .OnDelete(DeleteBehavior.SetNull); // Como en el SQL
            });

            modelBuilder.Entity<Wallet>(e =>
            {
                e.ToTable("Wallets");
                e.HasKey(w => w.Id);
                e.Property(w => w.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
                e.Property(w => w.OrganizationId).HasColumnName("organization_id").IsRequired();
                e.Property(w => w.CustomerId).HasColumnName("customer_id").IsRequired();
                e.Property(w => w.Balance).HasColumnName("balance").HasColumnType("decimal(10, 2)").IsRequired().HasDefaultValue(0.00m);
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
                e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(now() at time zone 'utc')");
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(now() at time zone 'utc')");

                e.HasOne(p => p.Organization)
                    .WithMany(o => o.Products)
                    .HasForeignKey(p => p.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict); // Como en el SQL
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
                e.Property(pv => pv.StockQuantity).HasColumnName("stock_quantity").IsRequired().HasDefaultValue(0);
                e.Property(pv => pv.Attributes).HasColumnName("attributes").HasColumnType("jsonb");
                e.Property(pv => pv.ImageUrl).HasColumnName("image_url");
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
        }
    }
}
