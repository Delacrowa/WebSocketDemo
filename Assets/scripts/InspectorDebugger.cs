using System.Collections.Generic;
using UnityEngine;

public class InspectorDebugger : MonoBehaviour
{
    public List<StringPair> pairs;

    public void addPair(StringPair s)
    {
        var exists = pairs.FindIndex(sin => sin.s1 == s.s1);
        if (exists >= 0)
        {
            pairs[exists] = s;
        }
        else
        {
            pairs.Add(s);
        }
    }
}