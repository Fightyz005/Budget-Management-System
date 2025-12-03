using BudgetManagementSystem.Web.Models;

namespace BudgetManagementSystem.Web.ViewModels
{
    /// <summary>
    /// ViewModel สำหรับแสดงสถิติงบประมาณ
    /// </summary>
    public class BudgetStatisticsViewModel
    {
        public decimal TotalProposed { get; set; }
        public decimal TotalApproved { get; set; }
        public int TotalItems { get; set; }
        public int ApprovedItems { get; set; }
        public int ProposedItems { get; set; }
        public int RejectedItems { get; set; }
        public double ApprovalPercentage => TotalProposed > 0 
            ? Math.Round((double)(TotalApproved / TotalProposed) * 100, 2) 
            : 0;
    }

    /// <summary>
    /// ViewModel สำหรับสรุปงบประมาณตามแผนก
    /// </summary>
    public class BudgetByDepartmentViewModel
    {
        public string Department { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalApproved { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับสรุปงบประมาณตามหมวดหมู่
    /// </summary>
    public class BudgetByCategoryViewModel
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalApproved { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับหน้า Dashboard
    /// </summary>
    public class DashboardViewModel
    {
        public BudgetStatisticsViewModel Statistics { get; set; } = new();
        public List<BudgetByDepartmentViewModel> DepartmentBudgets { get; set; } = new();
        public List<BudgetByCategoryViewModel> CategoryBudgets { get; set; } = new();
        public List<BudgetItem> BudgetItems { get; set; } = new(); // เพิ่มรายการงบประมาณ
    }

    /// <summary>
    /// ViewModel สำหรับการสร้างเซสชันลงคะแนน
    /// </summary>
    public class CreateVotingSessionViewModel
    {
        public int BudgetItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public List<string> Voters { get; set; } = new();
    }

    /// <summary>
    /// ViewModel สำหรับการลงคะแนน
    /// </summary>
    public class SubmitVoteViewModel
    {
        public int VotingSessionId { get; set; }
        public string? VoteId { get; set; } // Optional: for lookup by VoteId
        public string VoterName { get; set; } = string.Empty;
        public string? VoterEmail { get; set; }
        public string VoteChoice { get; set; } = string.Empty; // approved, partial, rejected
        public decimal? SuggestedAmount { get; set; }
        public string? Comment { get; set; }
    }

    /// <summary>
    /// ViewModel สำหรับแสดงผลการลงคะแนน
    /// </summary>
    public class VotingResultsViewModel
    {
        public VotingSession Session { get; set; } = new();
        public List<Vote> Votes { get; set; } = new();
        public int TotalVotes { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PartialCount { get; set; }
        public decimal AverageSuggestedAmount { get; set; }
    }
}
