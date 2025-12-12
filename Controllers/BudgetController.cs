using BudgetManagementSystem.Web.Models;
using BudgetManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับจัดการงบประมาณ - FIXED VERSION with File Upload
    /// </summary>
    public class BudgetController : Controller
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetController> _logger;
        private readonly IWebHostEnvironment _environment; // ✅ NEW

        public BudgetController(
            IBudgetService budgetService,
            ILogger<BudgetController> logger,
            IWebHostEnvironment environment) // ✅ NEW
        {
            _budgetService = budgetService;
            _logger = logger;
            _environment = environment; // ✅ NEW
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
        /// แสดงฟอร์มสร้างงบประมาณใหม่ (GET)
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            Console.WriteLine("=== CREATE GET - Showing Form ===");
            return View(new BudgetItem());
        }

        /// <summary>
        /// บันทึกงบประมาณใหม่ (POST) - Manual Form Binding with File Upload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            try
            {
                Console.WriteLine("=== CREATE POST ACTION START ===");

                // อ่านค่าจาก Form โดยตรง
                var category = Request.Form["Category"].ToString();
                var item = Request.Form["Item"].ToString();
                var description = Request.Form["Description"].ToString();
                var department = Request.Form["Department"].ToString();
                var division = Request.Form["Division"].ToString();
                var amountStr = Request.Form["Amount"].ToString();
                var approvedAmountStr = Request.Form["ApprovedAmount"].ToString();
                var notes = Request.Form["Notes"].ToString();
                var benefits = Request.Form["Benefits"].ToString();
                var worthiness = Request.Form["Worthiness"].ToString();
                var status = Request.Form["Status"].ToString();
                var projectType = Request.Form["ProjectType"].ToString();
                var urgent = Request.Form["Urgent"].ToString();
                var startDate = Request.Form["StartDate"].ToString();

                // ✅ NEW: อ่านไฟล์จาก Form
                IFormFile? uploadedFile = Request.Form.Files.GetFile("UploadedFile");

                // Validation
                if (string.IsNullOrWhiteSpace(category))
                {
                    ModelState.AddModelError("Category", "กรุณาเลือกหมวดหมู่");
                    Console.WriteLine("❌ Category is empty");
                }
                if (string.IsNullOrWhiteSpace(item))
                {
                    ModelState.AddModelError("Item", "กรุณาระบุชื่อรายการ");
                    Console.WriteLine("❌ Item is empty");
                }

                decimal amount = 0;
                if (string.IsNullOrWhiteSpace(amountStr) || !decimal.TryParse(amountStr, out amount) || amount <= 0)
                {
                    ModelState.AddModelError("Amount", "กรุณาระบุจำนวนเงินที่ถูกต้อง");
                    Console.WriteLine("❌ Amount is invalid");
                }

                if (string.IsNullOrWhiteSpace(projectType))
                {
                    ModelState.AddModelError("ProjectType", "กรุณาระบุประเภทโครงการ");
                }

                if (string.IsNullOrWhiteSpace(urgent))
                {
                    ModelState.AddModelError("Urgent", "กรุณาระบุความเร่งด่วน");
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ModelState Invalid ===");
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state != null && state.Errors.Count > 0)
                        {
                            foreach (var error in state.Errors)
                            {
                                Console.WriteLine($"  Key: {key}, Error: {error.ErrorMessage}");
                            }
                        }
                    }

                    // สร้าง model เพื่อส่งกลับไปแสดง
                    var errorModel = new BudgetItem
                    {
                        Category = category ?? "",
                        Item = item ?? "",
                        Description = description,
                        Department = department,
                        Division = division,
                        Amount = amount,
                        ApprovedAmount = decimal.TryParse(approvedAmountStr, out var approvedAmt) ? approvedAmt : 0,
                        Notes = notes,
                        Benefits = benefits,
                        Worthiness = worthiness,
                        Status = status ?? "proposed",
                        ProjectType = projectType,
                        Urgent = urgent,
                        StartDate = DateTime.TryParse(startDate, out var d) ? d : (DateTime?)null
                    };

                    return View("Create", errorModel);
                }

                // สร้าง BudgetItem object
                var budgetItem = new BudgetItem
                {
                    Category = category,
                    Item = item,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Department = string.IsNullOrWhiteSpace(department) ? null : department,
                    Division = string.IsNullOrWhiteSpace(division) ? null : division,
                    Amount = amount,
                    ApprovedAmount = decimal.TryParse(approvedAmountStr, out var approvedAmount) ? approvedAmount : 0,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    Benefits = string.IsNullOrWhiteSpace(benefits) ? null : benefits,
                    Worthiness = string.IsNullOrWhiteSpace(worthiness) ? null : worthiness,
                    Status = string.IsNullOrWhiteSpace(status) ? "proposed" : status,
                    ProjectType = string.IsNullOrWhiteSpace(projectType) ? null : projectType,
                    Urgent = string.IsNullOrWhiteSpace(urgent) ? null : urgent,
                    StartDate = DateTime.TryParse(startDate, out var dt) ? dt : (DateTime?)null
                };

                // ✅ NEW: จัดการไฟล์ที่อัปโหลด
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    Console.WriteLine($"📎 File uploaded: {uploadedFile.FileName} ({uploadedFile.Length} bytes)");
                    var uploadResult = await SaveUploadedFileAsync(uploadedFile);
                    if (uploadResult.Success)
                    {
                        budgetItem.FileName = uploadResult.FileName;
                        budgetItem.FileSize = uploadResult.FileSize;
                        budgetItem.FileExtension = uploadResult.FileExtension;
                        budgetItem.FileUploadDate = DateTime.Now;
                        Console.WriteLine($"✅ File saved: {uploadResult.FileName}");
                    }
                    else
                    {
                        ModelState.AddModelError("UploadedFile", uploadResult.ErrorMessage!);
                        return View("Create", budgetItem);
                    }
                }

                Console.WriteLine($"✅ Created BudgetItem object: {budgetItem.Category} - {budgetItem.Item}");

                await _budgetService.CreateBudgetItemAsync(budgetItem);

                Console.WriteLine("✅ Budget item saved successfully");
                TempData["SuccessMessage"] = "สร้างรายการงบประมาณสำเร็จ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                _logger.LogError(ex, "Error creating budget item");
                ModelState.AddModelError("", $"เกิดข้อผิดพลาดในการสร้างรายการ: {ex.Message}");
                return View("Create", new BudgetItem());
            }
        }

        /// <summary>
        /// แสดงฟอร์มแก้ไขงบประมาณ (GET)
        /// </summary>
        [HttpGet]
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
        /// บันทึกการแก้ไขงบประมาณ (POST) - Manual Form Binding with File Upload
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id)
        {
            try
            {
                Console.WriteLine($"=== EDIT POST ACTION START - ID: {id} ===");

                // อ่านค่าจาก Form โดยตรง
                var category = Request.Form["Category"].ToString();
                var item = Request.Form["Item"].ToString();
                var description = Request.Form["Description"].ToString();
                var department = Request.Form["Department"].ToString();
                var division = Request.Form["Division"].ToString();
                var amountStr = Request.Form["Amount"].ToString();
                var approvedAmountStr = Request.Form["ApprovedAmount"].ToString();
                var notes = Request.Form["Notes"].ToString();
                var benefits = Request.Form["Benefits"].ToString();
                var worthiness = Request.Form["Worthiness"].ToString();
                var status = Request.Form["Status"].ToString();
                var projectType = Request.Form["ProjectType"].ToString();
                var urgent = Request.Form["Urgent"].ToString();
                var startDate = Request.Form["StartDate"];

                // ✅ NEW: อ่านข้อมูลไฟล์เดิมและไฟล์ใหม่
                var existingFileName = Request.Form["FileName"].ToString();
                var existingFileSizeStr = Request.Form["FileSize"].ToString();
                var existingFileExtension = Request.Form["FileExtension"].ToString();
                var existingFileUploadDateStr = Request.Form["FileUploadDate"].ToString();
                IFormFile? uploadedFile = Request.Form.Files.GetFile("UploadedFile");

                Console.WriteLine($"Category: '{category}'");
                Console.WriteLine($"Item: '{item}'");

                // Validation
                decimal amount = 0;
                if (string.IsNullOrWhiteSpace(category))
                {
                    ModelState.AddModelError("Category", "กรุณาเลือกหมวดหมู่");
                }
                if (string.IsNullOrWhiteSpace(item))
                {
                    ModelState.AddModelError("Item", "กรุณาระบุชื่อรายการ");
                }
                if (string.IsNullOrWhiteSpace(amountStr) || !decimal.TryParse(amountStr, out amount) || amount <= 0)
                {
                    ModelState.AddModelError("Amount", "กรุณาระบุจำนวนเงินที่ถูกต้อง");
                }

                if (string.IsNullOrWhiteSpace(projectType))
                {
                    ModelState.AddModelError("ProjectType", "กรุณาระบุประเภทโครงการ");
                }

                if (string.IsNullOrWhiteSpace(urgent))
                {
                    ModelState.AddModelError("Urgent", "กรุณาระบุความเร่งด่วน");
                }

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("=== ModelState Invalid ===");
                    // โหลดข้อมูลเดิมเพื่อแสดง error
                    var existingItem = await _budgetService.GetBudgetItemByIdAsync(id);
                    if (existingItem == null)
                    {
                        return NotFound();
                    }
                    return View("Edit", existingItem);
                }

                // สร้าง BudgetItem object พร้อม ID
                var budgetItem = new BudgetItem
                {
                    Id = id,
                    Category = category,
                    Item = item,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Department = string.IsNullOrWhiteSpace(department) ? null : department,
                    Division = string.IsNullOrWhiteSpace(division) ? null : division,
                    Amount = amount,
                    ApprovedAmount = decimal.TryParse(approvedAmountStr, out var approvedAmount) ? approvedAmount : 0,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    Benefits = string.IsNullOrWhiteSpace(benefits) ? null : benefits,
                    Worthiness = string.IsNullOrWhiteSpace(worthiness) ? null : worthiness,
                    Status = string.IsNullOrWhiteSpace(status) ? "proposed" : status,
                    ProjectType = string.IsNullOrWhiteSpace(projectType) ? null : projectType,
                    Urgent = string.IsNullOrWhiteSpace(urgent) ? null : urgent,
                    StartDate = DateTime.TryParse(startDate, out var dt) ? dt : (DateTime?)null,
                    // ✅ NEW: เก็บข้อมูลไฟล์เดิมไว้
                    FileName = existingFileName,
                    FileSize = long.TryParse(existingFileSizeStr, out var fSize) ? fSize : (long?)null,
                    FileExtension = existingFileExtension,
                    FileUploadDate = DateTime.TryParse(existingFileUploadDateStr, out var fDate) ? fDate : (DateTime?)null
                };

                // ✅ NEW: จัดการไฟล์ใหม่ (ถ้ามี)
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    Console.WriteLine($"📎 New file uploaded: {uploadedFile.FileName} ({uploadedFile.Length} bytes)");

                    // ลบไฟล์เดิม (ถ้ามี)
                    if (!string.IsNullOrEmpty(budgetItem.FileName))
                    {
                        DeletePhysicalFile(budgetItem.FileName);
                        Console.WriteLine($"🗑️ Old file deleted: {budgetItem.FileName}");
                    }

                    // บันทึกไฟล์ใหม่
                    var uploadResult = await SaveUploadedFileAsync(uploadedFile);
                    if (uploadResult.Success)
                    {
                        budgetItem.FileName = uploadResult.FileName;
                        budgetItem.FileSize = uploadResult.FileSize;
                        budgetItem.FileExtension = uploadResult.FileExtension;
                        budgetItem.FileUploadDate = DateTime.Now;
                        Console.WriteLine($"✅ New file saved: {uploadResult.FileName}");
                    }
                    else
                    {
                        ModelState.AddModelError("UploadedFile", uploadResult.ErrorMessage!);
                        var existingItem = await _budgetService.GetBudgetItemByIdAsync(id);
                        return View("Edit", existingItem);
                    }
                }

                await _budgetService.UpdateBudgetItemAsync(budgetItem);

                Console.WriteLine("✅ Budget item updated successfully");
                TempData["SuccessMessage"] = "แก้ไขรายการงบประมาณสำเร็จ";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                _logger.LogError(ex, "Error updating budget item");
                ModelState.AddModelError("", $"เกิดข้อผิดพลาดในการแก้ไขรายการ: {ex.Message}");

                var existingItem = await _budgetService.GetBudgetItemByIdAsync(id);
                return View("Edit", existingItem);
            }
        }

        /// <summary>
        /// แสดงหน้ายืนยันการลบ
        /// </summary>
        [HttpGet]
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
                // ✅ NEW: ดึงข้อมูลเพื่อลบไฟล์
                var item = await _budgetService.GetBudgetItemByIdAsync(id);
                if (item != null && !string.IsNullOrEmpty(item.FileName))
                {
                    DeletePhysicalFile(item.FileName);
                    Console.WriteLine($"🗑️ File deleted: {item.FileName}");
                }

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

        /// <summary>
        /// ✅ NEW: ดาวน์โหลดไฟล์
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DownloadFile(int id)
        {
            try
            {
                var item = await _budgetService.GetBudgetItemByIdAsync(id);
                if (item == null || string.IsNullOrEmpty(item.FileName))
                {
                    return NotFound();
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                var filePath = Path.Combine(uploadsFolder, item.FileName);

                if (!System.IO.File.Exists(filePath))
                {
                    TempData["ErrorMessage"] = "ไม่พบไฟล์ในระบบ";
                    return RedirectToAction(nameof(Details), new { id });
                }

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                // กำหนด Content-Type ตามนามสกุลไฟล์
                var contentType = item.FileExtension?.ToLower() switch
                {
                    ".pdf" => "application/pdf",
                    ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                    ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ".csv" => "text/csv",
                    _ => "application/octet-stream"
                };

                return File(memory, contentType, item.FileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการดาวน์โหลดไฟล์";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        /// <summary>
        /// ✅ NEW: ลบไฟล์ (เฉพาะไฟล์ ไม่ลบรายการ)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int id)
        {
            try
            {
                var item = await _budgetService.GetBudgetItemByIdAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                // ลบไฟล์จาก wwwroot/uploads
                if (!string.IsNullOrEmpty(item.FileName))
                {
                    DeletePhysicalFile(item.FileName);
                    Console.WriteLine($"🗑️ File deleted: {item.FileName}");
                }

                // ลบข้อมูลไฟล์ในฐานข้อมูล
                await _budgetService.DeleteFileAsync(id);

                TempData["SuccessMessage"] = "ลบไฟล์สำเร็จ";
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return BadRequest(new { success = false, message = "เกิดข้อผิดพลาดในการลบไฟล์" });
            }
        }

        #region ✅ Private Helper Methods

        /// <summary>
        /// บันทึกไฟล์ที่อัปโหลด
        /// </summary>
        private async Task<FileUploadResult> SaveUploadedFileAsync(IFormFile file)
        {
            try
            {
                // ตรวจสอบนามสกุลไฟล์
                var extension = Path.GetExtension(file.FileName).ToLower();
                var allowedExtensions = new[] { ".pdf", ".pptx", ".xlsx", ".csv" };

                if (!allowedExtensions.Contains(extension))
                {
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = "รองรับเฉพาะไฟล์: .pdf, .pptx, .xlsx, .csv"
                    };
                }

                // ตรวจสอบขนาดไฟล์ (10 MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return new FileUploadResult
                    {
                        Success = false,
                        ErrorMessage = "ขนาดไฟล์ต้องไม่เกิน 10 MB"
                    };
                }

                // สร้างชื่อไฟล์ใหม่ (เพื่อป้องกันชื่อซ้ำ)
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";

                // สร้าง folder uploads (ถ้ายังไม่มี)
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // บันทึกไฟล์
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return new FileUploadResult
                {
                    Success = true,
                    FileName = uniqueFileName,
                    FileSize = file.Length,
                    FileExtension = extension
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving uploaded file");
                return new FileUploadResult
                {
                    Success = false,
                    ErrorMessage = "เกิดข้อผิดพลาดในการบันทึกไฟล์"
                };
            }
        }

        /// <summary>
        /// ลบไฟล์จาก wwwroot/uploads
        /// </summary>
        private void DeletePhysicalFile(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return;

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    _logger.LogInformation($"Deleted file: {fileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting physical file: {fileName}");
                // ไม่ throw exception เพราะไม่ต้องการให้ขัดขวางการทำงานหลัก
            }
        }

        #endregion
    }

    /// <summary>
    /// ✅ Helper class สำหรับผลลัพธ์การอัปโหลดไฟล์
    /// </summary>
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public long FileSize { get; set; }
        public string? FileExtension { get; set; }
        public string? ErrorMessage { get; set; }
    }
}