namespace TrialWork.Models
{
    public class GetMerchantTokenRequest
    {
        public Data data { get; set; } 

        public class Data
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string GrantType { get; set; }
        }
    }
}
