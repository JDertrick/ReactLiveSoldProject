# Sistema de Numeración Secuencial (NoSeries)

Sistema de numeración automática tipo Business Central para documentos y registros.

## Características

- ✅ Numeración automática para múltiples tipos de documentos
- ✅ Soporte para prefijos alfanuméricos (ej: INV-0001, CUST-0001)
- ✅ Múltiples líneas por serie con fechas diferentes
- ✅ Validación de rangos
- ✅ Detección de números agotados
- ✅ Serie por defecto por tipo de documento
- ✅ Opción de numeración manual

## Tipos de Documentos Soportados

```csharp
public enum DocumentType
{
    Customer = 1,           // Clientes
    Vendor = 2,             // Proveedores
    SalesInvoice = 3,       // Facturas de venta
    SalesOrder = 4,         // Órdenes de venta
    PurchaseInvoice = 5,    // Facturas de compra
    PurchaseOrder = 6,      // Órdenes de compra
    PurchaseReceipt = 7,    // Recibos de compra
    Product = 8,            // Productos
    ProductVariant = 9,     // Variantes de producto
    Payment = 10,           // Pagos
    JournalEntry = 11,      // Asientos contables
    WalletTransaction = 12, // Transacciones de wallet
    StockMovement = 13,     // Movimientos de inventario
    Contact = 14            // Contactos
}
```

## Ejemplos de Uso

### 1. Crear una Serie Numérica para Clientes

```http
POST /api/SerieNo
Authorization: Bearer {token}

{
  "code": "CUST",
  "description": "Numeración para Clientes",
  "documentType": 1,  // Customer
  "defaultNos": true,
  "manualNos": false,
  "dateOrder": false,
  "noSerieLines": [
    {
      "startingDate": "2025-01-01T00:00:00",
      "startingNo": "CUST-0001",
      "endingNo": "CUST-9999",
      "incrementBy": 1,
      "warningNo": "CUST-9900",
      "open": true
    }
  ]
}
```

### 2. Crear una Serie para Facturas con Múltiples Líneas

```http
POST /api/SerieNo

{
  "code": "INV",
  "description": "Facturas de Venta",
  "documentType": 3,  // SalesInvoice
  "defaultNos": true,
  "manualNos": false,
  "dateOrder": true,
  "noSerieLines": [
    {
      "startingDate": "2025-01-01T00:00:00",
      "startingNo": "INV-2025-0001",
      "endingNo": "INV-2025-9999",
      "incrementBy": 1,
      "open": true
    },
    {
      "startingDate": "2026-01-01T00:00:00",
      "startingNo": "INV-2026-0001",
      "endingNo": "INV-2026-9999",
      "incrementBy": 1,
      "open": false  // Se abrirá en 2026
    }
  ]
}
```

### 3. Obtener el Siguiente Número Disponible

**Por código de serie:**
```http
GET /api/SerieNo/next/CUST
```

**Respuesta:**
```json
{
  "serieCode": "CUST",
  "nextNumber": "CUST-0001",
  "date": "2025-12-14T10:30:00"
}
```

**Por tipo de documento (usa la serie por defecto):**
```http
GET /api/SerieNo/next/type/1  // 1 = Customer
```

**Respuesta:**
```json
{
  "documentType": "Customer",
  "nextNumber": "CUST-0002",
  "date": "2025-12-14T10:30:00"
}
```

### 4. Usar en el Servicio de Clientes

```csharp
public async Task<CustomerDto> CreateCustomerAsync(Guid organizationId, CreateCustomerDto dto)
{
    // Obtener el siguiente número de cliente automáticamente
    var customerNo = await _serieNoService.GetNextNumberByTypeAsync(
        organizationId,
        DocumentType.Customer
    );

    var customer = new Customer
    {
        Id = Guid.NewGuid(),
        CustomerNo = customerNo,  // CUST-0001, CUST-0002, etc.
        // ... resto de propiedades
    };

    // Guardar cliente...
}
```

### 5. Validar un Número

```http
GET /api/SerieNo/validate/CUST/CUST-0050
```

**Respuesta:**
```json
{
  "serieCode": "CUST",
  "number": "CUST-0050",
  "isValid": true
}
```

### 6. Verificar Disponibilidad

```http
GET /api/SerieNo/available/CUST/CUST-0100
```

**Respuesta:**
```json
{
  "serieCode": "CUST",
  "number": "CUST-0100",
  "isAvailable": true
}
```

## Endpoints Disponibles

### Gestión de Series

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/SerieNo` | Obtener todas las series |
| GET | `/api/SerieNo/{id}` | Obtener serie por ID |
| GET | `/api/SerieNo/code/{code}` | Obtener serie por código |
| POST | `/api/SerieNo` | Crear nueva serie |
| PUT | `/api/SerieNo/{id}` | Actualizar serie |
| DELETE | `/api/SerieNo/{id}` | Eliminar serie |

### Gestión de Líneas

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/SerieNo/{serieId}/lines` | Agregar línea a serie |
| PUT | `/api/SerieNo/lines/{lineId}` | Actualizar línea |
| DELETE | `/api/SerieNo/lines/{lineId}` | Eliminar línea |

