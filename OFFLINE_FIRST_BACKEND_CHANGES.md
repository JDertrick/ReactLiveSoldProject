# Cambios Necesarios en Backend para Offline-First

## 1. Nuevos Endpoints de Replicación

### **PullController.cs** (nuevo)
```csharp
[ApiController]
[Route("api/replication")]
public class ReplicationController : ControllerBase
{
    private readonly LiveSoldDbContext _context;

    [HttpPost("pull/products")]
    public async Task<IActionResult> PullProducts([FromBody] PullRequest request)
    {
        // Obtener documentos modificados después del checkpoint
        var products = await _context.Products
            .Where(p => p.OrganizationId == request.OrgId)
            .Where(p => p.UpdatedAt > request.Checkpoint)
            .OrderBy(p => p.UpdatedAt)
            .Take(50)
            .ToListAsync();

        var newCheckpoint = products.Any()
            ? products.Max(p => p.UpdatedAt)
            : request.Checkpoint;

        return Ok(new PullResponse
        {
            Documents = products,
            Checkpoint = newCheckpoint
        });
    }

    [HttpPost("push/products")]
    public async Task<IActionResult> PushProducts([FromBody] PushRequest<Product> request)
    {
        var conflicts = new List<Conflict>();

        foreach (var doc in request.Documents)
        {
            var existing = await _context.Products.FindAsync(doc.Id);

            // Detectar conflictos (versión del servidor más reciente)
            if (existing != null && existing.UpdatedAt > doc.UpdatedAt)
            {
                conflicts.Add(new Conflict
                {
                    DocumentId = doc.Id,
                    ServerVersion = existing.UpdatedAt,
                    ClientVersion = doc.UpdatedAt
                });
                continue;
            }

            // Aplicar cambio
            if (existing == null)
            {
                _context.Products.Add(doc);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(doc);
                existing.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new PushResponse
        {
            Success = conflicts.Count == 0,
            Conflicts = conflicts
        });
    }

    // Repetir para: product_variants, customers, sales_orders, stock_movements, wallet_transactions
}

public class PullRequest
{
    public long Checkpoint { get; set; }
    public string OrgId { get; set; }
}

public class PullResponse
{
    public object[] Documents { get; set; }
    public long Checkpoint { get; set; }
}

public class PushRequest<T>
{
    public T[] Documents { get; set; }
}

public class PushResponse
{
    public bool Success { get; set; }
    public List<Conflict> Conflicts { get; set; }
}

public class Conflict
{
    public string DocumentId { get; set; }
    public long ServerVersion { get; set; }
    public long ClientVersion { get; set; }
}
```

## 2. Modificaciones en Modelos

Agregar campos de sincronización a TODOS los modelos:

```csharp
public abstract class SyncableEntity
{
    public string Id { get; set; } // Ya existe
    public long UpdatedAt { get; set; } // NUEVO - timestamp Unix
    public bool IsDeleted { get; set; } // NUEVO - soft delete
    public string LastModifiedBy { get; set; } // NUEVO - para auditoría
}

// Actualizar todos los modelos:
public class Product : SyncableEntity
{
    // ... campos existentes
}

public class SalesOrder : SyncableEntity
{
    // ... campos existentes
}
```

## 3. Migration para agregar campos

```bash
dotnet ef migrations add AddSyncFields --project ReactLiveSoldProject.ServerBL
```

```csharp
// Migration
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Agregar columnas a todas las tablas
    var tables = new[] { "Products", "ProductVariants", "Customers", "SalesOrders",
                         "StockMovements", "WalletTransactions", "TaxRates" };

    foreach (var table in tables)
    {
        migrationBuilder.AddColumn<long>(
            name: "UpdatedAt",
            table: table,
            type: "bigint",
            nullable: false,
            defaultValue: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: table,
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "LastModifiedBy",
            table: table,
            maxLength: 100,
            nullable: true);

        // Índice para mejorar queries de sync
        migrationBuilder.CreateIndex(
            name: $"IX_{table}_UpdatedAt",
            table: table,
            column: "UpdatedAt");
    }
}
```

## 4. Estrategia de Conflictos

### Reglas de Resolución:

1. **Last-Write-Wins (por defecto)**
   - El timestamp más reciente gana
   - Aplicable a: Products, Customers, TaxRates

2. **Server Authority (inventario)**
   - El servidor siempre tiene la verdad
   - StockMovements: validar stock disponible antes de aceptar

