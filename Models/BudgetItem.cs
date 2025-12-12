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
        [StringLength(100)]
        public string? ProjectType { get; set; }
        [StringLength(50)]
        public string? Urgent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // ✅ File Upload Properties (เพิ่มใหม่)
        [StringLength(255)]
        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        [StringLength(10)]
        public string? FileExtension { get; set; }

        public DateTime? FileUploadDate { get; set; }

        // Helper: รับไฟล์จากฟอร์ม
        [Display(Name = "แนบไฟล์ (.pdf, .pptx, .xlsx, .csv)")]
        public IFormFile? UploadedFile { get; set; }

        // Helper: แสดงขนาดไฟล์
        public string FileSizeFormatted
        {
            get
            {
                if (FileSize == null || FileSize == 0) return "-";
                long bytes = FileSize.Value;
                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;
                double size = bytes;
                while (size >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    size = size / 1024;
                }
                return $"{size:0.##} {sizes[order]}";
            }
        }

        public bool HasFile => !string.IsNullOrEmpty(FileName);
    }
}
