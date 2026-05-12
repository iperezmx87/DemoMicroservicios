# 🏦 Demo de Microservicios Bancarios - Event Sourcing & CQRS

Este repositorio es una demostración técnica de una arquitectura de microservicios distribuida para la gestión de operaciones bancarias, implementada con **.NET 10** y orientada a eventos.

## 🏗️ Arquitectura y Patrones de Diseño

El proyecto implementa una separación clara entre la escritura y la lectura de datos, garantizando alta escalabilidad y trazabilidad completa.



### Patrones Destacados:
* **Event Sourcing:** El estado actual de una cuenta no se almacena directamente, sino que se reconstruye a partir de una secuencia inmutable de eventos almacenados en **MongoDB**.
* **CQRS (Command Query Responsibility Segregation):** Separación de los modelos de comando (escritura) y consulta (lectura).
* **Event-Driven Architecture:** Comunicación asíncrona entre servicios mediante **Apache Kafka**.
* **Proyecciones de Lectura:** Transformación de eventos en tablas relacionales optimizadas para el usuario final.

---

## 🛠️ Stack Tecnológico

| Capa | Tecnología |
| :--- | :--- |
| **Lenguaje / Runtime** | .NET 10 |
| **Event Store (Escritura)** | MongoDB |
| **Message Broker** | Apache Kafka |
| **Proyección de Saldo** | PostgreSQL (Dapper) |
| **Proyección de Historial** | SQL Server (Dapper) |
| **Reporteo** | QuestPDF |

---

## 🚀 Flujo de Operaciones

1.  **Comando:** El usuario envía una operación (Depósito/Retiro) a la Web API.
2.  **Persistencia de Evento:** El servicio valida la regla de negocio y persiste el evento en **MongoDB**.
3.  **Publicación:** El evento se publica en un tópico de **Kafka**.
4.  **Consumo y Proyección:**
    * **Microservicio de Saldo:** Escucha el evento y actualiza atómicamente el balance en **PostgreSQL**.
    * **Microservicio de Estado de Cuenta:** Registra el movimiento en **SQL Server** para auditoría.
5.  **Consulta:** El front-end consulta el saldo o descarga un **PDF** profesional generado bajo demanda desde las proyecciones de lectura.

---

## 📂 Estructura de la Solución

* `Isra.Demos.Microservicios.Modelo`: Librería de clases con el Agregado de Dominio y los Contratos de Eventos.
* `Isra.Demos.Microservicios.CuentaMovimientos`: Servicio de comandos (Write Side) encargado de la lógica de negocio.
* `Isra.Demos.Microservicios.Saldo`: Background Service que proyecta el saldo actual en **Postgres**.
* `Isra.Demos.Microservicios.EstadoCuenta`: Background Service que gestiona el historial en **SQL Server** y genera reportes PDF.
* `Isra.Demos.Microservicios.WebApi`: Gateway de consulta que expone los endpoints para el Front-end.

---

## 📋 Requisitos y Ejecución

1.  Levantar infraestructura mediante Docker:
    ```bash
    docker-compose up -d
    ```
    *(Asegúrate de tener instancias de Mongo, Kafka, Postgres y SQL Server listas)*.
2.  Ejecutar la solución desde Visual Studio o vía CLI:
    ```bash
    dotnet run --project Isra.Demos.Microservicios.WebApi
    ```

---

## ✨ Características Especiales
* **Idempotencia:** Garantizada mediante el control de versiones de eventos.
* **Resiliencia:** Manejo de reintentos en el consumo de mensajes.
* **Reportes Profesionales:** Generación de estados de cuenta con diseño bancario y soporte multimoneda.

---
Generado por [iperezmx87](https://github.com/iperezmx87) - 2026