namespace TrialWork.Models
{
    public class WalletRequest
    {
        public OperatorCredentials Credentials { get; set; }
        public class OperatorCredentials
        {
                 public string Username { get; set; }
                 public string Password { get; set; }
                 public string GrantType { get; set; }
                 public string MerchantPin { get; set; }

        }

        public string Msisdn { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string LanguageCode { get; set; }
    }
}
