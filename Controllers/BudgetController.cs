using BudgetManagementSystem.Web.Models;
using BudgetManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับจัดการงบประมาณ
    /// </summary>
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(
            IBudgetService budgetService,
            ILogger<BudgetController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        /// <summary>
        /// แสดงรายการงบประมาณทั้งหมด
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var items = await _budgetService.GetAllBudgetItemsAsync();
                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading budget items");
                return View("Error");
            }
        }

        /// <summary>
        /// แสดงรายละเอียดงบประมาณ
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var item = await _budgetService.GetBudgetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading budget item details");
                return View("Error");
            }
        }

        /// <summary>
        /// แสดงฟอร์มสร้างงบประมาณใหม่
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// บันทึกงบประมาณใหม่
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetItem item)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _budgetService.CreateBudgetItemAsync(item);
                    TempData["SuccessMessage"] = "สร้างรายการงบประมาณสำเร็จ";
                    return RedirectToAction(nameof(Index));
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget item");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการสร้างรายการ");
                return View(item);
            }
        }

        /// <summary>
        /// แสดงฟอร์มแก้ไขงบประมาณ
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var item = await _budgetService.GetBudgetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading budget item for edit");
                return View("Error");
            }
        }

        /// <summary>
        /// บันทึกการแก้ไขงบประมาณ
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BudgetItem item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _budgetService.UpdateBudgetItemAsync(item);
                    TempData["SuccessMessage"] = "แก้ไขรายการงบประมาณสำเร็จ";
                    return RedirectToAction(nameof(Index));
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating budget item");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการแก้ไขรายการ");
                return View(item);
            }
        }

        /// <summary>
        /// แสดงหน้ายืนยันการลบ
        /// </summary>
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var item = await _budgetService.GetBudgetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }
                return View(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading budget item for delete");
                return View("Error");
            }
        }

        /// <summary>
        /// ลบงบประมาณ
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _budgetService.DeleteBudgetItemAsync(id);
                TempData["SuccessMessage"] = "ลบรายการงบประมาณสำเร็จ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting budget item");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลบรายการ";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
