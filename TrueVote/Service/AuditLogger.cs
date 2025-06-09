using Serilog;
using Serilog.Context;
using TrueVote.Interfaces;

namespace TrueVote.Service
{
    public class AuditLogger : IAuditLogger
    {
        public void LogAction(string username, string action, bool isAudit)
        {
            Log.ForContext("IsAudit", isAudit)
            .Information("User: {User}, Action: {Action}", username, action);
        }
    }
}