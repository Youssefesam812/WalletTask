namespace TrialWork.Models
{
    public class StartTransactionRequest
    {
        internal DataStart data;
        public class DataStart
        {
            public string UserIdentifier { get; set; }
            public string TransactionPin { get; set; }
            public string OrderId { get; set; }
            public string Amount { get; set; }
            public string LanguageCode { get; set; }
        }
    }
}
