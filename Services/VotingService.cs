using BudgetManagementSystem.Web.Models;
using BudgetManagementSystem.Web.Repositories;
using BudgetManagementSystem.Web.ViewModels;
using System.Text.Json;

namespace BudgetManagementSystem.Web.Services
{
    /// <summary>
    /// Service สำหรับจัดการ Business Logic ของระบบลงคะแนน
    /// </summary>
    public interface IVotingService
    {
        Task<string> CreateVotingSessionAsync(CreateVotingSessionViewModel model);
        Task<VotingSession?> GetVotingSessionAsync(string voteId);
        Task<bool> SubmitVoteAsync(SubmitVoteViewModel model);
        Task<VotingResultsViewModel> GetVotingResultsAsync(string voteId);
        Task<bool> CloseVotingSessionAsync(string voteId);
    }

    public class VotingService : IVotingService
    {
        private readonly IVotingRepository _votingRepository;
        private readonly IBudgetRepository _budgetRepository;

        public VotingService(
            IVotingRepository votingRepository,
            IBudgetRepository budgetRepository)
        {
            _votingRepository = votingRepository;
            _budgetRepository = budgetRepository;
        }

        /// <summary>
        /// สร้างเซสชันการลงคะแนนใหม่
        /// </summary>
        public async Task<string> CreateVotingSessionAsync(CreateVotingSessionViewModel model)
        {
            // สร้าง VoteId ที่ไม่ซ้ำ
            var voteId = Guid.NewGuid().ToString("N").Substring(0, 8);

            // แปลง List voters เป็น JSON
            var votersJson = JsonSerializer.Serialize(model.Voters);

            var session = new VotingSession
            {
                VoteId = voteId,
                BudgetItemId = model.BudgetItemId,
                Title = model.Title,
                Description = model.Description,
                Amount = model.Amount,
                Voters = votersJson,
                Status = "pending",
                IsClosed = false
            };

            await _votingRepository.CreateVotingSessionAsync(session);
            return voteId;
        }

        /// <summary>
        /// ดึงข้อมูลเซสชันการลงคะแนน
        /// </summary>
        public async Task<VotingSession?> GetVotingSessionAsync(string voteId)
        {
            return await _votingRepository.GetVotingSessionByVoteIdAsync(voteId);
        }

        /// <summary>
        /// บันทึกการลงคะแนน
        /// </summary>
        public async Task<bool> SubmitVoteAsync(SubmitVoteViewModel model)
        {
            // ดึงข้อมูล session
            VotingSession? session = null;

            if (model.VotingSessionId > 0)
            {
                session = await _votingRepository.GetVotingSessionByIdAsync(model.VotingSessionId);
            }
            else if (!string.IsNullOrEmpty(model.VoteId))
            {
                session = await _votingRepository.GetVotingSessionByVoteIdAsync(model.VoteId);
                if (session != null)
                {
                    model.VotingSessionId = session.Id;
                }
            }

            if (session == null || session.IsClosed)
            {
                return false;
            }

            // ตรวจสอบว่าชื่อผู้ลงคะแนนอยู่ในรายชื่อหรือไม่
            var voters = JsonSerializer.Deserialize<List<string>>(session.Voters) ?? new List<string>();
            if (!voters.Any(v => v.Equals(model.VoterName, StringComparison.OrdinalIgnoreCase)))
            {
                return false; // ไม่มีสิทธิ์ลงคะแนน
            }

            var vote = new Vote
            {
                VotingSessionId = model.VotingSessionId,
                VoterName = model.VoterName,
                VoterEmail = model.VoterEmail,
                VoteChoice = model.VoteChoice,
                SuggestedAmount = model.SuggestedAmount,
                Comment = model.Comment
            };

            var result = await _votingRepository.SubmitVoteAsync(vote);
            return result > 0;
        }

        /// <summary>
        /// ดึงผลการลงคะแนน
        /// </summary>
        public async Task<VotingResultsViewModel> GetVotingResultsAsync(string voteId)
        {
            var session = await _votingRepository.GetVotingSessionByVoteIdAsync(voteId);
            if (session == null)
            {
                throw new ArgumentException("ไม่พบเซสชันการลงคะแนน");
            }

            var votes = await _votingRepository.GetVotingResultsAsync(session.Id);

            var results = new VotingResultsViewModel
            {
                Session = session,
                Votes = votes,
                TotalVotes = votes.Count,
                ApprovedCount = votes.Count(v => v.VoteChoice == "approved"),
                RejectedCount = votes.Count(v => v.VoteChoice == "rejected"),
                PartialCount = votes.Count(v => v.VoteChoice == "partial"),
                AverageSuggestedAmount = votes.Where(v => v.SuggestedAmount.HasValue && v.SuggestedAmount > 0)
                                             .Any() 
                    ? votes.Where(v => v.SuggestedAmount.HasValue && v.SuggestedAmount > 0)
                           .Average(v => v.SuggestedAmount!.Value)
                    : 0
            };

            return results;
        }

        /// <summary>
        /// ปิดเซสชันการลงคะแนน
        /// </summary>
        public async Task<bool> CloseVotingSessionAsync(string voteId)
        {
            var session = await _votingRepository.GetVotingSessionByVoteIdAsync(voteId);
            if (session == null)
            {
                return false;
            }

            return await _votingRepository.CloseVotingSessionAsync(session.Id);
        }
    }
}
