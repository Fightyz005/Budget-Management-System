using BudgetManagementSystem.Web.Models;
using BudgetManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BudgetManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับหน้าแรกและ Dashboard
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IBudgetService budgetService,
            ILogger<HomeController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        /// <summary>
        /// หน้าแรก - Dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await _budgetService.GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View("Error");
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
