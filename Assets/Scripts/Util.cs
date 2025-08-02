using UnityEngine;

public static class Util
{
    // Checks if the object is null and logs an error message if it is
    public static void AssertObject(Object obj, string message)
    {
        if (obj == null)
        {
            Debug.LogError(message);
#if UNITY_EDITOR
            // In the editor, throw an exception to halt execution
            throw new UnityException(message);
#endif
        }
    }

    // Returns a formatted time string in the format "HH:MM:SS"
    public static string GetFormattedTime(float seconds, bool reverse = false)
    {
        int hours = Mathf.FloorToInt(seconds / 3600);
        int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        string formattedTime = $"{hours:D2}:{minutes:D2}:{secs:D2}";
        if (reverse)
        {
            formattedTime = reverseString(formattedTime);
        }
        return formattedTime;
    }

    // return a reversed string - good for Hebrew
    // e.g. "12:34:56" -> "65:43:21
    public static string reverseString(string input)
    {
        char[] charArray = input.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }
}
