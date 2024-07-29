namespace TrialWork.Models
{
    public class GetMerchantTokenResponse
    {
        public DataGetMerchant data { get; set; }
        public int ResponseCode { get; set; }
        public string ErrCode { get; set; }
        public string Message { get; set; }
        public class DataGetMerchant
        {
            public string AccessToken { get; set; }
            public string AccessTokenExpiry { get; set; }
            public string TransactionId { get; set; }
            public string DeviceAction { get; set; }
            public string TenantId { get; set; }
        }

    }
}
