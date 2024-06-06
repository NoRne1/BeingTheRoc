using System.Collections.Generic;
using UnityEngine;

public abstract class BTNode
{
    public abstract bool Execute();
}

public class SequenceNode : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public SequenceNode(List<BTNode> nodes)
    {
        children = nodes;
    }

    public override bool Execute()
    {
        foreach (BTNode node in children)
        {
            if (!node.Execute())
                return false;
        }
        return true;
    }
}

public class SelectorNode : BTNode
{
    private List<BTNode> children = new List<BTNode>();

    public SelectorNode(List<BTNode> nodes)
    {
        children = nodes;
    }

    public override bool Execute()
    {
        foreach (BTNode node in children)
        {
            if (node.Execute())
                return true;
        }
        return false;
    }
}

public class ConditionNode : BTNode
{
    private System.Func<bool> condition;

    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override bool Execute()
    {
        return condition();
    }
}

public class ActionNode : BTNode
{
    private System.Action action;

    public ActionNode(System.Action action)
    {
        this.action = action;
    }

    public override bool Execute()
    {
        action();
        return true;
    }
}
