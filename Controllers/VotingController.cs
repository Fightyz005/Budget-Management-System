using BudgetManagementSystem.Web.Services;
using BudgetManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using BudgetManagementSystem.Web.ViewModels;

namespace BudgetManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับระบบลงคะแนน - COMPLETE VERSION with Manual Form Binding
    /// </summary>
    public class VotingController : Controller
    {
        private readonly IVotingService _votingService;
        private readonly IBudgetService _budgetService;
        private readonly ILogger<VotingController> _logger;

        public VotingController(
            IVotingService votingService,
            IBudgetService budgetService,
            ILogger<VotingController> logger)
        {
            _votingService = votingService;
            _budgetService = budgetService;
            _logger = logger;
        }

        /// <summary>
        /// แสดงฟอร์มสร้างเซสชันการลงคะแนน (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Create(int budgetItemId)
        {
            try
            {
                Console.WriteLine($"=== VOTING CREATE GET - BudgetItemId: {budgetItemId} ===");

                var budgetItem = await _budgetService.GetBudgetItemByIdAsync(budgetItemId);
                if (budgetItem == null)
                {
                    Console.WriteLine("❌ Budget item not found");
                    return NotFound();
                }

                var model = new VotingSession
                {
                    BudgetItemId = budgetItem.Id,
                    Title = budgetItem.Item,
                    Description = budgetItem.Description,
                    Amount = budgetItem.Amount
                };

                Console.WriteLine($"✅ Loaded budget item: {budgetItem.Item}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voting session create page");
                return View("Error");
            }
        }

        /// <summary>
        /// สร้างเซสชันการลงคะแนนใหม่ (POST) - Manual Form Binding
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            try
            {
                Console.WriteLine("=== VOTING CREATE POST START ===");

                // อ่านค่าจาก Form โดยตรง
                var budgetItemIdStr = Request.Form["BudgetItemId"].ToString();
                var title = Request.Form["Title"].ToString();
                var description = Request.Form["Description"].ToString();
                var amountStr = Request.Form["Amount"].ToString();

                // อ่าน Voters (เป็น array)
                var voters = Request.Form["Voters"].ToList();
                voters = voters.Where(v => !string.IsNullOrWhiteSpace(v)).ToList();

                Console.WriteLine($"BudgetItemId: {budgetItemIdStr}");
                Console.WriteLine($"Title: {title}");
                Console.WriteLine($"Voters count: {voters.Count}");

                // Validation
                int budgetItemId = 0;
                if (!int.TryParse(budgetItemIdStr, out budgetItemId) || budgetItemId <= 0)
                {
                    ModelState.AddModelError("BudgetItemId", "รหัสงบประมาณไม่ถูกต้อง");
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    ModelState.AddModelError("Title", "กรุณาระบุหัวข้อการลงคะแนน");
                }

                decimal amount = 0;
                if (!decimal.TryParse(amountStr, out amount) || amount <= 0)
                {
                    ModelState.AddModelError("Amount", "กรุณาระบุจำนวนเงินที่ถูกต้อง");
                }

                if (voters.Count == 0)
                {
                    ModelState.AddModelError("Voters", "กรุณาเพิ่มผู้มีสิทธิ์ลงคะแนนอย่างน้อย 1 คน");
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

                    var errorModel = new VotingSession
                    {
                        BudgetItemId = budgetItemId,
                        Title = title,
                        Description = description,
                        Amount = amount
                    };

                    return View("Create", errorModel);
                }

                // สร้าง Model สำหรับ Service
                var model = new CreateVotingSessionViewModel
                {
                    BudgetItemId = budgetItemId,
                    Title = title,
                    Description = string.IsNullOrWhiteSpace(description) ? null : description,
                    Amount = amount,
                    Voters = voters
                };

                Console.WriteLine("✅ Creating voting session...");
                var voteId = await _votingService.CreateVotingSessionAsync(model);

                Console.WriteLine($"✅ Voting session created: {voteId}");
                TempData["SuccessMessage"] = "สร้างเซสชันการลงคะแนนสำเร็จ";
                return RedirectToAction(nameof(Share), new { voteId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                _logger.LogError(ex, "Error creating voting session");
                ModelState.AddModelError("", $"เกิดข้อผิดพลาดในการสร้างเซสชัน: {ex.Message}");
                return View("Create", new VotingSession());
            }
        }

        /// <summary>
        /// แสดงหน้าแชร์ลิงก์การลงคะแนน
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Share(string voteId)
        {
            try
            {
                Console.WriteLine($"=== VOTING SHARE - VoteId: {voteId} ===");

                var session = await _votingService.GetVotingSessionAsync(voteId);
                if (session == null)
                {
                    Console.WriteLine("❌ Voting session not found");
                    return NotFound();
                }

                Console.WriteLine($"✅ Loaded voting session: {session.Title}");
                return View(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading share page");
                return View("Error");
            }
        }

        /// <summary>
        /// หน้าลงคะแนน (GET)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Vote(string id)
        {
            try
            {
                Console.WriteLine($"=== VOTING VOTE PAGE - VoteId: {id} ===");

                var session = await _votingService.GetVotingSessionAsync(id);
                if (session == null)
                {
                    Console.WriteLine("❌ Voting session not found");
                    return NotFound();
                }

                if (session.IsClosed)
                {
                    Console.WriteLine("⚠️ Voting session is closed, redirecting to results");
                    return RedirectToAction(nameof(Results), new { voteId = id });
                }

                Console.WriteLine($"✅ Loaded voting session: {session.Title}");
                return View(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voting page");
                return View("Error");
            }
        }

        /// <summary>
        /// บันทึกการลงคะแนน (POST) - Manual Form Binding
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitVotePost()
        {
            try
            {
                Console.WriteLine("=== SUBMIT VOTE POST START ===");

                // อ่านค่าจาก Form โดยตรง
                var votingSessionIdStr = Request.Form["VotingSessionId"].ToString();
                var voteId = Request.Form["VoteId"].ToString();
                var voterName = Request.Form["VoterName"].ToString()?.Trim();
                var voterEmail = Request.Form["VoterEmail"].ToString()?.Trim();
                var voteChoice = Request.Form["VoteChoice"].ToString();
                var suggestedAmountStr = Request.Form["SuggestedAmount"].ToString();
                var comment = Request.Form["Comment"].ToString();

                Console.WriteLine($"VotingSessionId: {votingSessionIdStr}");
                Console.WriteLine($"VoterName: {voterName}");
                Console.WriteLine($"VoteChoice: {voteChoice}");

                // Validation
                int votingSessionId = 0;
                if (!int.TryParse(votingSessionIdStr, out votingSessionId) || votingSessionId <= 0)
                {
                    ModelState.AddModelError("VotingSessionId", "รหัสเซสชันไม่ถูกต้อง");
                }

                if (string.IsNullOrWhiteSpace(voterName))
                {
                    ModelState.AddModelError("VoterName", "กรุณาระบุชื่อของคุณ");
                }

                if (string.IsNullOrWhiteSpace(voteChoice))
                {
                    ModelState.AddModelError("VoteChoice", "กรุณาเลือกคะแนนเสียง");
                }

                // Validate suggested amount for "partial" choice
                decimal? suggestedAmount = null;
                if (voteChoice == "partial")
                {
                    if (decimal.TryParse(suggestedAmountStr, out var amount) && amount > 0)
                    {
                        suggestedAmount = amount;
                    }
                    else
                    {
                        ModelState.AddModelError("SuggestedAmount", "กรุณาระบุงบประมาณที่เสนอ");
                    }
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

                    // โหลด session เพื่อแสดง error
                    var session = await _votingService.GetVotingSessionAsync(
                        await GetVoteIdFromSessionIdAsync(votingSessionId));

                    return View("Vote", session);
                }

                // สร้าง Model สำหรับ Service
                var model = new SubmitVoteViewModel
                {
                    VotingSessionId = votingSessionId,
                    VoterName = voterName,
                    VoterEmail = string.IsNullOrWhiteSpace(voterEmail) ? null : voterEmail,
                    VoteChoice = voteChoice,
                    SuggestedAmount = suggestedAmount,
                    Comment = string.IsNullOrWhiteSpace(comment) ? null : comment
                };

                Console.WriteLine("✅ Submitting vote...");
                var success = await _votingService.SubmitVoteAsync(model);

                if (success)
                {
                    Console.WriteLine("✅ Vote submitted successfully");
                    TempData["SuccessMessage"] = "บันทึกการลงคะแนนสำเร็จ";
                    return RedirectToAction(nameof(ThankYou));
                }
                else
                {
                    Console.WriteLine("❌ Failed to submit vote");
                    ModelState.AddModelError("", "ไม่สามารถบันทึกการลงคะแนนได้ ชื่อของคุณอาจไม่อยู่ในรายชื่อผู้มีสิทธิ์");

                    TempData["ErrorMessage"] = "ไม่สามารถบันทึกการลงคะแนนได้ กรุณาตรวจสอบว่า:<br>" +
                                             "1. ชื่อของคุณอยู่ในรายชื่อผู้มีสิทธิ์ลงคะแนน<br>" +
                                             "2. การลงคะแนนยังไม่ถูกปิด<br>" +
                                             "3. ชื่อต้องตรงตามที่ระบุไว้ในรายชื่อทุกตัวอักษร";

                    var session = await _votingService.GetVotingSessionAsync(
                        await GetVoteIdFromSessionIdAsync(votingSessionId));

                    return View("Vote", session);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                _logger.LogError(ex, "Error submitting vote");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการบันทึกการลงคะแนน";
                return RedirectToAction(nameof(ThankYou));
            }
        }

        /// <summary>
        /// หน้าขอบคุณหลังจากลงคะแนน
        /// </summary>
        [HttpGet]
        public IActionResult ThankYou()
        {
            return View();
        }

        /// <summary>
        /// แสดงผลการลงคะแนน
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Results(string id)
        {
            try
            {
                Console.WriteLine($"=== VOTING RESULTS - VoteId: {id} ===");

                var results = await _votingService.GetVotingResultsAsync(id);

                Console.WriteLine($"✅ Loaded results: {results.TotalVotes} votes");
                return View(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voting results");
                return View("Error");
            }
        }

        /// <summary>
        /// ปิดเซสชันการลงคะแนน (POST)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClosePost()
        {
            try
            {
                Console.WriteLine("=== CLOSE VOTING SESSION POST START ===");

                var voteId = Request.Form["voteId"].ToString();
                Console.WriteLine($"VoteId: {voteId}");

                if (string.IsNullOrWhiteSpace(voteId))
                {
                    TempData["ErrorMessage"] = "ไม่พบรหัสการลงคะแนน";
                    return RedirectToAction("Index", "Home");
                }

                var success = await _votingService.CloseVotingSessionAsync(voteId);

                if (success)
                {
                    Console.WriteLine("✅ Voting session closed successfully");
                    TempData["SuccessMessage"] = "ปิดเซสชันการลงคะแนนสำเร็จ";
                }
                else
                {
                    Console.WriteLine("❌ Failed to close voting session");
                    TempData["ErrorMessage"] = "ไม่สามารถปิดเซสชันได้";
                }

                return RedirectToAction(nameof(Results), new { voteId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Exception: {ex.Message}");
                _logger.LogError(ex, "Error closing voting session");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการปิดเซสชัน";

                var voteId = Request.Form["voteId"].ToString();
                return RedirectToAction(nameof(Results), new { voteId });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// ดึง VoteId จาก SessionId (helper method)
        /// </summary>
        private async Task<string> GetVoteIdFromSessionIdAsync(int sessionId)
        {
            // ใช้ Repository หรือ Service เพื่อดึง VoteId
            // สำหรับตอนนี้ return empty string
            // ต้องเพิ่ม method ใน IVotingService
            return string.Empty;
        }

        #endregion
    }
}