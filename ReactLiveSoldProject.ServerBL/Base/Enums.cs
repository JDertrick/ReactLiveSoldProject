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
        SaleCancellation
    }
}
