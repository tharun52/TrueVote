namespace TrueVote.Interfaces
{
    public interface IAuditLogger
    {
        void LogAction(string username, string action, bool isAudit);
    }

}