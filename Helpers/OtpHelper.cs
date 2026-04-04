namespace AUCAPulse.Helpers
{
    public class OtpHelper
    {
        public static string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public static bool ValidateOtp(string providedOtp, string storedOtp, DateTime? expiryTime)
        {
            if (string.IsNullOrEmpty(providedOtp) || string.IsNullOrEmpty(storedOtp))
                return false;

            if (expiryTime == null || DateTime.UtcNow > expiryTime)
                return false;

            return providedOtp == storedOtp;
        }

        public static DateTime GetOtpExpiry(int minutes = 5)
        {
            return DateTime.UtcNow.AddMinutes(minutes);
        }
    }
}
