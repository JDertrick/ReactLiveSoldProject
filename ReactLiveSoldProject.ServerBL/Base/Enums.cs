namespace ReactLiveSoldProject.ServerBL.Base
{
    /// <summary>
    /// Roles de usuarios dentro de una organización
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Vendedor - Puede gestionar clientes, ventas y transacciones de wallet
        /// </summary>
        Seller,

        /// <summary>
        /// Propietario - Puede hacer todo lo que hace un Seller + gestionar productos, variantes, tags y miembros
        /// </summary>
        Owner,

        /// <summary>
        /// Super Administrador - Acceso global para gestionar organizaciones (sin acceso a datos de organizaciones)
        /// </summary>
        SuperAdmin
    }

    /// <summary>
    /// Estado de una orden de venta
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Borrador - La orden está siendo creada
        /// </summary>
        Draft,

        /// <summary>
        /// Completada - La orden fue finalizada y el pago procesado
        /// </summary>
        Completed,

        /// <summary>
        /// Cancelada - La orden fue cancelada
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// Tipo de transacción en la billetera
    /// </summary>
    public enum TransactionType
    {
        /// <summary>
        /// Depósito - Agregar fondos a la billetera
        /// </summary>
        Deposit,

        /// <summary>
        /// Retiro - Restar fondos de la billetera (usado en ventas)
        /// </summary>
        Withdrawal
    }

    /// <summary>
    /// Tipo de producto
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// Producto simple - Un solo SKU/precio
        /// </summary>
        Simple,

        /// <summary>
        /// Producto variable - Múltiples variantes (ej: tallas, colores)
        /// </summary>
        Variable
    }

    /// <summary>
    /// Plan de suscripción de la organización
    /// </summary>
    public enum PlanType
    {
        /// <summary>
        /// Plan gratuito
        /// </summary>
        Free,

        /// <summary>
        /// Plan estándar
        /// </summary>
        Standard,

        /// <summary>
        /// Plan premium
        /// </summary>
        Premium,

        /// <summary>
        /// Plan empresarial
        /// </summary>
        Enterprise
    }

    /// <summary>
    /// Tipo de acción de auditoría
    /// </summary>
    public enum AuditActionType
    {
        /// <summary>
        /// Creación de registro
        /// </summary>
        Create,

        /// <summary>
        /// Actualización de registro
        /// </summary>
        Update,

        /// <summary>
        /// Eliminación de registro
        /// </summary>
        Delete
    }

    /// <summary>
    /// Tipo de venta (precio a aplicar)
    /// </summary>
    public enum SaleType
    {
        /// <summary>
        /// Venta al detal/minorista - Usa Price
        /// </summary>
        Retail,

        /// <summary>
        /// Venta al por mayor/mayorista - Usa WholesalePrice (si existe, sino Price)
        /// </summary>
        Wholesale
    }

    /// <summary>
    /// Tipo de movimiento de inventario
    /// </summary>
    public enum StockMovementType
    {
        /// <summary>
        /// Entrada inicial de inventario
        /// </summary>
        InitialStock,

        /// <summary>
        /// Compra/entrada de mercancía
        /// </summary>
        Purchase,

        /// <summary>
        /// Venta de producto
        /// </summary>
        Sale,

        /// <summary>
        /// Devolución de cliente
        /// </summary>
        Return,

        /// <summary>
        /// Ajuste manual de inventario (positivo o negativo)
        /// </summary>
        Adjustment,

        /// <summary>
        /// Merma o pérdida de inventario
        /// </summary>
        Loss,

        /// <summary>
        /// Transferencia entre ubicaciones
        /// </summary>
        Transfer,

        /// <summary>
        /// Cancelación de venta (devuelve stock)
        /// </summary>
        SaleCancellation,

        /// <summary>
        /// Ajuste por auditoría/toma física de inventario
        /// </summary>
        InventoryAudit
    }

    /// <summary>
    /// Estado de una auditoría de inventario
    /// </summary>
    public enum InventoryAuditStatus
    {
        /// <summary>
        /// Borrador - Snapshot tomado, pendiente de iniciar conteo
        /// </summary>
        Draft,

        /// <summary>
        /// En progreso - Conteo en curso
        /// </summary>
        InProgress,

        /// <summary>
        /// Completada - Conteo finalizado y ajustes aplicados
        /// </summary>
        Completed,

        /// <summary>
        /// Cancelada - Auditoría cancelada sin aplicar ajustes
        /// </summary>
        Cancelled
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Tipo de sistema de impuestos
    /// </summary>
    public enum TaxSystemType
    {
        /// <summary>
        /// Sin impuestos
        /// </summary>
        None,

        /// <summary>
        /// IVA (Impuesto al Valor Agregado) - Latinoamérica, Europa
        /// </summary>
        VAT,

        /// <summary>
        /// Sales Tax - USA, Canadá
        /// </summary>
        SalesTax,

        /// <summary>
        /// GST (Goods and Services Tax) - Australia, India, Nueva Zelanda
        /// </summary>
        GST,

        /// <summary>
        /// IGV (Impuesto General a las Ventas) - Perú
        /// </summary>
        IGV
    }

    /// <summary>
    /// Modo de aplicación del impuesto
    /// </summary>
    public enum TaxApplicationMode
    {
        /// <summary>
        /// El precio del producto ya incluye el impuesto (común en Europa/Latinoamérica)
        /// </summary>
        TaxIncluded,

        /// <summary>
        /// El impuesto se agrega al precio del producto (común en USA/Canadá)
        /// </summary>
        TaxExcluded
    }

    /// <summary>
    /// Tipo de cuenta contable para el plan de cuentas
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// Activos - Recursos económicos de la empresa
        /// </summary>
        Asset,

        /// <summary>
        /// Pasivos - Obligaciones financieras de la empresa
        /// </summary>
        Liability,

        /// <summary>
        /// Patrimonio Neto - Inversión de los propietarios en la empresa
        /// </summary>
        Equity,

        /// <summary>
        /// Ingresos - Incrementos en los beneficios económicos
        /// </summary>
        Revenue,

        /// <summary>
        /// Gastos - Disminuciones en los beneficios económicos
        /// </summary>
        Expense
    }

    /// <summary>
    /// Identificadores para cuentas contables que el sistema usa internamente.
    /// </summary>
    public enum SystemAccountType
    {
        AccountsReceivable,   // Cuentas por Cobrar (Activo)
        SalesRevenue,         // Ingresos por Ventas (Ingreso)
        SalesTaxPayable,      // Impuestos por Pagar (Pasivo)
        Inventory,            // Inventario (Activo)
        CostOfGoodsSold,      // Costo de Mercancía Vendida (Gasto)
        AccountsPayable,      // Cuentas por Pagar (Pasivo)
        Cash,                 // Efectivo (Activo)
        WalletBalance         // Saldo de Billetera (contrapartida de Cuentas por Cobrar)
    }
}
