using System;
using System.Collections.Generic;

public class Bot
{
    public BotAI ai;
    public BotState state;
    public List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> aiList;

    public Bot(BotAI ai, BotState state,
        List<Tuple<Dictionary<Condition, bool>, List<Tuple<Dictionary<Condition, bool>, BotBehavior>>>> aiList)
    {
        this.ai = ai;
        this.state = state;
        this.aiList = aiList;
    }

    public void reset()
    {
        state = new BotState(state.botNumber);
        aiList = null;
    }

    public override string ToString()
    {
        var ret = "";
        ret += "uid:" + state.uid + "\n";
        ret += "ai:" + (aiList != null ? Bots.printAIPriorityList(aiList, state) : "null") + "\n";
        ret += "state: " + state + "\n";
        return ret;
    }
}