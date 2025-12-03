using BudgetManagementSystem.Web.Data;
using BudgetManagementSystem.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BudgetManagementSystem.Web.Repositories
{
    /// <summary>
    /// Repository สำหรับจัดการข้อมูลงบประมาณ
    /// ใช้ Pure ADO.NET และ Stored Procedures
    /// ใช้ Parameterized Queries เพื่อป้องกัน SQL Injection
    /// </summary>
    public interface IBudgetRepository
    {
        Task<List<BudgetItem>> GetAllAsync();
        Task<BudgetItem?> GetByIdAsync(int id);
        Task<int> InsertAsync(BudgetItem item);
        Task<int> UpdateAsync(BudgetItem item);
        Task<int> DeleteAsync(int id);
    }

    public class BudgetRepository : IBudgetRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public BudgetRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// ดึงข้อมูลงบประมาณทั้งหมด
        /// </summary>
        public async Task<List<BudgetItem>> GetAllAsync()
        {
            var items = new List<BudgetItem>();

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetAllBudgetItems", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(MapFromReader(reader));
            }

            return items;
        }

        /// <summary>
        /// ดึงข้อมูลงบประมาณตาม ID
        /// </summary>
        public async Task<BudgetItem?> GetByIdAsync(int id)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetBudgetItemById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapFromReader(reader);
            }

            return null;
        }

        /// <summary>
        /// เพิ่มรายการงบประมาณใหม่
        /// </summary>
        public async Task<int> InsertAsync(BudgetItem item)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_InsertBudgetItem", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            AddParametersForInsert(command, item);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// แก้ไขข้อมูลงบประมาณ
        /// </summary>
        public async Task<int> UpdateAsync(BudgetItem item)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_UpdateBudgetItem", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            AddParametersForUpdate(command, item);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// ลบรายการงบประมาณ
        /// </summary>
        public async Task<int> DeleteAsync(int id)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_DeleteBudgetItem", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@Id", id);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        #region Helper Methods

        /// <summary>
        /// แปลงข้อมูลจาก SqlDataReader เป็น BudgetItem
        /// </summary>
        private BudgetItem MapFromReader(SqlDataReader reader)
        {
            return new BudgetItem
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Item = reader.GetString(reader.GetOrdinal("Item")),
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Description")),
                Department = reader.IsDBNull(reader.GetOrdinal("Department")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Department")),
                Division = reader.IsDBNull(reader.GetOrdinal("Division")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Division")),
                Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                ApprovedAmount = reader.GetDecimal(reader.GetOrdinal("ApprovedAmount")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Notes")),
                Benefits = reader.IsDBNull(reader.GetOrdinal("Benefits")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Benefits")),
                Worthiness = reader.IsDBNull(reader.GetOrdinal("Worthiness")) 
                    ? null 
                    : reader.GetString(reader.GetOrdinal("Worthiness")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        /// <summary>
        /// เพิ่ม Parameters สำหรับ Insert
        /// </summary>
        private void AddParametersForInsert(SqlCommand command, BudgetItem item)
        {
            command.Parameters.AddWithValue("@Category", item.Category);
            command.Parameters.AddWithValue("@Item", item.Item);
            command.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Department", (object?)item.Department ?? DBNull.Value);
            command.Parameters.AddWithValue("@Division", (object?)item.Division ?? DBNull.Value);
            command.Parameters.AddWithValue("@Amount", item.Amount);
            command.Parameters.AddWithValue("@ApprovedAmount", item.ApprovedAmount);
            command.Parameters.AddWithValue("@Notes", (object?)item.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@Benefits", (object?)item.Benefits ?? DBNull.Value);
            command.Parameters.AddWithValue("@Worthiness", (object?)item.Worthiness ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", item.Status);
        }

        /// <summary>
        /// เพิ่ม Parameters สำหรับ Update
        /// </summary>
        private void AddParametersForUpdate(SqlCommand command, BudgetItem item)
        {
            command.Parameters.AddWithValue("@Id", item.Id);
            command.Parameters.AddWithValue("@Category", item.Category);
            command.Parameters.AddWithValue("@Item", item.Item);
            command.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Department", (object?)item.Department ?? DBNull.Value);
            command.Parameters.AddWithValue("@Division", (object?)item.Division ?? DBNull.Value);
            command.Parameters.AddWithValue("@Amount", item.Amount);
            command.Parameters.AddWithValue("@ApprovedAmount", item.ApprovedAmount);
            command.Parameters.AddWithValue("@Notes", (object?)item.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@Benefits", (object?)item.Benefits ?? DBNull.Value);
            command.Parameters.AddWithValue("@Worthiness", (object?)item.Worthiness ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", item.Status);
        }

        #endregion
    }
}
