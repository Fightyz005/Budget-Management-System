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
    /// อัพเดทให้รองรับ Department และ Division เป็น text
    /// </summary>
    public interface IBudgetRepository
    {
        Task<List<BudgetItem>> GetAllAsync();
        Task<BudgetItem?> GetByIdAsync(int id);
        Task<int> InsertAsync(BudgetItem item);
        Task<int> UpdateAsync(BudgetItem item);
        Task<int> DeleteAsync(int id);

        Task<int> DeleteFileAsync(int id);
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

        public async Task<int> DeleteFileAsync(int id)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_DeleteBudgetItemFile", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Id", id);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
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
        /// รองรับ Department และ Division เป็น text (ชื่อ)
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
        /// รองรับ Department และ Division เป็น text (ชื่อ)
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
        /// รองรับ Department และ Division เป็น text (NVARCHAR)
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

                // Department และ Division เป็น text (ชื่อ) ไม่ใช่ ID
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
                ProjectType = reader.IsDBNull(reader.GetOrdinal("ProjectType"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("ProjectType")),

                Urgent = reader.IsDBNull(reader.GetOrdinal("Urgent"))
                    ? null
                    : reader.GetString(reader.GetOrdinal("Urgent")),

                StartDate = reader.IsDBNull(reader.GetOrdinal("StartDate"))
                    ? (DateTime?)null
                    : reader.GetDateTime(reader.GetOrdinal("StartDate")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),

                FileName = reader.IsDBNull(reader.GetOrdinal("FileName"))
                    ? null : reader.GetString(reader.GetOrdinal("FileName")),
                        FileSize = reader.IsDBNull(reader.GetOrdinal("FileSize"))
                    ? null : reader.GetInt64(reader.GetOrdinal("FileSize")),
                        FileExtension = reader.IsDBNull(reader.GetOrdinal("FileExtension"))
                    ? null : reader.GetString(reader.GetOrdinal("FileExtension")),
                        FileUploadDate = reader.IsDBNull(reader.GetOrdinal("FileUploadDate"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("FileUploadDate"))
            };
        }

        /// <summary>
        /// เพิ่ม Parameters สำหรับ Insert
        /// Department และ Division รับเป็น text (ชื่อ) ไม่ใช่ ID
        /// </summary>
        private void AddParametersForInsert(SqlCommand command, BudgetItem item)
        {
            command.Parameters.AddWithValue("@Category", item.Category);
            command.Parameters.AddWithValue("@Item", item.Item);
            command.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);

            // บันทึก Department และ Division เป็นชื่อ (text)
            command.Parameters.AddWithValue("@Department",
                string.IsNullOrEmpty(item.Department) ? (object)DBNull.Value : item.Department);
            command.Parameters.AddWithValue("@Division",
                string.IsNullOrEmpty(item.Division) ? (object)DBNull.Value : item.Division);

            command.Parameters.AddWithValue("@Amount", item.Amount);
            command.Parameters.AddWithValue("@ApprovedAmount", item.ApprovedAmount);
            command.Parameters.AddWithValue("@Notes", (object?)item.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@Benefits", (object?)item.Benefits ?? DBNull.Value);
            command.Parameters.AddWithValue("@Worthiness", (object?)item.Worthiness ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", item.Status);
            command.Parameters.AddWithValue("@ProjectType", (object?)item.ProjectType ?? DBNull.Value);
            command.Parameters.AddWithValue("@Urgent", (object?)item.Urgent ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", (object?)item.StartDate ?? DBNull.Value);

            // ✅ เพิ่มใน AddParametersForInsert และ AddParametersForUpdate
            command.Parameters.AddWithValue("@FileName", (object?)item.FileName ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileSize", (object?)item.FileSize ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileExtension", (object?)item.FileExtension ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileUploadDate", (object?)item.FileUploadDate ?? DBNull.Value);
        }

        /// <summary>
        /// เพิ่ม Parameters สำหรับ Update
        /// Department และ Division รับเป็น text (ชื่อ) ไม่ใช่ ID
        /// </summary>
        private void AddParametersForUpdate(SqlCommand command, BudgetItem item)
        {
            command.Parameters.AddWithValue("@Id", item.Id);
            command.Parameters.AddWithValue("@Category", item.Category);
            command.Parameters.AddWithValue("@Item", item.Item);
            command.Parameters.AddWithValue("@Description", (object?)item.Description ?? DBNull.Value);

            // บันทึก Department และ Division เป็นชื่อ (text)
            command.Parameters.AddWithValue("@Department",
                string.IsNullOrEmpty(item.Department) ? (object)DBNull.Value : item.Department);
            command.Parameters.AddWithValue("@Division",
                string.IsNullOrEmpty(item.Division) ? (object)DBNull.Value : item.Division);

            command.Parameters.AddWithValue("@Amount", item.Amount);
            command.Parameters.AddWithValue("@ApprovedAmount", item.ApprovedAmount);
            command.Parameters.AddWithValue("@Notes", (object?)item.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@Benefits", (object?)item.Benefits ?? DBNull.Value);
            command.Parameters.AddWithValue("@Worthiness", (object?)item.Worthiness ?? DBNull.Value);
            command.Parameters.AddWithValue("@Status", item.Status);
            command.Parameters.AddWithValue("@ProjectType", (object?)item.ProjectType ?? DBNull.Value);
            command.Parameters.AddWithValue("@Urgent", (object?)item.Urgent ?? DBNull.Value);
            command.Parameters.AddWithValue("@StartDate", (object?)item.StartDate ?? DBNull.Value);

            // ✅ เพิ่มใน AddParametersForInsert และ AddParametersForUpdate
            command.Parameters.AddWithValue("@FileName", (object?)item.FileName ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileSize", (object?)item.FileSize ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileExtension", (object?)item.FileExtension ?? DBNull.Value);
            command.Parameters.AddWithValue("@FileUploadDate", (object?)item.FileUploadDate ?? DBNull.Value);
        }

        #endregion
    }
}