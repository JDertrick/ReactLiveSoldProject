# Configuración Automática de Series Numéricas

## Descripción General

Al crear una nueva organización desde el panel de SuperAdmin, el sistema automáticamente configura **14 series numéricas** predeterminadas, una para cada tipo de documento del sistema.

## Series Creadas Automáticamente

Cada organización nueva recibe las siguientes series numéricas configuradas y listas para usar:

### 1. **CUST** - Clientes
- **Código**: CUST
- **Descripción**: Numeración de Clientes
- **Formato**: CUST-2025-0001 hasta CUST-2025-9999
- **Advertencia**: CUST-2025-9900
- **Por Defecto**: ✓

### 2. **VEND** - Proveedores
- **Código**: VEND
- **Descripción**: Numeración de Proveedores
- **Formato**: VEND-2025-0001 hasta VEND-2025-9999
- **Advertencia**: VEND-2025-9900
- **Por Defecto**: ✓

### 3. **SINV** - Facturas de Venta
- **Código**: SINV
- **Descripción**: Facturas de Venta
- **Formato**: SINV-2025-0001 hasta SINV-2025-9999
- **Advertencia**: SINV-2025-9800
- **Por Defecto**: ✓

### 4. **SO** - Órdenes de Venta
- **Código**: SO
- **Descripción**: Órdenes de Venta
- **Formato**: SO-2025-0001 hasta SO-2025-9999
- **Advertencia**: SO-2025-9800
- **Por Defecto**: ✓

### 5. **PINV** - Facturas de Compra
- **Código**: PINV
- **Descripción**: Facturas de Compra
- **Formato**: PINV-2025-0001 hasta PINV-2025-9999
- **Advertencia**: PINV-2025-9800
- **Por Defecto**: ✓

### 6. **PO** - Órdenes de Compra
- **Código**: PO
- **Descripción**: Órdenes de Compra
- **Formato**: PO-2025-0001 hasta PO-2025-9999
- **Advertencia**: PO-2025-9800
- **Por Defecto**: ✓

### 7. **PREC** - Recepciones de Compra
- **Código**: PREC
- **Descripción**: Recepciones de Compra
- **Formato**: PREC-2025-0001 hasta PREC-2025-9999
- **Advertencia**: PREC-2025-9800
- **Por Defecto**: ✓

### 8. **PROD** - Productos
- **Código**: PROD
- **Descripción**: Numeración de Productos
- **Formato**: PROD-0001 hasta PROD-99999
- **Advertencia**: PROD-99000
- **Por Defecto**: ✓
- **Nota**: No incluye año, es secuencia continua

### 9. **VAR** - Variantes de Producto
- **Código**: VAR
- **Descripción**: Variantes de Producto
- **Formato**: VAR-0001 hasta VAR-99999
- **Advertencia**: VAR-99000
- **Por Defecto**: ✓
- **Nota**: No incluye año, es secuencia continua

### 10. **PAY** - Pagos
- **Código**: PAY
- **Descripción**: Pagos
- **Formato**: PAY-2025-0001 hasta PAY-2025-9999
- **Advertencia**: PAY-2025-9800
- **Por Defecto**: ✓

### 11. **JE** - Asientos Contables
- **Código**: JE
- **Descripción**: Asientos Contables
- **Formato**: JE-2025-0001 hasta JE-2025-9999
- **Advertencia**: JE-2025-9800
- **Por Defecto**: ✓

### 12. **WT** - Transacciones de Billetera
- **Código**: WT
- **Descripción**: Transacciones de Billetera
- **Formato**: WT-2025-0001 hasta WT-2025-99999
- **Advertencia**: WT-2025-99000
- **Por Defecto**: ✓

### 13. **SM** - Movimientos de Inventario
- **Código**: SM
- **Descripción**: Movimientos de Inventario
- **Formato**: SM-2025-0001 hasta SM-2025-99999
- **Advertencia**: SM-2025-99000
- **Por Defecto**: ✓

### 14. **CONT** - Contactos
- **Código**: CONT
- **Descripción**: Numeración de Contactos
- **Formato**: CONT-0001 hasta CONT-99999
- **Advertencia**: CONT-99000
- **Por Defecto**: ✓
- **Nota**: No incluye año, es secuencia continua

## Características de las Series Predeterminadas

### Líneas de Numeración
Cada serie incluye **una línea activa** con:
- **Fecha de Inicio**: 1 de enero del año actual
- **Estado**: Abierta (Open = true)
- **Incremento**: 1
- **Último Número Usado**: null (sin usar aún)

### Configuración de Series
Todas las series se crean con:
- **DefaultNos**: true (serie por defecto para su tipo)
- **ManualNos**: false (no permite numeración manual)
- **DateOrder**: true (valida orden cronológico)

### Números de Advertencia (Warning Numbers)
Las series incluyen números de advertencia para alertar cuando se está agotando el rango:
- **Documentos transaccionales** (facturas, órdenes, pagos): 200 números antes del final
- **Maestros de datos** (productos, contactos): 1000 números antes del final

## Implementación Técnica

### Clase Seeder
**Archivo**: `ReactLiveSoldProject.ServerBL/Helpers/DefaultNoSeriesSeeder.cs`

```csharp
public class DefaultNoSeriesSeeder
{
    public async Task SeedDefaultNoSeriesAsync(Guid organizationId)
    {
        // Crea todas las series por defecto
    }
}
```

### Integración en OrganizationService
**Archivo**: `ReactLiveSoldProject.ServerBL/Infrastructure/Services/OrganizationService.cs`

El método `CreateOrganizationAsync` ejecuta automáticamente:
1. Creación de la organización
2. Seed del catálogo de cuentas contables
3. Configuración contable por defecto
4. **Seed de series numéricas** ← NUEVO

```csharp
// Crear series numéricas por defecto
var noSeriesSeeder = new DefaultNoSeriesSeeder(_dbContext);
await noSeriesSeeder.SeedDefaultNoSeriesAsync(organization.Id);
```

## Ventajas para el Usuario

1. **Configuración Inmediata**: No requiere configuración manual de series
2. **Convención Estándar**: Códigos y formatos consistentes y profesionales
3. **Listo para Producción**: Rangos amplios que soportan alto volumen
4. **Trazabilidad por Año**: La mayoría de series incluyen el año en el formato
5. **Alertas Preventivas**: Números de advertencia para evitar quedarse sin rangos

## Personalización Posterior

Los usuarios pueden:
- Modificar las series existentes desde `/app/no-series`
- Crear series adicionales para el mismo tipo de documento
- Cambiar la serie por defecto
- Agregar líneas nuevas con rangos diferentes
- Habilitar numeración manual si lo requieren

## Próximos Pasos de Integración

Para que las series se usen automáticamente, se debe integrar en los servicios:

```csharp
// Ejemplo en CustomerService
var customerNumber = await _serieNoService.GetNextNumberByTypeAsync(
    organizationId,
    DocumentType.Customer
);

customer.CustomerNo = customerNumber;
```

## Migración de Base de Datos

Asegúrate de aplicar la migración que agrega la columna `DocumentType`:

```bash
dotnet ef database update --project ReactLiveSoldProject.ServerBL --startup-project ReactLiveSoldProject.Server
```

## Notas Importantes

- Las series con año se reiniciarán cada año agregando nuevas líneas
- Los productos y contactos usan secuencias continuas sin año
- Todas las series están marcadas como "por defecto" para su tipo
- La validación de orden cronológico está habilitada en todas las series
