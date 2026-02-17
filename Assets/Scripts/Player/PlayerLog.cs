using System.Text;
using UnityEngine;

public static class PlayerLog
{
    public delegate void LogUpdateFunc(string s, int newCharCount);

    readonly static int maxLine = 64;

    static string[] buffer = new string[maxLine];
    static int last = 0;
    static bool loop = false;
    static bool generated = false;

    static StringBuilder sb = new StringBuilder(2048);

    public static LogUpdateFunc logUpdateFunc;

    public static void ResetLog()
    {
        last = 0;
        loop = false;
        generated = false;

        logUpdateFunc?.Invoke("", 0);
    }

    public static void AddLog(string s)
    {
        buffer[last] = s;
        generated = false;

        last++;

        if(last >= maxLine)
        {
            last = 0;
            loop = true;
        }

        logUpdateFunc?.Invoke(GetLogTexts(), s.Length);
    }

    public static string GetLogTexts()
    {
        if(generated)
        {
            return sb.ToString();
        }

        sb.Clear();

        if(loop)
        {
            for(int i = last+1; i < maxLine; ++i)
            {
                sb.AppendLine(buffer[i]);
            }
        }

        for(int i = 0; i < last; ++i)
        {
            sb.AppendLine(buffer[i]);
        }

        generated = true;

        return sb.ToString();
    }
}
