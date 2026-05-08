# API de Event Sourcing - Cuentas Bancarias

Este proyecto implementa el patrón **Event Sourcing** con arquitectura de **Domain-Driven Design (DDD)** para gestionar cuentas bancarias.

## 📋 Descripción

La solución contiene dos proyectos:

1. **Isra.Demos.EventSource.Models** (.NET 10)
   - Define los eventos del dominio (`DineroDepositado`, `DineroRetirado`)
   - Contiene el agregado `CuentaBancaria`
   - Usa MongoDB.Bson para serialización

2. **Isra.Demos.EventStore** (.NET 10)
   - API REST para gestionar cuentas y operaciones
   - Repositorio de eventos
   - Servicios de negocio
   - Controlador con endpoints

## 🏗️ Arquitectura

### Event Sourcing
- Cada cambio en una cuenta bancaria se registra como un evento
- Los eventos se persisten en MongoDB en orden cronológico
- El estado actual se reconstruye aplicando eventos en orden

### Eventos Disponibles
- `DineroDepositado`: Se registra cuando se deposita dinero
- `DineroRetirado`: Se registra cuando se retira dinero

### Agregado: CuentaBancaria
- `Id`: Identificador único de la cuenta
- `Saldo`: Saldo actual (calculado desde eventos)
- `Version`: Número de versión (incrementado con cada evento)

## 🚀 Endpoints de la API

### 1. Crear Cuenta
```
POST /api/cuentas/crear
Content-Type: application/json

{
  "cuentaId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 2. Depositar Dinero
```
POST /api/cuentas/{cuentaId}/depositar
Content-Type: application/json

{
  "monto": 100.50,
  "propietario": "Juan García"
}
```

### 3. Retirar Dinero
```
POST /api/cuentas/{cuentaId}/retirar
Content-Type: application/json

{
  "monto": 50.25,
  "propietario": "Juan García"
}
```

### 4. Obtener Saldo
```
GET /api/cuentas/{cuentaId}/saldo
```

**Respuesta:**
```json
{
  "cuentaId": "550e8400-e29b-41d4-a716-446655440000",
  "saldo": 50.25,
  "version": 2
}
```

## 📦 Dependencias

- **.NET 10**
- **MongoDB.Driver 3.1.0**
- **MongoDB.Bson 3.8.0**
- **Microsoft.AspNetCore.OpenApi 10.0.4**

## 🔧 Configuración

### MongoDB

Asegúrate de que MongoDB esté corriendo. Por defecto, la aplicación se conecta a:
```
mongodb://localhost:27017
```

Para cambiar la cadena de conexión, edita `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MongoDb": "tu-conexion-mongodb"
  }
}
```

## 📚 Estructura del Proyecto

```
Isra.Demos.EventStore/
├── Controllers/
│   └── CuentasController.cs          # Endpoints de la API
├── Models/
│   └── RequestModel.cs               # Modelos de solicitud
├── Services/
│   ├── ICuentaBancariaService.cs    # Interfaz del servicio
│   └── CuentaBancariaService.cs     # Lógica de negocio
├── Repositories/
│   ├── IRepositorioEventos.cs       # Interfaz del repositorio
│   └── RepositorioEventos.cs        # Persistencia de eventos
├── Properties/
│   └── launchSettings.json          # Configuración de inicio
├── Program.cs                        # Configuración de la aplicación
├── Isra.Demos.EventStore.http       # Ejemplos de prueba HTTP
└── appsettings.json                 # Configuración

Isra.Demos.EventSource.Models/
├── EventoBase.cs                     # Clase base para eventos
├── Eventos.cs                        # Definición de eventos (DineroDepositado, DineroRetirado)
└── CuentaBancaria.cs                 # Agregado CuentaBancaria
```

## 🎯 Ejemplo de Uso

1. **Crear una cuenta**
```bash
curl -X POST https://localhost:5001/api/cuentas/crear \
  -H "Content-Type: application/json" \
  -d '{"cuentaId":"550e8400-e29b-41d4-a716-446655440000"}'
```

2. **Depositar dinero**
```bash
curl -X POST https://localhost:5001/api/cuentas/550e8400-e29b-41d4-a716-446655440000/depositar \
  -H "Content-Type: application/json" \
  -d '{"monto":1000,"propietario":"Juan García"}'
```

3. **Retirar dinero**
```bash
curl -X POST https://localhost:5001/api/cuentas/550e8400-e29b-41d4-a716-446655440000/retirar \
  -H "Content-Type: application/json" \
  -d '{"monto":250,"propietario":"Juan García"}'
```

4. **Consultar saldo**
```bash
curl https://localhost:5001/api/cuentas/550e8400-e29b-41d4-a716-446655440000/saldo
```

## ✨ Características de Event Sourcing

- ✅ **Trazabilidad completa**: Todo cambio queda registrado
- ✅ **Reconstrucción de estado**: Se puede recuperar cualquier estado anterior
- ✅ **Auditoría integrada**: El historial de eventos es la auditoría
- ✅ **Escalabilidad**: Fácil de paralelizar y distribuir

## 🧪 Validaciones

- El monto debe ser mayor a 0
- No se permite retirar más dinero del disponible en la cuenta
- Cada evento incrementa la versión de la cuenta
