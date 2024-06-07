public static class StringExtensions
{
    /// <summary>
    /// Create a method that returns a string without any white space and should have a max length of 50 characters
    /// </summary>
    /// <param name="str">the string to edit</param>
    /// <param name="length">the max length to allow</param>
    public static string RemoveWhiteSpaceAndLimitLength(this string str, int length)
    {
        return str.Replace(" ", "")[..length];
    }

    // Add a method that will filter the string to only supports alphanumeric values, `-`, `_` and has a maximum length of 30 characters.
    public static string FilterStringToLetterDigitDashUnderscoreMaxLength(this string str, int length)
    {
        string filteredString = "";
        foreach (char c in str)
        {
            if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
            {
                filteredString += c;
            }
        }
        return filteredString[..length];
    }
}