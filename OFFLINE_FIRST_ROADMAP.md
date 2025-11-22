# Roadmap de Implementación: Arquitectura Offline-First

## ⏱️ Tiempo Total Estimado: 4-5 meses

---

## 🎯 FASE 0: Preparación y Setup (2 semanas)

### Semana 1: Análisis y Diseño
- [ ] Definir prioridades de features offline (qué es crítico vs. nice-to-have)
- [ ] Diseñar estrategia de IDs (UUIDs vs. auto-increment)
- [ ] Diseñar estrategia de resolución de conflictos por entidad
- [ ] Definir límites de almacenamiento local (cuántos productos/órdenes cachear)
- [ ] Crear documento de arquitectura detallado
- [ ] Planificar migración de datos existentes

### Semana 2: Setup de Dependencias
- [ ] Instalar RxDB, Dexie, vite-plugin-pwa
```bash
cd reactlivesoldproject.client
npm install rxdb dexie uuid
npm install vite-plugin-pwa -D
npm install @types/uuid -D
```
- [ ] Configurar Vite para PWA
- [ ] Crear estructura de carpetas:
```
src/
├── db/
│   ├── schemas/         # Schemas de RxDB
│   ├── database.ts      # Inicialización de DB
│   ├── replication.ts   # Lógica de sync
│   └── migrations.ts    # Migraciones de schema
├── services/
│   ├── offline/         # Servicios offline-first
│   │   ├── salesOrderService.ts
│   │   ├── productService.ts
│   │   ├── customerService.ts
│   │   └── stockMovementService.ts
│   └── sync/
│       ├── conflictResolver.ts
│       └── syncManager.ts
└── hooks/
    ├── useRxDB.ts       # Hook para acceder a RxDB
    ├── useRxCollection.ts
    └── useOfflineStatus.ts
```

**Entregables:**
- Documento de arquitectura
- Proyecto configurado con PWA
- Estructura de carpetas creada

---

## 🗄️ FASE 1: Base de Datos Local (3 semanas)

### Semana 3: Schemas y Database Setup
- [ ] Crear schemas RxDB para todas las entidades:
  - Products & ProductVariants
  - Customers & Wallets
  - SalesOrders & SalesOrderItems
  - StockMovements
  - WalletTransactions
  - TaxRates
- [ ] Implementar `initDatabase()` con encryption
- [ ] Crear hooks de React para RxDB:
```typescript
// useRxCollection.ts
export function useRxCollection<T>(collectionName: string) {
  const db = useRxDB();
  return db[collectionName] as RxCollection<T>;
}

// useRxQuery.ts
export function useRxQuery<T>(query: MangoQuery<T>) {
  const [results, setResults] = useState<T[]>([]);

  useEffect(() => {
    const sub = collection.find(query).$.subscribe(setResults);
    return () => sub.unsubscribe();
  }, [query]);

  return results;
}
```
- [ ] Testing: crear, leer, actualizar, eliminar documentos localmente

### Semana 4: Servicios Offline Básicos
- [ ] Implementar `OfflineProductService`:
  - Listar productos (con filtros)
  - Crear/editar productos (marcar como pending_sync)
  - Gestión de variantes
- [ ] Implementar `OfflineCustomerService`:
  - CRUD de clientes
  - Búsqueda local
- [ ] Implementar `OfflineTaxService`:
  - Cachear tasas de impuestos
  - Cálculos locales
- [ ] Testing: operaciones CRUD sin conexión

### Semana 5: Lógica de Negocio Compleja Offline
- [ ] Implementar `OfflineSalesOrderService`:
  - Crear orden (validar stock local)
  - Agregar/remover items
  - Calcular totales con impuestos
  - Finalizar orden (decrementar stock, debitar wallet)
- [ ] Implementar `OfflineStockMovementService`:
  - Crear movimientos
  - Actualizar stock de variantes
- [ ] Implementar `OfflineWalletService`:
  - Depósitos/retiros
  - Validar balance local
- [ ] Testing: flujo completo de venta offline

**Entregables:**
- Base de datos local funcional
- Servicios offline para todas las operaciones críticas
- Tests unitarios de lógica offline

