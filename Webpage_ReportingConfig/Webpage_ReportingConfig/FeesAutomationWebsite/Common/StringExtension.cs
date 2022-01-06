namespace FeesAutomationWebsite.Common
{
    public static class StringExtension
    {
        /// <summary>
        /// Get Tidy Cell Value remove unwanted characters
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TidyCellValue(this string value)
        {
            return value.Replace("\"", "").Replace("$", "").Trim();
        }

    }
}