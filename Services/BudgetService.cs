using BudgetManagementSystem.Web.Data;
using BudgetManagementSystem.Web.Models;
using BudgetManagementSystem.Web.Repositories;
using BudgetManagementSystem.Web.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BudgetManagementSystem.Web.Services
{
    /// <summary>
    /// Service สำหรับจัดการ Business Logic ของงบประมาณ
    /// </summary>
    public interface IBudgetService
    {
        Task<List<BudgetItem>> GetAllBudgetItemsAsync();
        Task<BudgetItem?> GetBudgetItemByIdAsync(int id);
        Task<int> CreateBudgetItemAsync(BudgetItem item);
        Task<int> UpdateBudgetItemAsync(BudgetItem item);
        Task<int> DeleteBudgetItemAsync(int id);
        Task<BudgetStatisticsViewModel> GetStatisticsAsync();
        Task<List<BudgetByDepartmentViewModel>> GetBudgetByDepartmentAsync();
        Task<List<BudgetByCategoryViewModel>> GetBudgetByCategoryAsync();
        Task<DashboardViewModel> GetDashboardDataAsync();
    }

    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly SqlConnectionFactory _connectionFactory;

        public BudgetService(
            IBudgetRepository budgetRepository,
            SqlConnectionFactory connectionFactory)
        {
            _budgetRepository = budgetRepository;
            _connectionFactory = connectionFactory;
        }

        public async Task<List<BudgetItem>> GetAllBudgetItemsAsync()
        {
            return await _budgetRepository.GetAllAsync();
        }

        public async Task<BudgetItem?> GetBudgetItemByIdAsync(int id)
        {
            return await _budgetRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateBudgetItemAsync(BudgetItem item)
        {
            // Business logic validation
            if (item.Amount < 0)
                throw new ArgumentException("จำนวนเงินต้องมากกว่า 0");

            return await _budgetRepository.InsertAsync(item);
        }

        public async Task<int> UpdateBudgetItemAsync(BudgetItem item)
        {
            // Business logic validation
            if (item.Amount < 0)
                throw new ArgumentException("จำนวนเงินต้องมากกว่า 0");

            return await _budgetRepository.UpdateAsync(item);
        }

        public async Task<int> DeleteBudgetItemAsync(int id)
        {
            return await _budgetRepository.DeleteAsync(id);
        }

        /// <summary>
        /// ดึงสถิติงบประมาณ
        /// </summary>
        public async Task<BudgetStatisticsViewModel> GetStatisticsAsync()
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetBudgetStatistics", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new BudgetStatisticsViewModel
                {
                    TotalProposed = reader.IsDBNull(reader.GetOrdinal("TotalProposed")) 
                        ? 0 
                        : reader.GetDecimal(reader.GetOrdinal("TotalProposed")),
                    TotalApproved = reader.IsDBNull(reader.GetOrdinal("TotalApproved")) 
                        ? 0 
                        : reader.GetDecimal(reader.GetOrdinal("TotalApproved")),
                    TotalItems = reader.GetInt32(reader.GetOrdinal("TotalItems")),
                    ApprovedItems = reader.GetInt32(reader.GetOrdinal("ApprovedItems")),
                    ProposedItems = reader.GetInt32(reader.GetOrdinal("ProposedItems")),
                    RejectedItems = reader.GetInt32(reader.GetOrdinal("RejectedItems"))
                };
            }

            return new BudgetStatisticsViewModel();
        }

        /// <summary>
        /// สรุปงบประมาณตามสายงาน
        /// </summary>
        public async Task<List<BudgetByDepartmentViewModel>> GetBudgetByDepartmentAsync()
        {
            var result = new List<BudgetByDepartmentViewModel>();

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetBudgetByDepartment", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new BudgetByDepartmentViewModel
                {
                    Department = reader.GetString(reader.GetOrdinal("Department")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    TotalApproved = reader.GetDecimal(reader.GetOrdinal("TotalApproved")),
                    ItemCount = reader.GetInt32(reader.GetOrdinal("ItemCount"))
                });
            }

            return result;
        }

        /// <summary>
        /// สรุปงบประมาณตามหมวดหมู่
        /// </summary>
        public async Task<List<BudgetByCategoryViewModel>> GetBudgetByCategoryAsync()
        {
            var result = new List<BudgetByCategoryViewModel>();

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetBudgetByCategory", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new BudgetByCategoryViewModel
                {
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                    TotalApproved = reader.GetDecimal(reader.GetOrdinal("TotalApproved")),
                    ItemCount = reader.GetInt32(reader.GetOrdinal("ItemCount"))
                });
            }

            return result;
        }

        /// <summary>
        /// ดึงข้อมูลสำหรับ Dashboard
        /// </summary>
        public async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var statistics = await GetStatisticsAsync();
            var departmentBudgets = await GetBudgetByDepartmentAsync();
            var categoryBudgets = await GetBudgetByCategoryAsync();
            var budgetItems = await GetAllBudgetItemsAsync();

            return new DashboardViewModel
            {
                Statistics = statistics,
                DepartmentBudgets = departmentBudgets,
                CategoryBudgets = categoryBudgets,
                BudgetItems = budgetItems
            };
        }
    }
}
