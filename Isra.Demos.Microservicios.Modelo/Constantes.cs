namespace Isra.Demos.Microservicios.Modelo
{
    /// <summary>
    /// Clase que contiene constantes utilizadas en la aplicación, como la configuración de Kafka y los nombres de los tópicos.
    /// </summary>
    public static class Constantes
    {
        /// <summary>
        /// MongoDB Connection String, en este caso se asume que MongoDB se está ejecutando localmente en el puerto 27017. Esta cadena de conexión es fundamental para establecer la comunicación entre la aplicación y la base de datos MongoDB, permitiendo así el almacenamiento y recuperación de eventos relacionados con el Event Sourcing. Al utilizar una cadena de conexión local, se facilita el desarrollo y las pruebas de la aplicación en un entorno controlado, aunque en un entorno de producción se recomendaría utilizar una configuración más robusta y segura para la conexión a la base de datos.
        /// </summary>
        public const string MongoDbConnectionString = "mongodb://localhost:27017";

        /// <summary>
        /// Nombre de la base de datos en MongoDB donde se almacenarán los eventos. En este caso, se ha definido como "BDEventSource", lo que indica que esta base de datos se utilizará específicamente para almacenar los eventos relacionados con el Event Sourcing en la aplicación. Al centralizar los eventos en una base de datos dedicada, se facilita la gestión y el acceso a los eventos, lo que es fundamental para la reconstrucción del estado de los agregados y para el análisis histórico de las acciones realizadas en el sistema.
        /// </summary>
        public const string EventStoreDatabaseName = "bd_cuentas_movimientos";

        /// <summary>
        /// Nombre de la colección en MongoDB donde se almacenarán los eventos de salida. Esta colección se utilizará para guardar todos los eventos relacionados con el Event Sourcing, permitiendo así la reconstrucción del estado de los agregados y el análisis histórico de las acciones realizadas en el sistema.
        /// </summary>
        public const string CuentasMovimientosCollectionName = "cuentas_movimientos_outbox";

        /// <summary>
        /// Nombre de la colección en MongoDB donde se almacenarán los eventos. Esta colección se utilizará para guardar todos los eventos relacionados con el Event Sourcing, permitiendo así la reconstrucción del estado de los agregados y el análisis histórico de las acciones realizadas en el sistema.
        /// </summary>
        public const string EventStoreCollectionName = "cuentas_movimientos";

        /// <summary>
        /// Url de Kafka, en este caso se asume que Kafka se está ejecutando localmente en el puerto 9092.
        /// </summary>
        public const string KafkaBootstrapServers = "localhost:9092";

        /// <summary>
        /// Principal topico de Kafka donde se publicarán los eventos relacionados con las transacciones de cuenta.
        /// </summary>
        public const string KafkaTopic = "cuentas_movimientos_eventos";

        /// <summary>
        /// Cadena de conexión a PostgreSQL, en este caso se asume que PostgreSQL se está ejecutando localmente en el puerto 5432, con una base de datos llamada "BDMicroservicios" y credenciales de usuario "postgres" con contraseña "postgres". Esta cadena de conexión es fundamental para establecer la comunicación entre la aplicación y la base de datos PostgreSQL, permitiendo así el almacenamiento y recuperación de información relacionada con las transacciones de cuenta y otros datos relevantes para el funcionamiento del sistema. Al utilizar una cadena de conexión local, se facilita el desarrollo y las pruebas de la aplicación en un entorno controlado, aunque en un entorno de producción se recomendaría utilizar una configuración más robusta y segura para la conexión a la base de datos.
        /// </summary>
        public const string PostgresConnectionString = "Host=localhost;Port=5432;Database=bd_saldos_cuentas;Username=postgres;Password=Saucedo870824";

        /// <summary>
        /// Cadena de conexión a SQL Server, en este caso se asume que SQL Server se está ejecutando localmente en el puerto 1433, con una base de datos llamada "BDESTADOCUENTAS" y credenciales de usuario "sa" con contraseña "Saucedo870824". Esta cadena de conexión es fundamental para establecer la comunicación entre la aplicación y la base de datos SQL Server, permitiendo así el almacenamiento y recuperación de información relacionada con las transacciones de cuenta y otros datos relevantes para el funcionamiento del sistema. Al utilizar una cadena de conexión local, se facilita el desarrollo y las pruebas de la aplicación en un entorno controlado, aunque en un entorno de producción se recomendaría utilizar una configuración más robusta y segura para la conexión a la base de datos.
        /// </summary>
        public const string SQLServerConnectionString = "Server=.\\SQLEXPRESS;Database=BDESTADOCUENTA;user id=sa;password=Saucedo870824;TrustServerCertificate=True;";

        /// <summary>
        /// Cadena de conexión a SQL Server para la base de datos de cuentas, en este caso se asume que SQL Server se está ejecutando localmente en el puerto 1433, con una base de datos llamada "BDBANCOCUENTAS" y credenciales de usuario "sa" con contraseña "Saucedo870824". Esta cadena de conexión es fundamental para establecer la comunicación entre la aplicación y la base de datos SQL Server, permitiendo así el almacenamiento y recuperación de información relacionada con las cuentas bancarias y otros datos relevantes para el funcionamiento del sistema. Al utilizar una cadena de conexión local, se facilita el desarrollo y las pruebas de la aplicación en un entorno controlado, aunque en un entorno de producción se recomendaría utilizar una configuración más robusta y segura para la conexión a la base de datos.
        /// </summary>
        public const string SqlServerBancoCuentasConnectionString = "Server=.\\SQLEXPRESS;Database=BDBANCOCUENTAS;user id=sa;password=Saucedo870824;TrustServerCertificate=True;";
    }
}
