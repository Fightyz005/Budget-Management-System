using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace BudgetManagementSystem.Web.Models
{
    /// <summary>
    /// Model สำหรับเซสชันการลงคะแนน
    /// </summary>
    public class VotingSession
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string VoteId { get; set; } = string.Empty;

        [Required]
        public int BudgetItemId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string Voters { get; set; } = string.Empty; // JSON array

        // Helper property to deserialize Voters JSON
        public List<string> VotersList
        {
            get
            {
                try
                {
                    return string.IsNullOrEmpty(Voters) 
                        ? new List<string>() 
                        : JsonSerializer.Deserialize<List<string>>(Voters) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
        }

        [StringLength(50)]
        public string Status { get; set; } = "pending";

        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public BudgetItem? BudgetItem { get; set; }
    }

    /// <summary>
    /// Model สำหรับการลงคะแนน
    /// </summary>
    public class Vote
    {
        public int Id { get; set; }

        [Required]
        public int VotingSessionId { get; set; }

        [Required]
        [StringLength(100)]
        public string VoterName { get; set; } = string.Empty;

        [StringLength(200)]
        [EmailAddress]
        public string? VoterEmail { get; set; }

        [Required]
        [StringLength(50)]
        public string VoteChoice { get; set; } = string.Empty; // approved, partial, rejected

        [Range(0, double.MaxValue)]
        public decimal? SuggestedAmount { get; set; }

        public string? Comment { get; set; }

        public DateTime VotedAt { get; set; }

        // Navigation property
        public VotingSession? VotingSession { get; set; }
    }
}
