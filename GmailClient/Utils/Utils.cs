namespace GmailClient.Utils
{
    public static class Utils
    {
        public static int? TryParseInt(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            int result;
            if (int.TryParse(str, out result))
            {
                return result;
            }

            return null;
        }
    }
}