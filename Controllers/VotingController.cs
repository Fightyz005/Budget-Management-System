using BudgetManagementSystem.Web.Services;
using BudgetManagementSystem.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BudgetManagementSystem.Web.Controllers
{
    /// <summary>
    /// Controller สำหรับระบบลงคะแนน
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
        /// แสดงฟอร์มสร้างเซสชันการลงคะแนน
        /// </summary>
        public async Task<IActionResult> Create(int budgetItemId)
        {
            try
            {
                var budgetItem = await _budgetService.GetBudgetItemByIdAsync(budgetItemId);
                if (budgetItem == null)
                {
                    return NotFound();
                }

                var model = new CreateVotingSessionViewModel
                {
                    BudgetItemId = budgetItem.Id,
                    Title = budgetItem.Item,
                    Description = budgetItem.Description,
                    Amount = budgetItem.Amount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voting session create page");
                return View("Error");
            }
        }

        /// <summary>
        /// สร้างเซสชันการลงคะแนนใหม่
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVotingSessionViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var voteId = await _votingService.CreateVotingSessionAsync(model);
                    TempData["SuccessMessage"] = "สร้างเซสชันการลงคะแนนสำเร็จ";
                    TempData["VoteId"] = voteId;
                    return RedirectToAction(nameof(Share), new { voteId });
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voting session");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการสร้างเซสชัน");
                return View(model);
            }
        }

        /// <summary>
        /// แสดงหน้าแชร์ลิงก์การลงคะแนน
        /// </summary>
        public IActionResult Share(string voteId)
        {
            ViewBag.VoteId = voteId;
            ViewBag.VoteUrl = Url.Action("Vote", "Voting", new { voteId }, Request.Scheme);
            return View();
        }

        /// <summary>
        /// หน้าลงคะแนน
        /// </summary>
        public async Task<IActionResult> Vote(string voteId)
        {
            try
            {
                var session = await _votingService.GetVotingSessionAsync(voteId);
                if (session == null)
                {
                    return NotFound();
                }

                if (session.IsClosed)
                {
                    return RedirectToAction(nameof(Results), new { voteId });
                }

                ViewBag.VoteId = voteId;
                ViewBag.Session = session;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voting page");
                return View("Error");
            }
        }

        /// <summary>
        /// บันทึกการลงคะแนน
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitVote(SubmitVoteViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var success = await _votingService.SubmitVoteAsync(model);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "บันทึกการลงคะแนนสำเร็จ";
                        return RedirectToAction(nameof(ThankYou));
                    }
                    else
                    {
                        ModelState.AddModelError("", "ไม่สามารถบันทึกการลงคะแนนได้");
                    }
                }
                return RedirectToAction(nameof(Vote), new { voteId = model.VoteId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting vote");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการบันทึกการลงคะแนน";
                return RedirectToAction(nameof(Vote), new { voteId = model.VoteId });
            }
        }

        /// <summary>
        /// หน้าขอบคุณหลังจากลงคะแนน
        /// </summary>
        public IActionResult ThankYou()
        {
            return View();
        }

        /// <summary>
        /// แสดงผลการลงคะแนน
        /// </summary>
        public async Task<IActionResult> Results(string voteId)
        {
            try
            {
                var results = await _votingService.GetVotingResultsAsync(voteId);
                return View(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voting results");
                return View("Error");
            }
        }

        /// <summary>
        /// ปิดเซสชันการลงคะแนน
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(string voteId)
        {
            try
            {
                var success = await _votingService.CloseVotingSessionAsync(voteId);
                if (success)
                {
                    TempData["SuccessMessage"] = "ปิดเซสชันการลงคะแนนสำเร็จ";
                }
                else
                {
                    TempData["ErrorMessage"] = "ไม่สามารถปิดเซสชันได้";
                }
                return RedirectToAction(nameof(Results), new { voteId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing voting session");
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการปิดเซสชัน";
                return RedirectToAction(nameof(Results), new { voteId });
            }
        }
    }
}
