using System;
using System.Collections.Generic;

[Serializable]
public class UserInput : Message
{
    public float x;
    public float y;
    public List<bool> buttonsDown;
    public SerializableVector3 target;
    public bool equipedSlot1 = true; // need to default to true or else might equip slot2 which is empty

    public override string ToString()
    {
        var ret = "" + x + "," + y;
        if (buttonsDown != null)
        {
            for (var i = 0; i < buttonsDown.Count; i++)
            {
                ret += " b" + i + ":" + buttonsDown[i];
            }
        }
        ret += " target: " + target.x + "," + target.y;
        return ret;
    }
}