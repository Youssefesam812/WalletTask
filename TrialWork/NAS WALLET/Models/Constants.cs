namespace TrialWork.Models
{
   public static class Constants
    {

        public enum methodAPI
        {
            Refund,
            GetToken,
            Create
        }


        public static string ReturnSuccess { get; } = "Success";
        public static string ReturnError { get; } = "Failure";
        public static string Operator { get; } = "NAS Wallet";

    
        public static readonly DateTime Date = DateTime.Now;
        public static readonly string FormattedDate = Date.ToString("yyyy-MM-dd");

        public static string Url { get; } = "https://uatgw1.nasswallet.com";
    }
}
