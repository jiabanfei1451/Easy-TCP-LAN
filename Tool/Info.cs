using System;

namespace NETDNS;
public static class Info{
    public static void Print(params object[] what)
    {
        string ToStr = "";
        foreach(var d in what)
        {
            if (!(d is string) || !(d is String)){
                ToStr += d;
            }
            else
            {
                ToStr += d.ToString();
            }
        }
        Console.Write(ToStr);
        Console.WriteLine();
    }
}