---

## 🔄 FASE 2: Backend Adaptation (4 semanas)

### Semana 6: Modificación de Modelos
- [ ] Agregar campos `UpdatedAt`, `IsDeleted`, `LastModifiedBy` a todos los modelos
- [ ] Crear clase base `SyncableEntity`
- [ ] Actualizar `DbContext` para auto-populate `UpdatedAt`
```csharp
public override int SaveChanges()
{
    foreach (var entry in ChangeTracker.Entries<SyncableEntity>())
    {
        if (entry.State == EntityState.Modified)
        {
            entry.Entity.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
    return base.SaveChanges();
}
```
- [ ] Crear y aplicar migration
```bash
dotnet ef migrations add AddOfflineFirstFields --project ReactLiveSoldProject.ServerBL
dotnet ef database update --project ReactLiveSoldProject.ServerBL --startup-project ReactLiveSoldProject.Server
```

### Semana 7-8: Endpoints de Replicación (Pull)
- [ ] Crear `ReplicationController.cs`
- [ ] Implementar endpoints de PULL para:
  - Products & ProductVariants
  - Customers & Wallets
  - TaxRates
  - SalesOrders (solo las del usuario)
- [ ] Paginación y checkpoints
- [ ] Filtrado por OrganizationId
- [ ] Testing: verificar que solo se devuelven datos de la org correcta

### Semana 9: Endpoints de Replicación (Push)
- [ ] Implementar endpoints de PUSH para todas las entidades
- [ ] Lógica de detección de conflictos (timestamp comparison)
- [ ] Validaciones de negocio:
  - Stock no puede ser negativo
  - Wallet no puede quedar en negativo
  - Órdenes duplicadas (idempotencia)
- [ ] Logging de operaciones de sync
- [ ] Testing: enviar cambios locales al servidor

**Entregables:**
- Backend listo para sincronización
- Endpoints de replicación funcionales
- Validaciones de negocio implementadas

---

## 🔁 FASE 3: Sincronización Bidireccional (4 semanas)

### Semana 10-11: Implementar Replication en Frontend
- [ ] Configurar RxDB Replication plugin
- [ ] Implementar `setupReplication()` para cada colección
- [ ] Estrategia Pull: Network First
  - Descargar cambios del servidor al iniciar app
  - Polling cada 30 segundos cuando online
- [ ] Estrategia Push: Queue local
  - Detectar cambios con `_pending_sync` flag
  - Enviar en lotes de 20 documentos
  - Retry con exponential backoff
- [ ] Testing: sync inicial de catálogo

### Semana 12: Manejo de Conflictos
- [ ] Implementar `ConflictResolver`:
```typescript
interface ConflictResolutionStrategy {
  resolve(local: any, remote: any): any;
}

class LastWriteWinsStrategy implements ConflictResolutionStrategy {
  resolve(local: any, remote: any) {
    return local.updatedAt > remote.updatedAt ? local : remote;
  }
}

class ServerAuthorityStrategy implements ConflictResolutionStrategy {
  resolve(local: any, remote: any) {
    return remote; // Servidor siempre gana
  }
}
```
- [ ] Aplicar estrategias por entidad:
  - Products/Customers: Last-Write-Wins
  - StockMovements: Server Authority
  - SalesOrders: Custom (mostrar conflicto al usuario)
- [ ] UI para mostrar conflictos no resueltos automáticamente
- [ ] Testing: simular conflictos y verificar resolución

### Semana 13: Optimización de Sync
- [ ] Implementar delta sync (solo cambios desde último checkpoint)
- [ ] Compresión de payloads (gzip)
- [ ] Batch requests (agrupar múltiples collections en un request)
- [ ] Sync selectivo (permitir al usuario elegir qué sincronizar)
- [ ] Progress indicator durante sync
- [ ] Testing: sync de 1000+ productos en <30 segundos

**Entregables:**
- Sincronización bidireccional funcional
- Manejo de conflictos automático y manual
- Performance optimizado

---

## 🎨 FASE 4: Interfaz de Usuario Offline (3 semanas)

