using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BudgetManagementSystem.Web.Data
{
    /// <summary>
    /// Factory class สำหรับจัดการ SQL Server Connection
    /// ใช้ Parameterized Queries เพื่อป้องกัน SQL Injection
    /// </summary>
    public class SqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Connection string cannot be null");
        }

        /// <summary>
        /// สร้าง SQL Connection ใหม่
        /// </summary>
        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            return connection;
        }

        /// <summary>
        /// สร้าง SQL Connection พร้อมเปิดการเชื่อมต่อ
        /// </summary>
        public async Task<SqlConnection> CreateOpenConnectionAsync()
        {
            var connection = CreateConnection();
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// ทดสอบการเชื่อมต่อ Database
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = CreateConnection();
                await connection.OpenAsync();
                return connection.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }
    }
}
