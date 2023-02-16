using System;
using System.Collections.Generic;
using UnityEngine;

public static class NetDebug
{
    private static string netDebug
    {
        get
        {
            var textToPrint = textSaved;
            var netDebugLines = textToPrint.Split('\n').Length;
            if (netDebugLines > netLinesMax)
            {
                var statements = textToPrint.Split('\n');
                //print("statementsL = " + statements.Length);
                textToPrint = "";
                for (var i = statements.Length - netLinesMax; i < statements.Length; i++)
                {
                    var end = "\n";
                    if (i == statements.Length - 1)
                    {
                        end = "";
                    }
                    textToPrint = textToPrint + statements[i] + end;
                }
            }
            return textToPrint;
        }
        set => textSaved = value;
    }

    private static readonly int
        debugLevel = 7; //higher the level, the less that gets printed. 0-7. 7 is for player ready version
    private static readonly int netLinesMax = 10;
    private static readonly List<string> usedDebugStrings = new();
    private static string textSaved = "";

    public static string getText() =>
        netDebug;

    public static void printBoth(string str, int level)
    {
        if (level >= debugLevel)
        {
            printDebug(str, level);
            Debug.Log(str + " " + DateTime.Now.ToString("h:mm:ss tt"));
        }
    }

    public static void printBoth(string str)
    {
        var level = debugLevel;
        if (level >= debugLevel)
        {
            printDebug(str, level);
            Debug.Log(str + " " + DateTime.Now.ToString("h:mm:ss tt"));
        }
    }

    //public static void setDebugText(Text inText)
    //{
    //    netDebug = inText;
    //}

    public static void printDebug(string statement, int level)
    {
        if (level >= debugLevel)
        {
            textSaved = textSaved + "\n" + statement;
        }
    }

    public static string printDictionaryKeys<T1, T2>(Dictionary<T1, T2> cb)
    {
        var keys = "";
        foreach (var k in cb.Keys)
        {
            keys = keys + " " + k;
        }
        return keys;
    }

    /// <summary>
    ///     Checks if already printed statementPrefix, then won't print again
    /// </summary>
    /// <param name="statementPreFix"></param>
    /// <param name="statementSuffix"></param>
    public static void printOnceDebug(string statementPreFix, string statementSuffix)
    {
        if (!usedDebugStrings.Contains(statementPreFix))
        {
            printDebug(statementPreFix + statementSuffix, 7);
            usedDebugStrings.Add(statementPreFix);
        }
    }
}