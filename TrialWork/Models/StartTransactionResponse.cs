namespace TrialWork.Models
{
    public class StartTransactionResponse
    {
        public int ResponseCode { get; set; }
        public string ErrCode { get; set; }
        public string Message { get; set; }
        public class DataStartResponse
        {
            public string TransactionId { get; set; }
            public string Token { get; set; }
        }
        
    }
}
