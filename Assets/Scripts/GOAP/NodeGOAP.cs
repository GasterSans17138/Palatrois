using System.Collections;

public class NodeGOAP
{
    public NodeGOAP parent;
    public float depth;
    public BitArray worldState;
    public GOAPActions action;

    public NodeGOAP(NodeGOAP _parent, BitArray _worldState, GOAPActions _action)
    {
        parent = _parent;
        depth = parent == null ? 0 : parent.depth + 1;
        worldState = _worldState;
        action = _action;
    }
}