```csharp
[HttpPost("push/stock_movements")]
public async Task<IActionResult> PushStockMovements([FromBody] PushRequest<StockMovement> request)
{
    foreach (var movement in request.Documents)
    {
        // Validar que el stock no quede negativo
        var variant = await _context.ProductVariants.FindAsync(movement.ProductVariantId);
        var newStock = variant.CurrentStock + movement.Quantity;

        if (newStock < 0)
        {
            return BadRequest(new {
                Error = "Stock insuficiente",
                VariantId = movement.ProductVariantId,
                Available = variant.CurrentStock,
                Requested = Math.Abs(movement.Quantity)
            });
        }

        // Aplicar movimiento
        _context.StockMovements.Add(movement);
        variant.CurrentStock = newStock;
    }

    await _context.SaveChangesAsync();
    return Ok(new { Success = true });
}
```

3. **Custom Logic (wallets)**
   - Validar fondos disponibles
   - Evitar doble gasto

```csharp
[HttpPost("push/wallet_transactions")]
public async Task<IActionResult> PushWalletTransactions([FromBody] PushRequest<WalletTransaction> request)
{
    foreach (var tx in request.Documents)
    {
        var wallet = await _context.Wallets.FindAsync(tx.WalletId);

        // Validar balance para débitos
        if (tx.Amount < 0 && wallet.Balance + tx.Amount < 0)
        {
            return BadRequest(new {
                Error = "Fondos insuficientes",
                WalletId = tx.WalletId,
                Balance = wallet.Balance,
                Requested = Math.Abs(tx.Amount)
            });
        }

        // Idempotencia: verificar que la transacción no se haya aplicado ya
        var exists = await _context.WalletTransactions
            .AnyAsync(t => t.Id == tx.Id);

        if (!exists)
        {
            _context.WalletTransactions.Add(tx);
            wallet.Balance += tx.Amount;
        }
    }

    await _context.SaveChangesAsync();
    return Ok(new { Success = true });
}
```

## 5. WebSocket para Notificaciones en Tiempo Real (opcional)

```csharp
// Startup.cs
services.AddSignalR();

// ReplicationHub.cs
public class ReplicationHub : Hub
{
    public async Task SubscribeToOrganization(string orgId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, orgId);
    }

    public async Task NotifyChange(string orgId, string collection)
    {
        await Clients.Group(orgId).SendAsync("DataChanged", collection);
    }
}
```

## 6. Logging y Auditoría

```csharp
public class SyncAuditLog
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string OrganizationId { get; set; }
    public string Collection { get; set; }
    public string Operation { get; set; } // Pull/Push
    public int DocumentCount { get; set; }
    public int ConflictCount { get; set; }
    public DateTime Timestamp { get; set; }
    public string ErrorMessage { get; set; }
}

// Guardar en cada operación de sync
await _context.SyncAuditLogs.AddAsync(new SyncAuditLog
{
    Id = Guid.NewGuid().ToString(),
    UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
    OrganizationId = request.OrgId,
    Collection = "products",
    Operation = "Push",
    DocumentCount = request.Documents.Length,
    ConflictCount = conflicts.Count,
    Timestamp = DateTime.UtcNow
});
```

## 7. Endpoints Adicionales

```csharp
// Verificar estado de sincronización
[HttpGet("sync/status/{orgId}")]
public async Task<IActionResult> GetSyncStatus(string orgId)
{
    var lastSync = await _context.SyncAuditLogs
        .Where(s => s.OrganizationId == orgId)
        .OrderByDescending(s => s.Timestamp)
        .FirstOrDefaultAsync();

    return Ok(new
    {
        LastSync = lastSync?.Timestamp,
        PendingChanges = await GetPendingChangesCount(orgId)
    });
}

// Forzar resolución de conflicto (admin)
[HttpPost("sync/resolve-conflict")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> ResolveConflict([FromBody] ConflictResolution resolution)
{
    // Aplicar resolución manual
    // ...
}
```

## 8. Performance: Índices Adicionales

```sql
-- Mejorar queries de replicación
CREATE INDEX idx_products_org_updated ON Products(OrganizationId, UpdatedAt);
CREATE INDEX idx_sales_org_updated ON SalesOrders(OrganizationId, UpdatedAt);
CREATE INDEX idx_stock_org_updated ON StockMovements(OrganizationId, UpdatedAt);

-- Soft deletes
CREATE INDEX idx_products_deleted ON Products(IsDeleted) WHERE IsDeleted = false;
```

## Estimación de Esfuerzo

- Nuevos endpoints de replicación: **2 semanas**
- Migrations y cambios en modelos: **1 semana**
- Lógica de conflictos: **2 semanas**
- WebSocket (opcional): **1 semana**
- Testing y ajustes: **2 semanas**

**TOTAL BACKEND: 6-8 semanas**
