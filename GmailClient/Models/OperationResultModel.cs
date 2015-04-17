namespace GmailClient.Models
{
    public class OperationResultModel
    {
        public OperationResultModel(bool success, string msg)
        {
            this.Success = success;
            this.Message = msg;
        }

        public bool Success { get; private set; }

        public string Message { get; private set; }
    }
}