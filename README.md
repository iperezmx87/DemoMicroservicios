# 🏦 Demo de Microservicios Bancarios - Event Sourcing & CQRS

Este repositorio es una demostración técnica de una arquitectura distribuida de alto rendimiento para la gestión de operaciones bancarias. El proyecto implementa un ecosistema de microservicios utilizando **.NET 10**, enfocado en la trazabilidad absoluta, la inmutabilidad de los datos y la resiliencia del sistema.

## 🏗️ Arquitectura y Patrones de Diseño

El sistema se basa en una arquitectura **Event-Driven**, separando las responsabilidades de escritura y lectura para optimizar la escalabilidad y la integridad.

### Patrones Implementados:
* **Event Sourcing:** La fuente de verdad reside en un historial inmutable de eventos almacenado en **MongoDB**. Esto permite reconstruir el estado del sistema en cualquier punto del tiempo.
* **CQRS (Command Query Responsibility Segregation):** Modelos de datos independientes para operaciones de escritura y lectura, permitiendo que cada lado escale de forma autónoma.
* **Transactional Outbox:** Garantiza la consistencia entre el Event Store y el Message Broker. Asegura que cada cambio en la base de datos se publique en Kafka, eliminando el problema del "Dual Write".
* **Event-Driven Projections:** Los servicios de lectura (Saldo y Estado de Cuenta) reaccionan de forma coreografiada a los eventos publicados en el bus para actualizar sus propios almacenes de datos.

## 🛡️ Resiliencia y Consistencia (Garantía de Entrega)

A diferencia de implementaciones convencionales, este proyecto resuelve retos críticos de los sistemas distribuidos mediante el patrón **Outbox**:

1.  **Atomicidad:** Se utilizan **Transacciones de MongoDB** para asegurar que el evento de dominio y el mensaje en el Outbox se persistan como una única unidad atómica.
2.  **At-Least-Once Delivery:** Un servicio en segundo plano (`OutboxPublisherService`) actúa como relay, monitoreando la colección y garantizando la entrega a **Kafka**.
3.  **Idempotencia:** Los consumidores en el lado de lectura (PostgreSQL y SQL Server) están diseñados para procesar mensajes basándose en la versión del evento, evitando inconsistencias por duplicidad.

## 🛠️ Stack Tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Runtime** | .NET 10 |
| **Event Store** | MongoDB (Replica Set para soporte de Transacciones) |
| **Message Broker** | Apache Kafka |
| **Read Side (Saldo)** | PostgreSQL + Dapper |
| **Read Side (Reportes)** | SQL Server + Dapper |
| **Generación de Documentos** | QuestPDF |

## 🚀 Flujo del Sistema

1.  **Command:** La Web API recibe una instrucción (ej. un nuevo movimiento) y la persiste en el EventStore y la tabla Outbox de forma atómica.
2.  **Relay:** El proceso de despacho detecta el nuevo registro y lo publica en el tópico correspondiente de Kafka.
3.  **Proyección de Saldo:** Un consumidor procesa el evento y actualiza el balance en tiempo real en **PostgreSQL**.
4.  **Proyección de Historial:** Simultáneamente, otro consumidor registra el movimiento detallado en **SQL Server**.
5.  **Query:** El usuario consulta su estado de cuenta; el sistema recupera los datos proyectados y genera un PDF profesional mediante **QuestPDF**.

## 📁 Estructura del Proyecto

* **`Isra.Demos.Microservicios.WebApi`**: Punto de entrada y gestión de Comandos.
* **`Isra.Demos.Microservicios.CuentaMovimientos`**: Gestión del flujo de movimientos bancarios.
* **`Isra.Demos.Microservicios.Saldo`**: Microservicio encargado de la proyección y consulta de saldos actuales.
* **`Isra.Demos.Microservicios.EstadoCuenta`**: Servicio especializado en la generación de reportes y documentos.
* **`Isra.Demos.Microservicios.Modelo`**: Biblioteca de clases compartida y definiciones de dominio.
* **`Isra.Demos.Microservicios.UsuariosCuentas`**: Api de entrada que crea las cuentas de usuario y asigna una cuenta bancaria para las operaciones

## 📋 Requisitos e Instalación

1.  **Infraestructura:** El proyecto incluye un archivo `docker-compose.yml` para levantar la infraestructura base.
    ```bash
    docker-compose up -d
    ```
2.  **Configuración:** Asegurarse de que el Replica Set de MongoDB esté activo para permitir el uso de transacciones multi-documento.

## 🔍 Notas Adicionales sobre el Proyecto

*   **Infraestructura en Docker:** Actualmente, el archivo `docker-compose.yml` incluye únicamente la configuración para **Apache Kafka** (en modo KRaft). Será necesario agregar los contenedores para MongoDB, PostgreSQL y SQL Server o aprovisionarlos externamente.
*   **Formato de Solución:** El proyecto hace uso del formato moderno de solución `.slnx` soportado por las versiones recientes de Visual Studio y el ecosistema .NET, manteniendo la configuración limpia y minimalista.