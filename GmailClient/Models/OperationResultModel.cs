namespace GmailClient.Models
{
    public class OperationResultModel
    {
        public OperationResultModel(bool success, string msg = null, object data = null)
        {
            this.Success = success;
            this.Message = msg;
            this.Data = data;
        }

        public bool Success { get; private set; }

        public string Message { get; private set; }

        public object Data { get; private set; }
    }
}