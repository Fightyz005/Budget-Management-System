using System.ComponentModel.DataAnnotations;

namespace BudgetManagementSystem.Web.Models
{
    /// <summary>
    /// Model สำหรับรายการงบประมาณ
    /// </summary>
    public class BudgetItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณาเลือกหมวดหมู่")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาระบุชื่อรายการ")]
        [StringLength(200)]
        public string Item { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(200)]
        public string? Department { get; set; }

        [StringLength(200)]
        public string? Division { get; set; }

        [Required(ErrorMessage = "กรุณาระบุจำนวนเงิน")]
        [Range(0, double.MaxValue, ErrorMessage = "จำนวนเงินต้องมากกว่า 0")]
        public decimal Amount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "จำนวนเงินต้องมากกว่า 0")]
        public decimal ApprovedAmount { get; set; }

        public string? Notes { get; set; }
        public string? Benefits { get; set; }
        public string? Worthiness { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "proposed";

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
