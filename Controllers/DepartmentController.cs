using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BudgetManagementSystem.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DepartmentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = new List<object>();
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT [id], [departmentName] FROM [dbo].[Departments] ORDER BY [departmentName]", connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                departments.Add(new
                                {
                                    id = reader.GetInt32(0),
                                    name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("divisions/{departmentId}")]
        public async Task<IActionResult> GetDivisions(int departmentId)
        {
            try
            {
                var divisions = new List<object>();
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT [id], [divisionName] FROM [dbo].[Divisions] WHERE [departmentId] = @DepartmentId ORDER BY [divisionName]", 
                        connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentId", departmentId);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                divisions.Add(new
                                {
                                    id = reader.GetInt32(0),
                                    name = reader.GetString(1)
                                });
                            }
                        }
                    }
                }

                return Ok(divisions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("department-name/{departmentId}")]
        public async Task<IActionResult> GetDepartmentName(int departmentId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT [departmentName] FROM [dbo].[Departments] WHERE [id] = @DepartmentId", 
                        connection))
                    {
                        command.Parameters.AddWithValue("@DepartmentId", departmentId);
                        
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return Ok(new { name = result.ToString() });
                        }
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("division-name/{divisionId}")]
        public async Task<IActionResult> GetDivisionName(int divisionId)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT [divisionName] FROM [dbo].[Divisions] WHERE [id] = @DivisionId", 
                        connection))
                    {
                        command.Parameters.AddWithValue("@DivisionId", divisionId);
                        
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            return Ok(new { name = result.ToString() });
                        }
                        return NotFound();
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
