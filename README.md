# 🏦 Demo de Microservicios Bancarios - Event Sourcing & CQRS

Este repositorio es una demostración técnica de una arquitectura distribuida para la gestión de operaciones bancarias, construida con **.NET 10**. El sistema destaca por su enfoque en la consistencia de datos y la trazabilidad absoluta.

## 🏗️ Arquitectura y Patrones de Diseño

El sistema utiliza una separación estricta entre la escritura (Commands) y la lectura (Queries), garantizando que las fallas en los reportes no afecten la integridad de las transacciones.

### Patrones Destacados:
* **Event Sourcing:** La "verdad" del sistema reside en un historial inmutable de eventos en **MongoDB**.
* **CQRS:** Modelos de datos independientes para optimizar la velocidad de escritura y la eficiencia de las consultas.
* **Transactional Outbox:** Garantiza que cada cambio en la base de datos se publique en Kafka sin pérdida de mensajes, incluso ante fallas de red.
* **Event-Driven Projections:** Los servicios de lectura (Saldo y Estado de Cuenta) reaccionan de forma coreografiada a los eventos del bus.



---

## 🛡️ Resiliencia y Consistencia (Garantía de Entrega)

A diferencia de implementaciones simples, este proyecto resuelve el problema del "Dual Write" mediante el patrón **Outbox**:

1. **Atomicidad:** Usamos **Transacciones de MongoDB** para asegurar que el evento y el mensaje pendiente se guarden como una única unidad de trabajo.
2. **At-Least-Once Delivery:** Un Background Service (*Outbox Relay*) monitorea la colección y garantiza la publicación en Kafka.
3. **Idempotencia:** Los consumidores (Postgres/SQL) están diseñados para ignorar mensajes duplicados basándose en la versión del evento.

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Runtime** | .NET 10 |
| **Event Store** | MongoDB (Replica Set para Transacciones) |
| **Message Broker** | Apache Kafka |
| **Read Side (Saldo)** | PostgreSQL + Dapper |
| **Read Side (Reportes)** | SQL Server + Dapper |
| **PDF Engine** | QuestPDF |

---

## 🚀 Flujo del Sistema

1. **Command:** La API recibe una transacción y la persiste en el EventStore + Outbox (Mongo).
2. **Relay:** El `OutboxPublisherService` detecta el mensaje y lo entrega a **Kafka**.
3. **Proyección de Saldo:** El consumidor actualiza el balance en tiempo real en **Postgres**.
4. **Proyección de Historial:** El consumidor registra el movimiento en **SQL Server**.
5. **Query:** La Web API expone los datos y genera estados de cuenta bancarios en PDF con formato profesional.

---

## 📋 Requisitos de Ejecución

1. **Infraestructura:** Levantar el entorno con Docker:
   ```bash
   docker-compose up -d