### Semana 14: Indicadores de Estado
- [ ] Crear componente `<OfflineIndicator />`:
```tsx
export function OfflineIndicator() {
  const isOffline = useOfflineStatus();
  const syncState = useSyncState();

  return (
    <div className={isOffline ? 'bg-red-500' : 'bg-green-500'}>
      {isOffline ? '📡 Sin conexión' : '✓ Conectado'}
      {syncState.pending > 0 && (
        <span>{syncState.pending} cambios pendientes</span>
      )}
    </div>
  );
}
```
- [ ] Badge en cada documento indicando si está sincronizado
- [ ] Progress bar durante sync
- [ ] Notificaciones de sync completado/fallido

### Semana 15: Adaptaciones de UI
- [ ] Deshabilitar features que requieren conexión obligatoria (si las hay)
- [ ] Mostrar advertencias en operaciones críticas:
  - "⚠️ Trabajando offline. Los cambios se sincronizarán cuando vuelva la conexión."
- [ ] Botón de "Sincronizar ahora" (manual trigger)
- [ ] Página de "Estado de Sincronización":
  - Última sync exitosa
  - Cambios pendientes por entidad
  - Conflictos sin resolver
  - Tamaño de caché local

### Semana 16: Manejo de Errores
- [ ] Toast notifications para errores de sync
- [ ] Página de "Conflictos" para resolución manual
- [ ] Logs de sync accesibles para debugging
- [ ] Botón de "Reset local database" (emergency)
- [ ] Testing: todos los flujos de error

**Entregables:**
- UI completa para trabajar offline
- Feedback visual de estado de sync
- Herramientas de debugging para usuarios

---

## 🚀 FASE 5: PWA y Optimización (2 semanas)

### Semana 17: PWA Completo
- [ ] Configurar `manifest.json`:
```json
{
  "name": "LiveSold - Gestión Retail",
  "short_name": "LiveSold",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#000000",
  "icons": [
    {
      "src": "/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    },
    {
      "src": "/icon-512.png",
      "sizes": "512x512",
      "type": "image/png"
    }
  ]
}
```
- [ ] Service Worker para cachear assets (JS, CSS, imágenes)
- [ ] Estrategia de caché para imágenes de productos:
  - Cache First con límite de 100MB
  - LRU eviction
- [ ] Install prompt ("Agregar a inicio")
- [ ] Testing: instalar como app en Android/iOS

### Semana 18: Performance y Cleanup
- [ ] Profiling de performance:
  - Tiempo de carga inicial
  - Tiempo de sync inicial (target: <1 min para 500 productos)
  - Uso de memoria (target: <200MB)
- [ ] Lazy loading de colecciones grandes
- [ ] Implementar paginación virtual en listas
- [ ] Cleanup automático de datos antiguos (> 30 días sin uso)
- [ ] Optimización de índices en RxDB
- [ ] Code splitting y tree shaking
- [ ] Testing de performance en dispositivos low-end

**Entregables:**
- App instalable como PWA
- Performance optimizado
- Gestión eficiente de memoria

---

## ✅ FASE 6: Testing y Deployment (3 semanas)

### Semana 19: Testing Integral
- [ ] Unit tests de servicios offline
- [ ] Integration tests de sync
- [ ] E2E tests con Playwright:
  - Crear orden offline → sync → verificar en backend
  - Conflictos → resolución → verificar resultado
- [ ] Testing en dispositivos reales:
  - Android (Chrome)
  - iOS (Safari)
  - Tablets
- [ ] Testing de escenarios extremos:
  - 1000+ productos
  - 100+ órdenes pendientes de sync
  - Cambiar de online a offline en medio de operación
  - Network flakiness (intermitente)

### Semana 20: User Acceptance Testing (UAT)
- [ ] Deploy a staging con usuarios reales
- [ ] Capacitación a vendedores sobre:
  - Cómo funciona el modo offline
  - Qué hacer si hay conflictos
  - Cuándo sincronizar manualmente
- [ ] Recopilar feedback y ajustar UI
- [ ] Documentación de usuario:
  - Manual de uso offline
  - Troubleshooting común
  - FAQs

