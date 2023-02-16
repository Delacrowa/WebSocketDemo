using System.IO;
using System.Text;
using UnityEngine;

public class DebugLogWriter : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(string value)
    {
        base.Write(value);
        Debug.Log(value);
        NetDebug.printBoth(value);
    }

    public override void WriteLine(string value)
    {
        base.WriteLine();
        Debug.Log(value);
        NetDebug.printBoth(value);
    }
}