# Changelog

All notable changes to the LiveSold React Client project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

#### Navigation & Layout
- **AppLayout Component**: Implementado layout principal con sidebar colapsable para navegación
  - Sidebar oscuro (gray-900) con transición suave entre estados expandido/colapsado
  - Logo dinámico: "LiveSold" (expandido) / "LS" (colapsado)
  - Navegación contextual basada en rol (SuperAdmin vs App)
  - Sección de usuario en la parte inferior con avatar, nombre, rol y botón de logout
  - Header sticky mostrando nombre de la página actual y email del usuario
  - Ancho responsive: 256px (expandido) / 80px (colapsado)

- **PortalLayout Component**: Mejorado layout del portal de clientes
  - Navbar sticky con branding de la organización
  - Logo y nombre de la organización obtenidos de `portalBrandStore`
  - Navegación con iconos (Dashboard, My Orders)
  - Estados activos con resaltado (bg-indigo-50)
  - Avatar del usuario con gradiente (indigo-500 to purple-600)
  - Soporte para navegación móvil
  - Botón de logout con confirmación visual

#### Dashboards

- **Customer Portal Dashboard** (`/portal/:orgSlug/dashboard`):
  - Integración completa con API usando React Query
  - Tarjeta de billetera con gradiente (indigo-500 to purple-600)
  - Estadísticas en tiempo real:
    - Total Orders (total de órdenes)
    - Total Spent (total gastado)
    - Items Purchased (artículos comprados)
  - Sección "Recent Orders" mostrando últimas 5 órdenes:
    - Número de orden
    - Estado (Finalized/Draft) con badges coloreados
    - Fecha de creación
    - Cantidad de items
    - Monto total
  - Información de perfil del cliente:
    - Nombre completo
    - Email
    - Teléfono
    - Fecha de registro
  - Estado de carga con spinner
  - Estado vacío con iconos y mensajes descriptivos

- **App Dashboard** (`/app`):
  - Grid de 6 tarjetas de estadísticas (2 filas x 3 columnas):
    - Total Customers
    - Active Customers (con icono verde)
    - Products (con contador de publicados)
    - Total Wallet Balance (balance total de billeteras)
    - Total Orders (órdenes totales)
    - Total Revenue (ingresos totales con icono de tendencia)
  - Sección "Recent Orders" mostrando últimas 5 órdenes:
    - Número de orden
    - Nombre del cliente
    - Estado con badge
    - Fecha de creación
    - Monto total
  - Sección "Recent Customers" mostrando últimos 5 clientes:
    - Nombre completo
    - Email y teléfono
    - Balance de billetera
    - Estado (Active/Inactive)
  - Quick Actions con enlaces a:
    - Manage Customers
    - Manage Products
    - Start Live Sale
  - Todas las métricas calculadas desde datos reales de la API

- **SuperAdmin Dashboard** (`/superadmin`):
  - Estadísticas de organizaciones:
    - Total Organizations
    - Active Organizations (con icono verde)
    - Inactive Organizations (con icono rojo)
  - Listado de organizaciones recientes con:
    - Logo de la organización
    - Nombre y slug
    - Tipo de plan
    - Estado (Active/Inactive)
  - Quick Actions para gestión de organizaciones

#### Customer Portal Features

- **Orders Page** (`/portal/:orgSlug/orders`):
  - Vista completa del historial de órdenes del cliente
  - Integración con `useGetCustomerOrders` hook
  - Cada orden muestra:
    - Número de orden
    - Estado con badge coloreado
    - Fecha y hora de creación
    - Items con imágenes de productos
    - Cantidad por item
    - Precio total por item
    - Monto total de la orden
  - Estadísticas resumidas:
    - Total de órdenes
    - Total gastado (suma de todas las órdenes)
    - Total de items comprados
  - Estado vacío con mensaje cuando no hay órdenes
  - Estado de carga con spinner

#### Sales Orders Implementation

- **Sales Order Hooks** (`src/hooks/useSalesOrders.ts`):
  - `useGetSalesOrders(status?)`: Obtener todas las órdenes con filtro opcional por estado
  - `useGetSalesOrder(id)`: Obtener una orden específica
  - `useGetCustomerOrders(customerId)`: Obtener órdenes de un cliente específico
  - `useCreateSalesOrder()`: Crear nueva orden de venta
  - `useUpdateSalesOrderStatus()`: Actualizar estado de una orden
  - `useCancelSalesOrder()`: Cancelar una orden
  - Invalidación automática de queries después de mutaciones

- **Sales Order Types** (`src/types/salesOrder.types.ts`):
  - `SalesOrder`: Interface completa de orden
  - `SalesOrderItem`: Interface de item de orden
  - `CreateSalesOrderDto`: DTO para crear órdenes
  - `CreateSalesOrderItemDto`: DTO para items de orden

#### Live Sales Enhancement