### Generación de Números (CORE)

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/SerieNo/next/{code}` | Siguiente número por código |
| GET | `/api/SerieNo/next/type/{documentType}` | Siguiente número por tipo |
| GET | `/api/SerieNo/type/{documentType}` | Series por tipo de documento |
| GET | `/api/SerieNo/type/{documentType}/default` | Serie por defecto por tipo |

### Validación

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/SerieNo/validate/{code}/{number}` | Validar número |
| GET | `/api/SerieNo/available/{code}/{number}` | Verificar disponibilidad |

## Propiedades de NoSerie

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Code` | string | Código único de la serie (ej: "CUST", "INV") |
| `Description` | string | Descripción de la serie |
| `DocumentType` | enum? | Tipo de documento asociado |
| `DefaultNos` | bool | Si es la serie por defecto para el tipo |
| `ManualNos` | bool | Permite ingreso manual de números |
| `DateOrder` | bool | Valida orden cronológico de fechas |

## Propiedades de NoSerieLine

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `StartingDate` | DateTime | Fecha desde la cual aplica esta línea |
| `StartingNo` | string | Primer número del rango (ej: "INV-0001") |
| `EndingNo` | string | Último número del rango (ej: "INV-9999") |
| `LastNoUsed` | string | Último número utilizado (automático) |
| `IncrementBy` | int | Incremento (normalmente 1) |
| `WarningNo` | string | Número de advertencia (opcional) |
| `Open` | bool | Si la línea está activa |

## Flujo de Trabajo Recomendado

1. **Configuración Inicial:**
   - Crear series para cada tipo de documento que necesites
   - Marcar una como `defaultNos: true` para cada tipo
   - Configurar líneas con rangos apropiados

2. **Durante la Creación de Registros:**
   ```csharp
   // Obtener número automáticamente
   var nextNo = await _serieNoService.GetNextNumberByTypeAsync(orgId, DocumentType.Customer);

   // Usar el número
   customer.CustomerNo = nextNo;

   // Guardar
   await _dbContext.SaveChangesAsync();
   ```

3. **Mantenimiento:**
   - Monitorear cuando una línea se acerque a `warningNo`
   - Crear nuevas líneas antes de agotar el rango
   - Cerrar líneas antiguas (`open: false`)

## Manejo de Errores

El servicio lanza excepciones específicas:

- `KeyNotFoundException`: Serie no encontrada
- `InvalidOperationException`:
  - Rango agotado
  - Código duplicado
  - Línea con fecha solapada
  - Número sin parte numérica

## Notas Importantes

1. **Concurrencia**: El sistema actualiza `LastNoUsed` en la base de datos, manejando concurrencia automáticamente

2. **Formato de Números**: Deben tener una parte numérica al final para poder incrementarse:
   - ✅ "CUST-0001", "INV-2025-0001", "0001"
   - ❌ "CUSTOMER", "ABC"

3. **Longitud**: Se preserva la longitud de ceros a la izquierda:
   - "0001" → "0002" (no "2")
   - "INV-00001" → "INV-00002"

4. **Múltiples Líneas**: Útil para cambios de año o periodicidad:
   ```
   2025: INV-2025-0001 → INV-2025-9999
   2026: INV-2026-0001 → INV-2026-9999
   ```

## Ejemplo Completo: Integración con Customers

```csharp
public class CustomerService : ICustomerService
{
    private readonly LiveSoldDbContext _dbContext;
    private readonly ISerieNoService _serieNoService;

    public async Task<CustomerDto> CreateCustomerAsync(Guid organizationId, CreateCustomerDto dto)
    {
        // 1. Obtener siguiente número de cliente
        var customerNo = await _serieNoService.GetNextNumberByTypeAsync(
            organizationId,
            DocumentType.Customer,
            DateTime.UtcNow
        );

        // 2. Crear el contacto
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone
        };

        _dbContext.Contacts.Add(contact);

        // 3. Crear el cliente con el número generado
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ContactId = contact.Id,
            CustomerNo = customerNo,  // ← Número automático
            PasswordHash = PasswordHelper.HashPassword(dto.Password),
            IsActive = true
        };

        _dbContext.Customers.Add(customer);

        // 4. Crear wallet
        var wallet = new Wallet
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            CustomerId = customer.Id,
            Balance = 0
        };

        _dbContext.Wallets.Add(wallet);

        await _dbContext.SaveChangesAsync();

        return MapToDto(customer);
    }
}
```

## Seguridad

- ✅ Todos los endpoints requieren autenticación (`[Authorize]`)
- ✅ CRUD de series requiere rol `OrgOwner`
- ✅ Obtención de números requiere rol `Employee`
- ✅ Validación por `organizationId` en todas las operaciones
