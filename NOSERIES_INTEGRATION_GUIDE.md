# Guía de Integración del Sistema de Series Numéricas

## Resumen General

Se ha integrado el sistema de series numéricas (NoSeries) en el proyecto para generar automáticamente números únicos y secuenciales para todas las entidades del sistema.

## Cambios Completados

### 1. Modelos Actualizados

Se agregaron campos de número a los siguientes modelos:

| Modelo | Campo Agregado | Ejemplo |
|--------|---------------|---------|
| **Customer** | `CustomerNo` | CUST-2025-0001 |
| **Vendor** | `VendorNo` | VEND-2025-0001 |
| **Contact** | `ContactNo` | CONT-0001 |
| **Product** | `ProductNo` | PROD-0001 |
| **ProductVariant** | `VariantNo` | VAR-0001 |
| **SalesOrder** | `OrderNo` | SO-2025-0001 |
| **StockMovement** | `MovementNo` | SM-2025-0001 |
| **WalletTransaction** | `TransactionNo` | WT-2025-0001 |

**Modelos que ya tenían campos de número:**
- PurchaseOrder: `PONumber`
- VendorInvoice: `InvoiceNumber`
- PurchaseReceipt: `ReceiptNumber`
- Payment: `PaymentNumber`
- JournalEntry: `EntryNumber`

### 2. Servicios Integrados

Los siguientes servicios ya están integrados con el sistema de series numéricas:

#### ✅ CustomerService
- Archivo: `ReactLiveSoldProject.ServerBL/Infrastructure/Services/CustomerService.cs`
- Inyectado `ISerieNoService`
- Genera `CustomerNo` en `CreateCustomerAsync` usando `DocumentType.Customer`

#### ✅ VendorService
- Archivo: `ReactLiveSoldProject.ServerBL/Infrastructure/Services/VendorService.cs`
- Inyectado `ISerieNoService`
- Genera `VendorNo` en `CreateVendorAsync` usando `DocumentType.Vendor`

#### ✅ ContactService
- Archivo: `ReactLiveSoldProject.ServerBL/Infrastructure/Services/ContactService.cs`
- Inyectado `ISerieNoService`
- Genera `ContactNo` en `CreateContactAsync` usando `DocumentType.Contact`

#### ✅ ProductService
- Archivo: `ReactLiveSoldProject.ServerBL/Infrastructure/Services/ProductService.cs`
- Inyectado `ISerieNoService`
- Genera `ProductNo` en `CreateProductAsync` usando `DocumentType.Product`

### 3. Series Numéricas por Defecto

Al crear una organización nueva, se crean automáticamente 14 series numéricas:

- CUST - Clientes
- VEND - Proveedores
- CONT - Contactos
- PROD - Productos
- VAR - Variantes de Producto
- SO - Órdenes de Venta
- PO - Órdenes de Compra
- PREC - Recepciones de Compra
- PINV - Facturas de Compra
- PAY - Pagos
- SINV - Facturas de Venta
- JE - Asientos Contables
- WT - Transacciones de Billetera
- SM - Movimientos de Inventario

## Servicios Pendientes de Integración

Los siguientes servicios **aún necesitan ser integrados** con el sistema de series numéricas:

### 🔄 ProductVariant (Si existe ProductVariantService)

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public ProductVariantService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreateVariantAsync (o similar):
var variantNo = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.ProductVariant);
variant.VariantNo = variantNo;
```

### 🔄 PurchaseOrderService

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public PurchaseOrderService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreatePurchaseOrderAsync:
var poNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.PurchaseOrder);
purchaseOrder.PONumber = poNumber;
```

### 🔄 PurchaseReceiptService

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public PurchaseReceiptService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreatePurchaseReceiptAsync:
var receiptNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.PurchaseReceipt);
receipt.ReceiptNumber = receiptNumber;
```

### 🔄 VendorInvoiceService

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public VendorInvoiceService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreateVendorInvoiceAsync:
var invoiceNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.PurchaseInvoice);
invoice.InvoiceNumber = invoiceNumber;
```

### 🔄 PaymentService

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public PaymentService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreatePaymentAsync:
var paymentNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.Payment);
payment.PaymentNumber = paymentNumber;
```

### 🔄 SalesOrderService (Si existe)

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public SalesOrderService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreateSalesOrderAsync:
var orderNo = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.SalesOrder);
salesOrder.OrderNo = orderNo;
```

### 🔄 AccountingService (Para JournalEntry)

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public AccountingService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreateJournalEntryAsync:
var entryNumber = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.JournalEntry);
journalEntry.EntryNumber = entryNumber;
```

### 🔄 WalletService (Para WalletTransaction)

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public WalletService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreateTransactionAsync o método similar:
var transactionNo = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.WalletTransaction);
transaction.TransactionNo = transactionNo;
```

### 🔄 StockMovementService (Si existe un servicio dedicado)