### Semana 21: Production Deployment
- [ ] Migración de base de datos en producción
- [ ] Deploy gradual (phased rollout):
  - 10% de usuarios (1 día)
  - 50% de usuarios (3 días)
  - 100% de usuarios
- [ ] Monitoreo intensivo:
  - Errores de sync
  - Performance metrics
  - Crashes
- [ ] Hotfixes si es necesario
- [ ] Post-mortem y retrospectiva

**Entregables:**
- Sistema offline-first en producción
- Usuarios capacitados
- Documentación completa

---

## 📊 Resumen de Tiempo y Recursos

| Fase | Duración | Complejidad | Personal Requerido |
|------|----------|-------------|-------------------|
| 0. Preparación | 2 semanas | Baja | 1 dev full-stack |
| 1. DB Local | 3 semanas | Media | 1 dev frontend |
| 2. Backend | 4 semanas | Media | 1 dev backend |
| 3. Sync | 4 semanas | Alta | 1 dev full-stack |
| 4. UI | 3 semanas | Media | 1 dev frontend |
| 5. PWA | 2 semanas | Baja | 1 dev frontend |
| 6. Testing | 3 semanas | Media | 2 devs + QA |
| **TOTAL** | **21 semanas (5 meses)** | **Alta** | **1-2 devs** |

### Costos Estimados (asumiendo 1 dev full-stack)
- Desarrollo: 21 semanas × 40 horas = **840 horas**
- A $50/hora = **$42,000 USD**
- A $30/hora = **$25,200 USD**

### Riesgos Principales
1. **Conflictos de datos complejos** → Requiere lógica custom robusta
2. **Performance en dispositivos low-end** → Puede requerir optimizaciones extra
3. **Curva de aprendizaje de RxDB** → Primera vez usándolo
4. **Migración de usuarios existentes** → Requiere estrategia cuidadosa
5. **Bugs inesperados en sync** → Puede alargar la fase de testing

---

## 🎯 Hitos Clave (Milestones)

| Hito | Fecha (desde inicio) | Criterio de Éxito |
|------|---------------------|-------------------|
| ✅ Setup completo | Semana 2 | PWA instalable, RxDB configurado |
| ✅ DB local funcional | Semana 5 | Crear venta offline completa |
| ✅ Backend listo | Semana 9 | Endpoints de sync funcionales |
| ✅ Sync bidireccional | Semana 13 | Sync automático online/offline |
| ✅ UI completa | Semana 16 | UX fluida en modo offline |
| ✅ PWA optimizado | Semana 18 | <1min sync inicial, <200MB RAM |
| 🚀 Producción | Semana 21 | 100% usuarios usando offline-first |

---

## 📝 Checklist Pre-Implementación

Antes de empezar, asegúrate de:
- [ ] Aprobar presupuesto y tiempo
- [ ] Definir prioridades de features (MVP vs. nice-to-have)
- [ ] Tener ambiente de staging para testing
- [ ] Tener dispositivos de prueba (Android/iOS)
- [ ] Comunicar cambios a stakeholders
- [ ] Planificar capacitación de usuarios
- [ ] Definir estrategia de rollback si algo falla

---

## 🚨 Plan de Contingencia

Si el proyecto se alarga o hay bloqueos:

### Plan B: MVP Reducido (3 meses)
- Solo órdenes y productos offline
- Sin wallet offline (requiere conexión)
- Sin auditorías de inventario offline
- Sync manual (botón, no automático)

### Plan C: Híbrido (2 meses)
- Solo draft orders offline
- Finalización requiere conexión
- Caché de catálogo para consulta
- Sin sync bidireccional completo

---

## 📞 Siguiente Paso

¿Quieres que proceda con alguna de estas opciones?

1. **Implementar FASE 0 y FASE 1** (empezar con DB local)
2. **Crear prototipo de prueba de concepto** (1 semana, solo productos)
3. **Analizar Plan B/C** (versiones reducidas)
4. **Otra opción o pregunta**

Confirma y empezamos con el código 🚀