- **LiveSales Page** (`/app/live-sales`):
  - Validación estricta de selección de variante antes de agregar al carrito
  - CartItem interface actualizada con `variantId` y `variant` requeridos
  - Validación de balance de billetera antes de crear orden
  - Implementación completa de `handlePlaceOrder`:
    - Verificación de cliente seleccionado
    - Verificación de items en el carrito
    - Validación de fondos suficientes en billetera
    - Creación de orden via API
    - Envío de `productVariantId`, `quantity`, `customUnitPrice` opcional
    - Manejo de errores con mensajes descriptivos
    - Limpieza automática del carrito después de orden exitosa
  - Banner de éxito por 3 segundos después de crear orden
  - Deshabilitación de "Add to Cart" para productos sin variantes
  - Mensaje informativo para productos sin variantes

#### Wallet Management

- **Wallet Hooks** (`src/hooks/useWallet.ts`):
  - `useGetCustomerWallet(customerId)`: Obtener billetera de un cliente
  - `useGetWalletTransactions(customerId)`: Obtener transacciones de billetera
  - `useGetAllWallets()`: Obtener todas las billeteras
  - `useCreateWalletTransaction()`: Crear nueva transacción
  - `useAddFundsToWallet()`: Wrapper para agregar fondos (tipo Credit)
  - `useDeductFundsFromWallet()`: Wrapper para deducir fondos (tipo Debit)
  - Invalidación automática de queries relacionadas

- **Wallet Management Page** (`/app/wallet`):
  - Tarjetas de resumen con estadísticas:
    - Total Customers con billeteras
    - Total Balance acumulado
    - Average Balance por cliente
  - Tabla de billeteras mostrando:
    - Nombre del cliente
    - Email
    - Balance actual
    - Última actualización
    - Acciones: Add Funds, Deduct, History
  - Modal para agregar/deducir fondos:
    - Input de monto con validación
    - Campo de notas opcional
    - Preview del nuevo balance calculado en tiempo real
    - Botón de envío con texto contextual
  - Modal de historial de transacciones:
    - Lista completa de transacciones
    - Tipo (Credit/Debit) con indicador visual
    - Monto con formato
    - Fecha de transacción
    - Usuario que autorizó
    - Notas de la transacción

#### Authentication & Security

- **Login Pages**:
  - Eliminadas secciones visuales de credenciales de prueba
  - `EmployeeLogin.tsx`: Login para empleados (SuperAdmin/Owner/Seller)
  - `CustomerPortalLogin.tsx`: Login para clientes con branding de organización
  - Redirección automática basada en rol después de login exitoso
  - Manejo de errores con mensajes claros

### Changed

- **Router Configuration** (`src/router/index.tsx`):
  - Agregada ruta `/app/wallet` con WalletPage
  - Protected routes con validación de roles
  - Rutas organizadas por tipo de usuario

- **AppLayout Navigation**:
  - Cambiado de navbar horizontal a sidebar vertical
  - Agregado estado de colapso persistente
  - Iconos SVG para cada item de navegación
  - Navegación diferenciada para SuperAdmin y App users

- **Dashboard Grids**:
  - App Dashboard: Cambiado de 4 columnas a 3 columnas (grid responsive)
  - Todas las estadísticas ahora usan datos reales de la API
  - Agregadas secciones de Recent Orders en todos los dashboards

### Fixed

- **SuperAdmin Login Issue**:
  - Agregado import faltante `useAuthStore` en EmployeeLogin.tsx
  - Solucionado problema de caché del navegador
  - Corregida navegación basada en rol después de login

- **Tailwind CSS v4 Configuration**:
  - Actualizado postcss.config.js con `@tailwindcss/postcss`
  - Cambiado src/index.css a usar `@import "tailwindcss"`
  - Agregada directiva `@source "../../src"`

- **LoginResponse Type Mismatch**:
  - Actualizada interface LoginResponse para manejar estructura anidada del backend
  - Backend retorna `{ user: {...}, token: "..." }`
  - Corregidos todos los hooks de autenticación

### Technical Details

#### State Management
- Zustand stores con persistencia en localStorage
- React Query para server state con caching agresivo
- Invalidación automática de queries después de mutaciones

#### API Integration
- Axios client configurado con interceptores JWT
- Base URL dinámica desde environment variables
- Manejo centralizado de errores
- Tipos TypeScript para todas las respuestas

#### UI/UX
- Tailwind CSS v4 con PostCSS
- Componentes responsive mobile-first
- Estados de carga consistentes
- Estados vacíos informativos
- Animaciones suaves con transiciones CSS

#### Architecture
- Multi-tenant con aislamiento por organización
- Role-based access control (RBAC)
- Protected routes con validación
- Layouts composables con React Router

## Initial Setup

### Project Creation
- Created with Vite + React + TypeScript template
- Configured vite.config.ts for proxying and certificates
- Added @type/node for vite.config.js typing
- Created project file (reactlivesoldproject.client.esproj)
- Created launch.json for debugging
- Added to solution and startup projects

### Dependencies Installed
- React 18.3.1
- React Router v6
- TanStack Query (React Query)
- Zustand with persist middleware
- Axios
- Tailwind CSS v4
- TypeScript 5.6.2
