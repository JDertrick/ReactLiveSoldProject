# 📘 Implementación - LiveSold Platform

## 📋 Índice

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Arquitectura del Proyecto](#arquitectura-del-proyecto)
3. [Modelos de Datos](#modelos-de-datos)
4. [Enumeraciones](#enumeraciones)
5. [Validaciones Implementadas](#validaciones-implementadas)
6. [Configuración de Base de Datos](#configuración-de-base-de-datos)
7. [Análisis de Requerimientos](#análisis-de-requerimientos)
8. [Endpoints a Implementar](#endpoints-a-implementar)
9. [Sistema de Autenticación](#sistema-de-autenticación)
10. [Multi-Tenancy](#multi-tenancy)
11. [Próximos Pasos](#próximos-pasos)

---

## 📊 Resumen Ejecutivo

**Proyecto:** LiveSold Platform - Plataforma SaaS Multi-Tenant para gestión de inventarios y ventas en vivo
**Tecnología:** .NET 9 + Entity Framework Core + PostgreSQL
**Estado:** Modelos de datos completados al 100% ✅
**Última Actualización:** 2025-10-29

### ✅ Estado de Implementación

| Componente | Estado | Completado |
|------------|--------|------------|
| Modelos de Datos | ✅ Completo | 100% |
| Enumeraciones | ✅ Completo | 100% |
| Validaciones (Data Annotations) | ✅ Completo | 100% |
| DbContext (Fluent API) | ✅ Completo | 100% |
| Controladores | ⏳ Pendiente | 0% |
| Servicios | ⏳ Pendiente | 0% |
| DTOs | ⏳ Pendiente | 0% |
| Autenticación JWT | ⏳ Pendiente | 0% |
| Migraciones | ⏳ Pendiente | 0% |

---

## 🏗️ Arquitectura del Proyecto

### Estructura de Carpetas

```
ReactLiveSoldProject/
├── ReactLiveSoldProject.Server/          # API Controllers
│   ├── Controllers/                      # Controladores REST
│   ├── Program.cs                        # Configuración del servidor
│   └── appsettings.json                  # Configuración
│
├── ReactLiveSoldProject.ServerBL/        # Business Logic Layer
│   ├── Base/
│   │   ├── Enums.cs                      # ✅ Enumeraciones
│   │   └── LiveSoldDbContext.cs          # ✅ DbContext configurado
│   │
│   ├── Models/
│   │   ├── Authentication/               # ✅ User, Organization, OrganizationMember
│   │   ├── Audit/                        # ✅ AuditLog
│   │   ├── CustomerWallet/               # ✅ Customer, Wallet, WalletTransaction
│   │   ├── Inventory/                    # ✅ Product, ProductVariant, Tag, ProductTag
│   │   └── Sales/                        # ✅ SalesOrder, SalesOrderItem
│   │
│   ├── Services/                         # ⏳ Servicios (pendiente)
│   └── DTOs/                             # ⏳ Data Transfer Objects (pendiente)
│
└── reactlivesoldproject.client/          # Frontend React + TypeScript
```

### Principios de Diseño

✅ **Persistence Ignorance:** Modelos POCO limpios sin dependencias de EF
✅ **Fluent API:** Configuración de relaciones en DbContext
✅ **Multi-Tenant:** Aislamiento de datos por `OrganizationId`
✅ **Type-Safe Enums:** En lugar de strings hardcoded
✅ **Validación en Capas:** Data Annotations + DbContext constraints

---

## 📦 Modelos de Datos

### 1. BLOQUE: Autenticación y Multi-Tenancy

#### Organization
**Ubicación:** `Models/Authentication/Organization.cs`

```csharp
public class Organization
{
    public Guid Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; }

    [Required, MaxLength(100)]
    [RegularExpression(@"^[a-z0-9-]+$")]
    public string Slug { get; set; }  // ✅ NUEVO - Para rutas del portal

    [Url, MaxLength(500)]
    public string? LogoUrl { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string PrimaryContactEmail { get; set; }

    [Required]
    public PlanType PlanType { get; set; } = PlanType.Standard;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<OrganizationMember> Members { get; set; }
    public virtual ICollection<Customer> Customers { get; set; }
    public virtual ICollection<Product> Products { get; set; }
    public virtual ICollection<Tag> Tags { get; set; }
    public virtual ICollection<SalesOrder> SalesOrders { get; set; }
    public virtual ICollection<AuditLog> AuditLogs { get; set; }
}
```

**Índices:**
- ✅ `Slug` - Único (para búsquedas por URL del portal)

---

#### User
**Ubicación:** `Models/Authentication/User.cs`

```csharp
public class User
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public bool IsSuperAdmin { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<OrganizationMember> OrganizationLinks { get; set; }
    public virtual ICollection<Customer> AssignedCustomers { get; set; }
    public virtual ICollection<WalletTransaction> AuthorizedTransactions { get; set; }
    public virtual ICollection<SalesOrder> CreatedSalesOrders { get; set; }
    public virtual ICollection<AuditLog> AuditLogs { get; set; }
}
```

**Índices:**
- ✅ `Email` - Único

---

#### OrganizationMember
**Ubicación:** `Models/Authentication/OrganizationMember.cs`

```csharp
public class OrganizationMember
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required]
    public Guid UserId { get; set; }
    public virtual User User { get; set; }

    [Required]
    public UserRole Role { get; set; } = UserRole.Seller;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Índices:**
- ✅ `(OrganizationId, UserId)` - Único compuesto

---

### 2. BLOQUE: Clientes y Billetera

#### Customer
**Ubicación:** `Models/CustomerWallet/Customer.cs`

```csharp
public class Customer
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }  // ✅ REQUIRED - Para login del portal

    [Phone, MaxLength(20)]
    public string? Phone { get; set; }

    [Required]
    public string PasswordHash { get; set; }  // ✅ REQUIRED - Para login del portal

    public Guid? AssignedSellerId { get; set; }
    public virtual User? AssignedSeller { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<SalesOrder> SalesOrders { get; set; }
}
```

**Índices:**
- ✅ `(OrganizationId, Email)` - Único compuesto
- ✅ `(OrganizationId, Phone)` - Único compuesto (con filtro para NULLs)

**Notas:**
- Email y PasswordHash son **required** para soportar login del portal de clientes
- Relación 1-a-1 con Wallet

---

#### Wallet
**Ubicación:** `Models/CustomerWallet/Wallet.cs`

```csharp
public class Wallet
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Balance { get; set; } = 0.00m;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<WalletTransaction> Transactions { get; set; }
}
```

**Índices:**
- ✅ `CustomerId` - Único (relación 1-a-1)

---

#### WalletTransaction
**Ubicación:** `Models/CustomerWallet/WalletTransaction.cs`

```csharp
public class WalletTransaction
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required]
    public Guid WalletId { get; set; }
    public virtual Wallet Wallet { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public Guid? RelatedSalesOrderId { get; set; }
    public virtual SalesOrder? RelatedSalesOrder { get; set; }

    public Guid? AuthorizedByUserId { get; set; }
    public virtual User? AuthorizedByUser { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

### 3. BLOQUE: Inventario

#### Product
**Ubicación:** `Models/Inventory/Product.cs`

```csharp
public class Product
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required, MaxLength(255)]
    public string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public ProductType ProductType { get; set; } = ProductType.Simple;

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<ProductVariant> Variants { get; set; }
    public virtual ICollection<ProductTag> TagLinks { get; set; }
}
```

---

#### ProductVariant
**Ubicación:** `Models/Inventory/ProductVariant.cs`

```csharp
public class ProductVariant
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required]
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    [MaxLength(100)]
    public string? Sku { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; } = 0.00m;

    [Required]
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } = 0;

    public string? Attributes { get; set; }  // JSONB en BD

    [Url, MaxLength(500)]
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<SalesOrderItem> SalesOrderItems { get; set; }
}
```

**Índices:**
- ✅ `(OrganizationId, Sku)` - Único compuesto (con filtro para NULLs)

---

#### Tag
**Ubicación:** `Models/Inventory/Tag.cs`

```csharp
public class Tag
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    // Navegación M-a-M
    public virtual ICollection<ProductTag> ProductLinks { get; set; }
}
```

**Índices:**
- ✅ `(OrganizationId, Name)` - Único compuesto

---

#### ProductTag
**Ubicación:** `Models/Inventory/ProductTag.cs`

```csharp
public class ProductTag
{
    // Clave primaria compuesta
    public Guid ProductId { get; set; }
    public virtual Product Product { get; set; }

    public Guid TagId { get; set; }
    public virtual Tag Tag { get; set; }
}
```

**Clave Primaria:**
- ✅ `(ProductId, TagId)` - Compuesta

---

### 4. BLOQUE: Ventas

#### SalesOrder
**Ubicación:** `Models/Sales/SalesOrder.cs`

```csharp
public class SalesOrder
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public virtual User? CreatedByUser { get; set; }

    [Required]
    public OrderStatus Status { get; set; } = OrderStatus.Draft;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; } = 0.00m;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public virtual ICollection<SalesOrderItem> Items { get; set; }
    public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
}
```

---

#### SalesOrderItem
**Ubicación:** `Models/Sales/SalesOrderItem.cs`

```csharp
public class SalesOrderItem
{
    public Guid Id { get; set; }

    [Required]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; }

    [Required]
    public Guid SalesOrderId { get; set; }
    public virtual SalesOrder SalesOrder { get; set; }

    [Required]
    public Guid ProductVariantId { get; set; }
    public virtual ProductVariant ProductVariant { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal OriginalPrice { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }  // Precio editable en venta LIVE

    [MaxLength(500)]
    public string? ItemDescription { get; set; }
}
```

**Notas:**
- `OriginalPrice`: Precio original del catálogo
- `UnitPrice`: Precio que se aplicará en la venta (puede ser diferente para ventas en vivo)

---

### 5. BLOQUE: Auditoría

#### AuditLog
**Ubicación:** `Models/Audit/AuditLog.cs`

```csharp
public class AuditLog
{
    public Guid Id { get; set; }

    public Guid? OrganizationId { get; set; }  // Nullable para acciones de SuperAdmin
    public virtual Organization? Organization { get; set; }

    public Guid? UserId { get; set; }  // Nullable para acciones del sistema
    public virtual User? User { get; set; }

    [Required]
    public AuditActionType ActionType { get; set; }

    [Required]
    public string TargetTable { get; set; }

    public Guid? TargetRecordId { get; set; }

    public string? Changes { get; set; }  // JSONB - Almacena before/after

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Índices:**
- ✅ `OrganizationId`
- ✅ `UserId`
- ✅ `(TargetTable, TargetRecordId)` - Compuesto

---

## 🔢 Enumeraciones

**Ubicación:** `Base/Enums.cs`

### UserRole
```csharp
public enum UserRole
{
    Seller,      // Vendedor - Gestiona clientes, ventas, transacciones
    Owner,       // Propietario - Todo lo de Seller + productos, tags, miembros
    SuperAdmin   // Super Admin - Gestiona organizaciones (sin acceso a datos)
}
```

### OrderStatus
```csharp
public enum OrderStatus
{
    Draft,       // Borrador - Orden en creación
    Completed,   // Completada - Orden finalizada y pagada
    Cancelled    // Cancelada
}
```

### TransactionType
```csharp
public enum TransactionType
{
    Deposit,     // Depósito - Agregar fondos
    Withdrawal   // Retiro - Restar fondos (usado en ventas)
}
```

### ProductType
```csharp
public enum ProductType
{
    Simple,      // Producto simple - Un solo SKU/precio
    Variable     // Producto variable - Múltiples variantes
}
```

### PlanType
```csharp
public enum PlanType
{
    Free,
    Standard,
    Premium,
    Enterprise
}
```

### AuditActionType
```csharp
public enum AuditActionType
{
    Create,
    Update,
    Delete
}
```

**Mapeo en Base de Datos:**
- ✅ Todos los enums se convierten a `string` en PostgreSQL
- ✅ Uso de `.HasConversion<string>()` en DbContext

---

## ✔️ Validaciones Implementadas

### Tipos de Validaciones

#### 1. Data Annotations (Nivel Aplicación)
Validan **antes** de llegar a la base de datos.

| Validación | Uso | Ejemplo |
|------------|-----|---------|
| `[Required]` | Campo obligatorio | Email, Name, etc. |
| `[EmailAddress]` | Formato de email válido | User.Email, Customer.Email |
| `[Phone]` | Formato de teléfono válido | Customer.Phone |
| `[Url]` | URL válida | Organization.LogoUrl |
| `[MaxLength(n)]` | Longitud máxima | Name(200), Email(255) |
| `[Range(min, max)]` | Rango numérico | Price(0, ∞), Quantity(1, ∞) |
| `[RegularExpression]` | Patrón específico | Organization.Slug |

#### 2. Fluent API (Nivel Base de Datos)
Restricciones en la base de datos (última línea de defensa).

```csharp
// Ejemplos en DbContext:
e.Property(o => o.Name).IsRequired();
e.HasIndex(u => u.Email).IsUnique();
e.Property(p => p.Price).HasColumnType("decimal(10, 2)");
```

### Validaciones Críticas Implementadas

#### Organization
- ✅ `Name`: Required, MaxLength(200)
- ✅ `Slug`: Required, MaxLength(100), Regex(`^[a-z0-9-]+$`), Único
- ✅ `PrimaryContactEmail`: Required, EmailAddress, MaxLength(255)
- ✅ `LogoUrl`: Url, MaxLength(500)

#### User
- ✅ `Email`: Required, EmailAddress, MaxLength(255), Único
- ✅ `PasswordHash`: Required

#### Customer
- ✅ `Email`: Required, EmailAddress, MaxLength(255), Único por organización
- ✅ `PasswordHash`: Required
- ✅ `Phone`: Phone, MaxLength(20), Único por organización (opcional)

#### Product & ProductVariant
- ✅ `Price`: Required, Range(0, ∞)
- ✅ `StockQuantity`: Required, Range(0, ∞)
- ✅ `Sku`: MaxLength(100), Único por organización

#### Wallet & WalletTransaction
- ✅ `Balance`: Required, Range(0, ∞)
- ✅ `Amount`: Required, Range(0.01, ∞)

#### SalesOrder & SalesOrderItem
- ✅ `Quantity`: Required, Range(1, ∞)
- ✅ `UnitPrice`: Required, Range(0, ∞)
- ✅ `TotalAmount`: Required, Range(0, ∞)

---

## 🗄️ Configuración de Base de Datos

### DbContext: LiveSoldDbContext
**Ubicación:** `Base/LiveSoldDbContext.cs`

### Convenciones

1. **Naming Convention:**
   - Tablas: PascalCase (e.g., `Organizations`)
   - Columnas: snake_case (e.g., `organization_id`)

2. **Tipos de Datos:**
   - Guids: `uuid` con `gen_random_uuid()`
   - Decimales: `decimal(10, 2)`
   - JSON: `jsonb` (PostgreSQL)
   - Timestamps: UTC con `(now() at time zone 'utc')`

3. **Delete Behaviors:**
   - `Cascade`: Para relaciones dependientes (Items → Order)
   - `Restrict`: Para relaciones de referencia (Customer → Organization)
   - `SetNull`: Para relaciones opcionales (Customer → AssignedSeller)

### Índices Importantes

```csharp
// Multi-Tenancy
e.HasIndex(o => o.Slug).IsUnique();
e.HasIndex(u => u.Email).IsUnique();
e.HasIndex(om => new { om.OrganizationId, om.UserId }).IsUnique();

// Performance
e.HasIndex(c => new { c.OrganizationId, c.Email }).IsUnique();
e.HasIndex(pv => new { pv.OrganizationId, pv.Sku }).IsUnique()
    .HasFilter("\"sku\" IS NOT NULL");

// Auditoría
e.HasIndex(al => al.OrganizationId);
e.HasIndex(al => al.UserId);
e.HasIndex(al => new { al.TargetTable, al.TargetRecordId });
```

---

## 📋 Análisis de Requerimientos

### Cambios Principales en el Prompt Actualizado

1. **Versión:** .NET 8 → .NET 9
2. **Portal de Cliente:** Nueva funcionalidad con rutas dinámicas por slug
3. **Autenticación Dual:** Empleados vs Clientes
4. **Endpoints Públicos:** Sin autenticación
5. **Nuevo Rol:** "Customer"

### Requerimientos Cumplidos ✅

| Requerimiento | Estado | Notas |
|---------------|--------|-------|
| Multi-Tenant por OrganizationId | ✅ | Todos los modelos tienen OrganizationId |
| Enums type-safe | ✅ | 6 enums implementados |
| User.IsSuperAdmin | ✅ | Para gestionar organizaciones |
| OrganizationMember.Role | ✅ | Seller, Owner, SuperAdmin |
| Customer.PasswordHash | ✅ | Para login del portal |
| Organization.Slug | ✅ | Para rutas dinámicas del portal |
| Wallet 1-a-1 con Customer | ✅ | Relación configurada |
| WalletTransaction.AuthorizedByUserId | ✅ | Para auditoría |
| SalesOrder.Status | ✅ | Enum OrderStatus |
| SalesOrderItem precios dinámicos | ✅ | OriginalPrice vs UnitPrice |
| Validaciones completas | ✅ | Data Annotations + Fluent API |
| Índices de performance | ✅ | Multi-tenant + búsquedas |

### Cambios Realizados en Esta Sesión

#### 1. Campo `Slug` en Organization
```csharp
[Required]
[MaxLength(100)]
[RegularExpression(@"^[a-z0-9-]+$")]
public string Slug { get; set; }
```
- ✅ Índice único agregado en DbContext
- ✅ Validación de formato (solo minúsculas, números, guiones)

#### 2. Customer.Email y PasswordHash Required
```csharp
[Required, EmailAddress, MaxLength(255)]
public string Email { get; set; }  // Era nullable

[Required]
public string PasswordHash { get; set; }  // Era nullable
```

#### 3. Índice de Phone con Filtro
```csharp
e.HasIndex(c => new { c.OrganizationId, c.Phone })
    .IsUnique()
    .HasFilter("\"phone\" IS NOT NULL");
```

---

## 🔐 Sistema de Autenticación

### Tipos de Tokens JWT

#### 1. Token de Empleado (User)
**Endpoint:** `POST /api/auth/employee-login`

**Claims:**
```csharp
{
    "sub": "user-guid",                    // UserId
    "email": "user@example.com",
    "organizationId": "org-guid",          // Si no es SuperAdmin
    "role": "Seller" | "Owner" | "SuperAdmin"
}
```

**Ejemplo de uso:**
```csharp
// Seller
claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
claims.Add(new Claim("OrganizationId", orgMember.OrganizationId.ToString()));
claims.Add(new Claim(ClaimTypes.Role, "Seller"));

// SuperAdmin (sin OrganizationId)
claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
```

---

#### 2. Token de Cliente (Customer)
**Endpoint:** `POST /api/portal/login`

**Claims:**
```csharp
{
    "sub": "customer-guid",                // CustomerId
    "email": "customer@example.com",
    "organizationId": "org-guid",          // CRÍTICO
    "role": "Customer"
}
```

**Ejemplo de uso:**
```csharp
claims.Add(new Claim("CustomerId", customer.Id.ToString()));
claims.Add(new Claim("OrganizationId", customer.OrganizationId.ToString()));
claims.Add(new Claim(ClaimTypes.Role, "Customer"));
```

---

### Políticas de Autorización

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("OrgOwner", policy =>
        policy.RequireRole("Owner"));

    options.AddPolicy("Seller", policy =>
        policy.RequireRole("Seller", "Owner"));

    options.AddPolicy("Customer", policy =>
        policy.RequireRole("Customer"));
});
```

---

## 📡 Endpoints a Implementar

### 1. Módulo de Autenticación (AuthController)

#### POST /api/auth/employee-login
**Autenticación:** No
**Rol:** Público
**Request:**
```json
{
    "email": "seller@example.com",
    "password": "password123"
}
```
**Response:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "user": {
        "id": "guid",
        "email": "seller@example.com",
        "role": "Seller",
        "organizationId": "guid"
    }
}
```

---

#### GET /api/auth/me
**Autenticación:** Sí
**Rol:** Cualquier usuario autenticado
**Response:**
```json
{
    "id": "guid",
    "email": "seller@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "Seller",
    "organizationId": "guid"
}
```

---

### 2. Módulo de SuperAdmin (SuperAdminController)

#### GET /api/superadmin/organizations
**Autenticación:** Sí
**Rol:** SuperAdmin
**Response:**
```json
[
    {
        "id": "guid",
        "name": "Tienda de Juan",
        "slug": "tienda-de-juan",
        "planType": "Standard",
        "isActive": true,
        "createdAt": "2025-01-15T10:30:00Z"
    }
]
```

---

#### POST /api/superadmin/organizations
**Autenticación:** Sí
**Rol:** SuperAdmin
**Request:**
```json
{
    "name": "Tienda de Juan",
    "slug": "tienda-de-juan",  // Auto-generado si no se proporciona
    "primaryContactEmail": "juan@tienda.com",
    "planType": "Standard"
}
```

**Lógica:**
1. Validar que el slug sea único
2. Si no se proporciona slug, generarlo desde el nombre
3. Crear organización

---

#### PUT /api/superadmin/organizations/{id}
**Autenticación:** Sí
**Rol:** SuperAdmin

---

#### DELETE /api/superadmin/organizations/{id}
**Autenticación:** Sí
**Rol:** SuperAdmin

---

### 3. Módulo Público (PublicController)

#### GET /api/public/organization-by-slug/{slug}
**Autenticación:** No
**Rol:** Público
**Response:**
```json
{
    "name": "Tienda de Juan",
    "logoUrl": "https://cdn.example.com/logo.png"
}
```

**⚠️ IMPORTANTE:**
- Solo devolver: `name`, `logoUrl`
- NUNCA devolver: `primaryContactEmail`, `planType`, `isActive`, etc.

---

### 4. Módulo de Portal de Cliente (CustomerPortalController)

#### POST /api/portal/login
**Autenticación:** No
**Rol:** Público
**Request:**
```json
{
    "email": "customer@example.com",
    "password": "password123",
    "organizationSlug": "tienda-de-juan"
}
```

**Lógica:**
1. Buscar `Organization` por `slug`
2. Si no existe, error 404
3. Buscar `Customer` por `email`
4. **CRÍTICO:** Validar `Customer.OrganizationId == Organization.Id`
5. Validar password
6. Generar JWT de Customer

**Response:**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "customer": {
        "id": "guid",
        "email": "customer@example.com",
        "firstName": "Maria",
        "lastName": "Lopez"
    }
}
```

---

#### GET /api/portal/my-wallet
**Autenticación:** Sí (Customer)
**Rol:** Customer
**Response:**
```json
{
    "balance": 150.50,
    "transactions": [
        {
            "id": "guid",
            "type": "Deposit",
            "amount": 200.00,
            "notes": "Recarga inicial",
            "createdAt": "2025-01-15T10:00:00Z"
        },
        {
            "id": "guid",
            "type": "Withdrawal",
            "amount": 49.50,
            "relatedSalesOrderId": "guid",
            "createdAt": "2025-01-15T11:30:00Z"
        }
    ]
}
```

**Lógica:**
1. Obtener `CustomerId` del JWT
2. Buscar `Wallet` por `CustomerId`
3. Filtrar por `OrganizationId` del JWT

---

#### GET /api/portal/my-orders
**Autenticación:** Sí (Customer)
**Rol:** Customer
**Response:**
```json
[
    {
        "id": "guid",
        "status": "Completed",
        "totalAmount": 49.50,
        "createdAt": "2025-01-15T11:00:00Z",
        "items": [
            {
                "productName": "Camisa Azul",
                "quantity": 1,
                "unitPrice": 49.50
            }
        ]
    }
]
```

---

### 5. Módulo de Inventario (ProductController)

#### GET /api/products
**Autenticación:** Sí
**Rol:** Seller, Owner
**Filtros:** `OrganizationId` del JWT

---

#### POST /api/products
**Autenticación:** Sí
**Rol:** Owner
**Request:**
```json
{
    "name": "Camisa Azul",
    "description": "Camisa de algodón",
    "productType": "Variable",
    "variants": [
        {
            "sku": "CAM-AZ-M",
            "price": 49.99,
            "stockQuantity": 10,
            "attributes": "{\"size\": \"M\", \"color\": \"azul\"}"
        }
    ]
}
```

---

### 6. Módulo de Clientes (CustomerController)

#### GET /api/customers
**Autenticación:** Sí
**Rol:** Seller, Owner
**Filtros:** `OrganizationId` del JWT

---

#### POST /api/customers
**Autenticación:** Sí
**Rol:** Seller, Owner
**Request:**
```json
{
    "email": "customer@example.com",
    "password": "password123",
    "firstName": "Maria",
    "lastName": "Lopez",
    "phone": "+1234567890"
}
```

**Lógica:**
1. Hash del password
2. Crear `Customer` con `OrganizationId` del JWT
3. Crear `Wallet` asociado con balance 0

---

### 7. Módulo de Billetera (WalletController)

#### POST /api/wallets/deposit
**Autenticación:** Sí
**Rol:** Seller, Owner
**Request:**
```json
{
    "customerId": "guid",
    "amount": 100.00,
    "notes": "Recarga de saldo"
}
```

**Lógica:**
1. Validar que `Customer` pertenezca a la `OrganizationId` del JWT
2. Buscar `Wallet` del customer
3. Crear `WalletTransaction` tipo `Deposit`
4. Actualizar `Wallet.Balance += Amount`
5. Setear `AuthorizedByUserId` = `UserId` del JWT

---

### 8. Módulo de Venta LIVE (SalesOrderController)

#### POST /api/salesorders
**Autenticación:** Sí
**Rol:** Seller, Owner
**Request:**
```json
{
    "customerId": "guid"
}
```

**Response:**
```json
{
    "id": "guid",
    "status": "Draft",
    "totalAmount": 0.00
}
```

---

#### POST /api/salesorders/{orderId}/items
**Autenticación:** Sí
**Rol:** Seller, Owner
**Request:**
```json
{
    "productVariantId": "guid",
    "quantity": 2,
    "unitPrice": 45.00  // Puede ser diferente del precio de lista
}
```

**Lógica:**
1. Validar que `SalesOrder` pertenezca a la `OrganizationId` del JWT
2. Validar que `SalesOrder.Status == Draft`
3. Obtener `ProductVariant.Price` como `OriginalPrice`
4. Crear `SalesOrderItem` con `UnitPrice` del request
5. Recalcular `SalesOrder.TotalAmount = SUM(Quantity * UnitPrice)`

---

#### DELETE /api/salesorders/{orderId}/items/{itemId}
**Autenticación:** Sí
**Rol:** Seller, Owner
**Lógica:**
1. Eliminar item
2. Recalcular `SalesOrder.TotalAmount`

---

#### POST /api/salesorders/{orderId}/finalize
**Autenticación:** Sí
**Rol:** Seller, Owner

**Lógica Crítica:**
1. Validar que `SalesOrder.Status == Draft`
2. Obtener `Wallet` del `Customer`
3. Verificar: `Wallet.Balance >= SalesOrder.TotalAmount`
4. **Si hay fondos:**
   - Cambiar `SalesOrder.Status = Completed`
   - Restar: `Wallet.Balance -= TotalAmount`
   - Crear `WalletTransaction` tipo `Withdrawal` vinculada a la orden
5. **Si no hay fondos:**
   - Error 400: "Saldo insuficiente"

---

## 🔒 Multi-Tenancy

### Estrategia de Aislamiento

**Enfoque:** Todos los datos filtrados por `OrganizationId`

### Reglas de Oro

1. ✅ **SIEMPRE** filtrar consultas por `OrganizationId` del JWT
2. ✅ **NUNCA** permitir acceso cruzado entre organizaciones
3. ✅ Validar que los recursos pertenecen a la organización del usuario

### Ejemplo de Implementación en Servicios

```csharp
public class ProductService
{
    public async Task<List<Product>> GetProductsAsync(Guid organizationId)
    {
        return await _dbContext.Products
            .Where(p => p.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid id, Guid organizationId)
    {
        return await _dbContext.Products
            .Where(p => p.Id == id && p.OrganizationId == organizationId)
            .FirstOrDefaultAsync();
    }
}
```

### Validación en Controladores

```csharp
[HttpGet("{id}")]
[Authorize(Policy = "Seller")]
public async Task<IActionResult> GetProduct(Guid id)
{
    var orgId = GetOrganizationIdFromToken();
    var product = await _productService.GetProductByIdAsync(id, orgId);

    if (product == null)
        return NotFound(); // O 404 si no existe, o 403 si no pertenece a la org

    return Ok(product);
}

private Guid GetOrganizationIdFromToken()
{
    var orgIdClaim = User.FindFirst("OrganizationId")?.Value;
    return Guid.Parse(orgIdClaim);
}
```

---

## 🚀 Próximos Pasos

### 1. Crear Migraciones
```bash
# En ReactLiveSoldProject.Server
dotnet ef migrations add InitialCreate --project ../ReactLiveSoldProject.ServerBL

# Aplicar a la BD
dotnet ef database update --project ../ReactLiveSoldProject.ServerBL
```

---

### 2. Implementar Servicios

Crear en `ReactLiveSoldProject.ServerBL/Services/`:
- `AuthService.cs`
- `OrganizationService.cs`
- `CustomerService.cs`
- `ProductService.cs`
- `WalletService.cs`
- `SalesOrderService.cs`
- `AuditLogService.cs`

---

### 3. Implementar DTOs

Crear en `ReactLiveSoldProject.ServerBL/DTOs/`:
- `LoginRequestDto.cs`
- `LoginResponseDto.cs`
- `OrganizationDto.cs`
- `CustomerDto.cs`
- `ProductDto.cs`
- etc.

**Ejemplo:**
```csharp
public class OrganizationPublicDto
{
    public string Name { get; set; }
    public string? LogoUrl { get; set; }
}
```

---

### 4. Implementar Controladores

Crear en `ReactLiveSoldProject.Server/Controllers/`:
- `AuthController.cs`
- `SuperAdminController.cs`
- `PublicController.cs`
- `CustomerPortalController.cs`
- `ProductController.cs`
- `CustomerController.cs`
- `WalletController.cs`
- `SalesOrderController.cs`

---

### 5. Configurar JWT en Program.cs

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
```

---

### 6. Helper para Generación de Slugs

```csharp
public static class SlugHelper
{
    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre no puede estar vacío");

        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-")
            // Remover acentos
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c))
            .ToString()
            .Trim('-');
    }

    public static async Task<string> EnsureUniqueSlugAsync(
        LiveSoldDbContext dbContext,
        string baseSlug)
    {
        var slug = baseSlug;
        var counter = 1;

        while (await dbContext.Organizations.AnyAsync(o => o.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
```

---

### 7. Implementar Auditoría Automática

Override de `SaveChangesAsync` en `LiveSoldDbContext`:

```csharp
public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
{
    var auditEntries = new List<AuditLog>();

    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.Entity is AuditLog || entry.State == EntityState.Unchanged)
            continue;

        var auditLog = new AuditLog
        {
            ActionType = entry.State switch
            {
                EntityState.Added => AuditActionType.Create,
                EntityState.Modified => AuditActionType.Update,
                EntityState.Deleted => AuditActionType.Delete,
                _ => throw new ArgumentOutOfRangeException()
            },
            TargetTable = entry.Entity.GetType().Name,
            // ... configurar TargetRecordId, Changes, etc.
        };

        auditEntries.Add(auditLog);
    }

    var result = await base.SaveChangesAsync(cancellationToken);

    if (auditEntries.Any())
    {
        await AuditLogs.AddRangeAsync(auditEntries, cancellationToken);
        await base.SaveChangesAsync(cancellationToken);
    }

    return result;
}
```

---

## 📚 Recursos Adicionales

### Documentación Oficial
- [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core JWT Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)

### Buenas Prácticas
- [REST API Best Practices](https://learn.microsoft.com/en-us/azure/architecture/best-practices/api-design)
- [Multi-Tenant Applications](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

## 📝 Notas Finales

### Modelo de Datos: 100% Completo ✅

Los modelos están listos para implementar TODOS los endpoints del prompt actualizado.

### Cambios Críticos Implementados

1. ✅ Campo `Slug` en `Organization` con validación y índice único
2. ✅ `Customer.Email` y `Customer.PasswordHash` como required
3. ✅ Enums type-safe en todos los modelos
4. ✅ Validaciones completas (Data Annotations + Fluent API)
5. ✅ Configuración multi-tenant robusta

### Lo que Falta

1. ⏳ Migraciones de base de datos
2. ⏳ Servicios de negocio
3. ⏳ DTOs
4. ⏳ Controladores
5. ⏳ Configuración JWT
6. ⏳ Políticas de autorización
7. ⏳ Tests unitarios e integración

---

**Autor:** Claude Code
**Versión:** 1.0
**Fecha:** 2025-10-29
**Proyecto:** LiveSold Platform - Multi-Tenant SaaS
