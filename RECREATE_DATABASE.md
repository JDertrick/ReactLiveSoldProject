# 🔄 Recrear Base de Datos con Nuevos Datos de Prueba

## ✅ Cambios Agregados al Seeder

Se ha agregado un **Customer de prueba** para el Customer Portal:

```
Email: maria@cliente.com
Password: Customer123!
Organización: Tienda Demo (slug: tienda-demo)
Balance Inicial: $1,000.00
```

## 📋 Opción 1: Borrar y Recrear Base de Datos (Más Fácil)

### Desde el directorio del proyecto ServerBL:

```bash
cd ReactLiveSoldProject.ServerBL

# Eliminar la base de datos actual
"/mnt/c/Program Files/dotnet/dotnet.exe" ef database drop --force

# Aplicar las migraciones nuevamente
"/mnt/c/Program Files/dotnet/dotnet.exe" ef database update
```

### O desde PowerShell en Windows:

```powershell
cd ReactLiveSoldProject.ServerBL

# Eliminar la base de datos actual
dotnet ef database drop --force

# Aplicar las migraciones nuevamente
dotnet ef database update
```

## 📋 Opción 2: Usando Docker PostgreSQL

Si estás usando Docker:

```bash
cd postgresql

# Detener y eliminar contenedor
docker-compose down -v

# Iniciar nuevamente (creará nueva base de datos)
docker-compose up -d
```

Luego ejecuta las migraciones:

```bash
cd ../ReactLiveSoldProject.ServerBL
"/mnt/c/Program Files/dotnet/dotnet.exe" ef database update
```

## 🚀 Después de Recrear

Inicia el backend:

```bash
cd ../ReactLiveSoldProject.Server
"/mnt/c/Program Files/dotnet/dotnet.exe" run
```

Verás en la consola:
```
✓ Datos semilla creados exitosamente:
  - SuperAdmin: admin@livesold.com / Admin123!
  - Owner: juan@tiendademo.com / Owner123!
  - Customer: maria@cliente.com / Customer123!
  - Organización: Tienda Demo (slug: tienda-demo)
  - Wallet inicial: $1,000.00
```

## 🔑 Credenciales de Prueba

### Portal de Cliente (Customer)
- URL: http://localhost:5173/portal/tienda-demo/login
- Email: `maria@cliente.com`
- Password: `Customer123!`

### Login de Empleados
- SuperAdmin: `admin@livesold.com` / `Admin123!`
- Owner: `juan@tiendademo.com` / `Owner123!`

## ⚠️ Importante

El seeder solo se ejecuta si la base de datos está **completamente vacía** (no hay usuarios). Por eso necesitas borrar y recrear la base de datos.
