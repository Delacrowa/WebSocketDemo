using System;
using UnityEngine;

[Serializable]
public class StringPair
{
    public string s1;
    [TextArea(15, 20)] public string s2;

    public StringPair(string s1, string s2)
    {
        this.s1 = s1;
        this.s2 = s2;
    }

    //public override bool Equals(object obj)
    //{
    //    var pair = obj as StringPair;
    //    return pair != null &&
    //           s1 == pair.s1;
    //}
}