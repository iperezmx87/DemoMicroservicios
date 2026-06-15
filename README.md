# Algunas estadísticas

## Pulls de microservicios
### Portal react
![Docker Pulls](https://img.shields.io/docker/pulls/pesimx87/banco-react?style=for-the-badge&color=blue&logo=docker)

### Servicio cuentas movimientos
![Docker Pulls](https://img.shields.io/docker/pulls/pesimx87/isra-microservicios-cuenta-movimientos?style=for-the-badge&color=blue&logo=docker)

### Servicio recepción transferencias
![Docker Pulls](https://img.shields.io/docker/pulls/pesimx87/isra-microservicios-recepcion-transferencias?style=for-the-badge&color=blue&logo=docker)

### Servicio saldo
![Docker Pulls](https://img.shields.io/docker/pulls/pesimx87/isra-microservicios-saldo?style=for-the-badge&color=blue&logo=docker)

### Servicio estado de cuenta
![Docker Pulls](https://img.shields.io/docker/pulls/pesimx87/isra-microservicios-estado-cuenta?style=for-the-badge&color=blue&logo=docker)

### Vistas del perfil de github
![Vistas](https://komarev.com/ghpvc/?username=iperezmx87&repo=DemoMicroservicios&style=for-the-badge&color=green)

# 🏦 Demo de Microservicios Bancarios - Event Sourcing & CQRS

Este repositorio es una demostración técnica de una arquitectura distribuida de alto rendimiento para la gestión de operaciones bancarias. El proyecto implementa un ecosistema de microservicios utilizando **.NET 10**, enfocado en la trazabilidad absoluta, la inmutabilidad de los datos, la resiliencia del sistema y la **observabilidad distribuida en tiempo real**.

## 🏗️ Arquitectura y Patrones de Diseño

El sistema se basa en una arquitectura **Event-Driven**, separando las responsabilidades de escritura y lectura para optimizar la escalabilidad y la integridad.

### Patrones Implementados:
* **Event Sourcing:** La fuente de verdad reside en un historial inmutable de eventos almacenado en **MongoDB**. Esto permite reconstruir el estado del sistema en cualquier punto del tiempo.
* **CQRS (Command Query Responsibility Segregation):** Modelos de datos independientes para operaciones de escritura y lectura, permitiendo que cada lado escale de forma autónoma.
* **Transactional Outbox:** Garantiza la consistencia entre el Event Store y el Message Broker. Asegura que cada cambio en la base de datos se publique en Kafka, eliminando el problema del "Dual Write".
* **Event-Driven Projections:** Los servicios de lectura (Saldo y Estado de Cuenta) reaccionan de forma coreografiada a los eventos publicados en el bus para actualizar sus propios almacenes de datos.
* **Distributed Tracing (Observabilidad):** Monitoreo unificado de transacciones asíncronas cruzando fronteras de red mediante el estándar de **OpenTelemetry** y el contexto de propagación W3C.

## 🛡️ Resiliencia, Consistencia y Trazabilidad

A diferencia de implementaciones convencionales, este proyecto resuelve retos críticos de los sistemas distribuidos mediante el patrón **Outbox** y el rastreo de transacciones complejas:

1.  **Atomicidad:** Se utilizan **Transacciones de MongoDB** para asegurar que el evento de dominio y el mensaje en el Outbox se persistan como una única unidad atómica.
2.  **At-Least-Once Delivery:** Un servicio en segundo plano (`ProcesadorMensajesSalidaService`) actúa como relay, monitoreando la colección y garantizando la entrega a **Kafka**.
3.  **Idempotencia:** Los consumidores en el lado de lectura (PostgreSQL y SQL Server) están diseñados para procesar mensajes basándose en la versión del evento, evitando inconsistencias por duplicidad.
4.  **Trace Propagation síncrona:** El contexto de telemetría original (`TraceId`) se extrae de la base de datos Mongo, se propaga inyectado en arreglos binarios (`byte[]`) en las cabeceras nativas de **Kafka Message Headers**, y es extraído por el consumidor de lectura para enlazar las consultas de **Dapper**, cerrando el ciclo de monitoreo ciego en procesos asíncronos.

## ✨ Funcionalidades Clave

* **Autenticación y Seguridad:** Implementación de inicio de sesión seguro con tokens JWT (JSON Web Tokens) en el servicio `UsuariosCuentas`, protegiendo de esta manera el acceso a los endpoints restringidos del ecosistema (`WebApi`). *[Características desarrolladas junto a Antigravity (IA de Google DeepMind)]*
* **Gestión Integral de Cuentas:** Capacidad para crear usuarios con validación estricta de unicidad en base de datos relacional antes de la emisión de eventos de dominio.
* **Saga de Transferencias Asíncronas:** Soporte para depósitos, retiros y **transferencias internas** mediante el **Patrón Saga por Coreografía**. Las transferencias dividen su ejecución en dos eventos distribuidos (`TransferenciaRealizadaEvento` y `TransferenciaRecibidaEvento`), procesados de forma asíncrona a través de Kafka por diferentes microservicios (`CuentaMovimientos` y `RecepcionTransferencias`) para evitar el acoplamiento transaccional.
* **Monitoreo con Jaeger y Otel-Collector:** Infraestructura empresarial integrada que unifica trazas, métricas de runtime y logs estructurados bajo un único identificador de correlación transaccional.

## 📊 Observabilidad Distribuida (Músculo Arquitectónico)

El ecosistema entero está instrumentado de manera nativa sin recurrir a llamadas acopladas tradicionales de .NET 10 (evitando errores clásicos de *Top-Level Statements* mediante encapsulamiento estático inmutable). El flujo de datos en el monitor visual se observa así:

* **Petición HTTP Front (React -> WebApi):** Inicia la traza transaccional raíz.
* **Mongo Persistence:** Captura el tiempo de inserción en el EventStore y Outbox.
* **Kafka Boundary:** El Relay captura el mensaje, le inyecta las cabeceras de diagnóstico y calcula la latencia del bus.
* **Consumer & Dapper:** El consumidor revive la traza hija exacta, calculando el impacto de rendimiento de las sentencias SQL en PostgreSQL o SQL Server.

## 🛠️ Stack Tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Runtime** | .NET 10 (C# 14 con Top-Level Statements modernos) |
| **Event Store** | MongoDB (Replica Set para soporte de Transacciones) |
| **Message Broker** | Apache Kafka (Confluent Kafka SDK) |
| **Read Side (Saldo)** | PostgreSQL + Dapper |
| **Read Side (Reportes)** | SQL Server + Dapper |
| **Gestión de Usuarios** | SQL Server + Dapper |
| **Observabilidad** | OpenTelemetry Core SDK + W3C TextMapPropagator |
| **Ingesta de Trazas** | OpenTelemetry Collector Contrib |
| **Visualizador de Telemetría**| Jaeger UI (All-in-one distributed tracing) |
| **Generación de Estado de cuenta**| QuestPDF |

## 🚀 Flujo del Sistema y Telemetría

1.  **Command:** La Web API recibe una instrucción y la persiste en el EventStore de la cuenta origen y la tabla Outbox.
2.  **Relay & Trace Ingestion:** El proceso de despacho (`ProcesadorMensajesSalidaService`) detecta el nuevo registro, extrae el identificador de diagnóstico, abre un **Span de tipo Producer** de OpenTelemetry e inyecta el contexto en el mensaje de Kafka.
3.  **Saga (Para Transferencias):**
    * El servicio `CuentaMovimientos` inicia la transacción debitando los fondos y emitiendo `TransferenciaRealizadaEvento`.
    * El servicio `RecepcionTransferencias` consume dicho evento, valida la cuenta destino y acredita el dinero asíncronamente emitiendo `TransferenciaRecibidaEvento`.
4.  **Proyección de Saldo (Extracción del Contexto):** El `SaldoConsumerService` extrae los bytes del header de Kafka, inicia una **Actividad Hija (Consumer)**, ejecuta logs estructurados correlacionados y actualiza el balance de manera idempotente en **PostgreSQL**.
5.  **Visualización:** El administrador ingresa al portal de Jaeger para auditar el rendimiento exacto de extremo a extremo (Front -> API -> Mongo -> Outbox -> Kafka -> Dapper).

## 📁 Estructura del Proyecto
Isra.Demos.Microservicios.WebApi: Punto de entrada y gestión de Comandos.

Isra.Demos.Microservicios.CuentaMovimientos: Gestión del flujo de movimientos bancarios (Depósitos, Retiros e Inicio de Transferencias) mediante Event Sourcing. Cuenta con instrumentación nativa aislada para evitar bloqueos del compilador en declaraciones superiores.

Isra.Demos.Microservicios.RecepcionTransferencias: Microservicio encargado de recibir y procesar las transferencias entrantes asíncronamente a través del bus Kafka, aplicando validaciones del lado receptor y coordinando el éxito o reverso de la Saga.

Isra.Demos.Microservicios.Saldo: Microservicio de lectura encargado de la proyección y consulta de saldos actuales mediante Dapper, integrado al SDK de rastreo distribuido.

Isra.Demos.Microservicios.EstadoCuenta: Servicio especializado en la generación de reportes e historiales.

Isra.Demos.Microservicios.UsuariosCuentas: API de entrada que gestiona identidades, valida la unicidad de usuarios en base de datos relacional, asigna una cuenta bancaria inicial, y gentiona el flujo de inicio de sesión entregando tokens JWT.

## 🔍 Notas Adicionales sobre el Proyecto
Formato de Solución Extensible: El proyecto hace uso del formato moderno de solución .slnx soportado por las versiones recientes de Visual Studio y el ecosistema .NET, manteniendo la configuración limpia y libre de metadatos XML redundantes.