```csharp
// 1. Agregar using
using ReactLiveSoldProject.ServerBL.Models.Configuration;

// 2. Inyectar en constructor
private readonly ISerieNoService _serieNoService;

public StockMovementService(LiveSoldDbContext dbContext, ISerieNoService serieNoService)
{
    _dbContext = dbContext;
    _serieNoService = serieNoService;
}

// 3. En CreateStockMovementAsync:
var movementNo = await _serieNoService.GetNextNumberByTypeAsync(organizationId, DocumentType.StockMovement);
movement.MovementNo = movementNo;
```

## Pasos para Completar la Integración

### 1. Crear y Aplicar Migración

```bash
cd ReactLiveSoldProject.ServerBL
dotnet ef migrations add AddNumberFieldsToEntities --startup-project ../ReactLiveSoldProject.Server
dotnet ef database update --startup-project ../ReactLiveSoldProject.Server
```

Esta migración agregará todos los campos de número que se agregaron a los modelos.

### 2. Integrar Servicios Pendientes

Para cada servicio en la lista de "Servicios Pendientes", seguir el patrón de código proporcionado arriba.

### 3. Actualizar DTOs (Opcional)

Si deseas que los números se muestren en el frontend, agrega los campos correspondientes a los DTOs:

```csharp
// Ejemplo: CustomerDto.cs
public string? CustomerNo { get; set; }

// Ejemplo: VendorDto.cs
public string? VendorNo { get; set; }

// etc...
```

### 4. Actualizar Frontend

#### a. Agregar campos en las tablas de listado:

```tsx
// Ejemplo: Customers.tsx
<TableHead>Número</TableHead>

// En el mapeo:
<TableCell>{customer.customerNo}</TableCell>
```

#### b. Mostrar el número en formularios de creación/edición:

```tsx
// El número se genera automáticamente en el backend
// Puedes mostrar un campo de solo lectura después de crear:
{customer?.customerNo && (
  <div className="mb-4">
    <Label>Número de Cliente</Label>
    <Input value={customer.customerNo} disabled />
  </div>
)}
```

#### c. Agregar búsqueda por número:

```tsx
// En los servicios de búsqueda del backend, agregar búsqueda por número:
query = query.Where(c =>
    c.CustomerNo != null && c.CustomerNo.ToLower().Contains(searchTerm) ||
    c.Contact.Email.ToLower().Contains(searchTerm) ||
    ...
);
```

## Personalización de Series

Los usuarios pueden personalizar las series desde el frontend en `/app/no-series`:

1. **Modificar rangos**: Cambiar números de inicio y fin
2. **Agregar líneas nuevas**: Para nuevo año o cambio de formato
3. **Crear series adicionales**: Múltiples series para el mismo tipo de documento
4. **Habilitar numeración manual**: Permitir ingresar números personalizados
5. **Configurar alertas**: Definir cuándo alertar que se acerca el final del rango

## Ventajas de este Sistema

1. **Numeración Automática**: No más conflictos de números duplicados
2. **Trazabilidad**: Todos los documentos tienen un número único secuencial
3. **Auditoría**: Fácil identificar gaps o números faltantes
4. **Personalizable**: Los usuarios pueden configurar sus propias series
5. **Multi-año**: Soporte para cambiar formato por año
6. **Validación**: El sistema valida que los números sean correctos

## Notas Importantes

- ⚠️ **No eliminar series que ya tienen números asignados** - Podría romper referencias
- 💡 **Las series se asignan por tipo de documento** - Una organización puede tener múltiples series del mismo tipo
- 🔒 **Los números una vez asignados no se reutilizan** - Incluso si se elimina el registro
- 📅 **Series con año**: Se recomienda agregar nuevas líneas cada año
- 🎯 **Serie por defecto**: Solo puede haber una serie por defecto por tipo de documento

## Solución de Problemas

### Error: "No se encontró una serie numérica para..."

**Solución**: Asegúrate de que existe una serie activa y abierta para ese tipo de documento en esa organización.

```sql
-- Verificar series disponibles
SELECT * FROM NoSeries WHERE OrganizationId = 'guid' AND DocumentType = 1;
SELECT * FROM NoSerieLines WHERE Open = 1;
```

### Error: "Se alcanzó el final del rango"

**Solución**: Agregar una nueva línea de numeración con un rango más alto o extender el rango actual.

### Los números no se generan

**Verificar**:
1. ¿El servicio está inyectando `ISerieNoService`?
2. ¿El método está llamando a `GetNextNumberByTypeAsync`?
3. ¿Existen series configuradas para esa organización?
4. ¿La línea está marcada como `Open = true`?

## Próximos Pasos Recomendados

1. ✅ Aplicar la migración
2. ⬜ Integrar todos los servicios pendientes
3. ⬜ Actualizar los DTOs para incluir los números
4. ⬜ Actualizar el frontend para mostrar los números en tablas
5. ⬜ Agregar búsqueda por número en todos los listados
6. ⬜ Probar la creación de registros en desarrollo
7. ⬜ Documentar para el usuario final cómo usar el sistema de series
