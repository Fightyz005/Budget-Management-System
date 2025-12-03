using BudgetManagementSystem.Web.Data;
using BudgetManagementSystem.Web.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BudgetManagementSystem.Web.Repositories
{
    /// <summary>
    /// Repository สำหรับจัดการข้อมูลการลงคะแนน
    /// ใช้ Pure ADO.NET และ Stored Procedures
    /// ใช้ Parameterized Queries เพื่อป้องกัน SQL Injection
    /// </summary>
    public interface IVotingRepository
    {
        Task<int> CreateVotingSessionAsync(VotingSession session);
        Task<VotingSession?> GetVotingSessionByVoteIdAsync(string voteId);
        Task<VotingSession?> GetVotingSessionByIdAsync(int sessionId);
        Task<int> SubmitVoteAsync(Vote vote);
        Task<List<Vote>> GetVotingResultsAsync(int votingSessionId);
        Task<bool> CloseVotingSessionAsync(int votingSessionId);
    }

    public class VotingRepository : IVotingRepository
    {
        private readonly SqlConnectionFactory _connectionFactory;

        public VotingRepository(SqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// สร้างเซสชันการลงคะแนนใหม่
        /// </summary>
        public async Task<int> CreateVotingSessionAsync(VotingSession session)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_CreateVotingSession", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@VoteId", session.VoteId);
            command.Parameters.AddWithValue("@BudgetItemId", session.BudgetItemId);
            command.Parameters.AddWithValue("@Title", session.Title);
            command.Parameters.AddWithValue("@Description", (object?)session.Description ?? DBNull.Value);
            command.Parameters.AddWithValue("@Amount", session.Amount);
            command.Parameters.AddWithValue("@Voters", session.Voters);
            command.Parameters.AddWithValue("@Status", session.Status);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// ดึงข้อมูลเซสชันการลงคะแนน
        /// </summary>
        public async Task<VotingSession?> GetVotingSessionByVoteIdAsync(string voteId)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetVotingSessionByVoteId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@VoteId", voteId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new VotingSession
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    VoteId = reader.GetString(reader.GetOrdinal("VoteId")),
                    BudgetItemId = reader.GetInt32(reader.GetOrdinal("BudgetItemId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("Description")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    Voters = reader.GetString(reader.GetOrdinal("Voters")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    IsClosed = reader.GetBoolean(reader.GetOrdinal("IsClosed")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                };
            }

            return null;
        }

        /// <summary>
        /// ดึงข้อมูลเซสชันการลงคะแนนจาก Session Id
        /// </summary>
        public async Task<VotingSession?> GetVotingSessionByIdAsync(int sessionId)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand(
                "SELECT * FROM VotingSessions WHERE Id = @SessionId", 
                connection);

            command.Parameters.AddWithValue("@SessionId", sessionId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new VotingSession
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    VoteId = reader.GetString(reader.GetOrdinal("VoteId")),
                    BudgetItemId = reader.GetInt32(reader.GetOrdinal("BudgetItemId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("Description")),
                    Amount = reader.GetDecimal(reader.GetOrdinal("Amount")),
                    Voters = reader.GetString(reader.GetOrdinal("Voters")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    IsClosed = reader.GetBoolean(reader.GetOrdinal("IsClosed")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                };
            }

            return null;
        }

        /// <summary>
        /// บันทึกการลงคะแนน
        /// </summary>
        public async Task<int> SubmitVoteAsync(Vote vote)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_SubmitVote", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@VotingSessionId", vote.VotingSessionId);
            command.Parameters.AddWithValue("@VoterName", vote.VoterName);
            command.Parameters.AddWithValue("@VoterEmail", (object?)vote.VoterEmail ?? DBNull.Value);
            command.Parameters.AddWithValue("@VoteChoice", vote.VoteChoice);
            command.Parameters.AddWithValue("@SuggestedAmount", (object?)vote.SuggestedAmount ?? DBNull.Value);
            command.Parameters.AddWithValue("@Comment", (object?)vote.Comment ?? DBNull.Value);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var result = reader.GetString(0);
                return result == "inserted" ? 1 : 2; // 1 = inserted, 2 = updated
            }

            return 0;
        }

        /// <summary>
        /// ดึงผลการลงคะแนน
        /// </summary>
        public async Task<List<Vote>> GetVotingResultsAsync(int votingSessionId)
        {
            var votes = new List<Vote>();

            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_GetVotingResults", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@VotingSessionId", votingSessionId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                votes.Add(new Vote
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    VotingSessionId = reader.GetInt32(reader.GetOrdinal("VotingSessionId")),
                    VoterName = reader.GetString(reader.GetOrdinal("VoterName")),
                    VoterEmail = reader.IsDBNull(reader.GetOrdinal("VoterEmail")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("VoterEmail")),
                    VoteChoice = reader.GetString(reader.GetOrdinal("VoteChoice")),
                    SuggestedAmount = reader.IsDBNull(reader.GetOrdinal("SuggestedAmount")) 
                        ? null 
                        : reader.GetDecimal(reader.GetOrdinal("SuggestedAmount")),
                    Comment = reader.IsDBNull(reader.GetOrdinal("Comment")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("Comment")),
                    VotedAt = reader.GetDateTime(reader.GetOrdinal("VotedAt"))
                });
            }

            return votes;
        }

        /// <summary>
        /// ปิดเซสชันการลงคะแนน
        /// </summary>
        public async Task<bool> CloseVotingSessionAsync(int votingSessionId)
        {
            using var connection = await _connectionFactory.CreateOpenConnectionAsync();
            using var command = new SqlCommand("sp_CloseVotingSession", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ใช้ Parameterized Query เพื่อป้องกัน SQL Injection
            command.Parameters.AddWithValue("@VotingSessionId", votingSessionId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var result = reader.GetString(0);
                return result == "success";
            }

            return false;
        }
    }
}
