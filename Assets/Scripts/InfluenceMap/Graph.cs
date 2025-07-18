using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System;

public class Node
{
    // TODO : use an int id for nodes comparison
    public Vector3 Position = Vector3.zero;
    public List<Node> Neighbours;
}

public class Connection
{
    public int Cost;
    public Node FromNode;
    public Node ToNode;
}

public class Graph : MonoBehaviour
{
    [SerializeField]
    protected int GridSizeH = 100;
    [SerializeField]
    protected int GridSizeV = 100;
    [SerializeField]
    protected int SquareSize = 1;
    [SerializeField]
    protected float MaxHeight = 10f;

    // enable / disable debug Gizmos
    [SerializeField]
    protected bool DrawGrid = false;
    [SerializeField]
    protected bool DrawNodes = false;
    [SerializeField]
    protected bool DrawConnections = false;

    // threading
    protected Thread GraphThread = null;

    // Grid parameters
    protected Vector3 GridStartPos = Vector3.zero;
    protected int NbTilesH = 0;
    protected int NbTilesV = 0;

    // Nodes
    public Node[] NodeList = new Node[0];

    protected Dictionary<Node, List<Connection>> ConnectionGraph = new Dictionary<Node, List<Connection>>();


    protected Dictionary<Node, Connection[]> ConnectionGraph2 = new Dictionary<Node, Connection[]>();

    public Action OnGraphCreated;

    public Dictionary<Node, Connection[]> GetConnectionGraph2()
    {
        return ConnectionGraph2;
    }

    public Dictionary<Node, List<Connection>> GetConnectionGraph()
    {
        return ConnectionGraph;
    }

    private void Awake()
    {
        CreateTiledGrid();
    }

    private void Start()
    {
        // Generate graph in a new thread
        ThreadStart threadStart = new ThreadStart(CreateGraph);
        GraphThread = new Thread(threadStart);
        GraphThread.Start();
    }

    #region graph construction

    // Node factory for class specific nodes
    protected virtual Node CreateNode()
    {
        return new Node();
    }

    // Create all nodes for the tiled grid
    protected void CreateTiledGrid()
    {
        GridStartPos = transform.position + new Vector3(-GridSizeH / 2f, 0f, -GridSizeV / 2f);

        NbTilesH = GridSizeH / SquareSize;
        NbTilesV = GridSizeV / SquareSize;

        NodeList = new Node[NbTilesH *  NbTilesV];

        for (int i = 0; i < NbTilesV; i++)
        {
            for (int j = 0; j < NbTilesH; j++)
            {
                Vector3 nodePos = GridStartPos + new Vector3((j + 0.5f) * SquareSize, 0f, (i + 0.5f) * SquareSize);

                NodeList[i * NbTilesH + j] = CreateAndSetupNode(nodePos);
            }
        }
    }

    virtual protected Node CreateAndSetupNode(Vector3 pos)
    {
        RaycastHit hitInfo = new RaycastHit();

        // Always compute node Y pos from floor collision
        if (Physics.Raycast(pos + Vector3.up * MaxHeight, Vector3.down, out hitInfo, MaxHeight + 1, 1 << LayerMask.NameToLayer("Floor")))
        {
            pos.y = hitInfo.point.y;
        }

        Node node = CreateNode();
        node.Position = pos;

        return node;
    }

    virtual protected Connection CreateConnection(Node from, Node to)
    {
        Connection connection = new Connection();
        connection.FromNode = from;
        connection.ToNode = to;

        return connection;
    }

    // Compute possible connections between each nodes
    virtual protected void CreateGraph()
    {
        foreach (Node node in NodeList)
        {
            if (IsNodeValid(node))
            {
                ConnectionGraph.Add(node, new List<Connection>());
                node.Neighbours = GetNeighbours(node); // cache neighbours list
                foreach (Node neighbour in node.Neighbours)
                {
                    ConnectionGraph[node].Add(CreateConnection(node, neighbour));
                }
            }
        }
        OnGraphCreated?.Invoke();
    }

    public bool IsPosValid(Vector3 pos)
    {
        if (GraphThread != null && GraphThread.ThreadState == ThreadState.Running)
            return false;

        if (pos.x > (-GridSizeH / 2) && pos.x < (GridSizeH / 2) && pos.z > (-GridSizeV / 2) && pos.z < (GridSizeV / 2))
            return true;
        return false;
    }

    // Converts world 3d pos to tile 2d pos
    public Vector2Int GetTileCoordFromPos(Vector3 pos)
    {
        Vector3 realPos = pos - GridStartPos;
        Vector2Int tileCoords = Vector2Int.zero;
        tileCoords.x = Mathf.FloorToInt(realPos.x / SquareSize);
        tileCoords.y = Mathf.FloorToInt(realPos.z / SquareSize);
        return tileCoords;
    }

    public Node GetNode(Vector3 pos)
    {
        return GetNode(GetTileCoordFromPos(pos));
    }

    public Node GetNode(Vector2Int pos)
    {
        return GetNode(pos.x, pos.y);
    }

    protected Node GetNode(int x, int y)
    {
        int index = y * NbTilesH + x;

        if (index >= NodeList.Length || index < 0)
            return null;

        return NodeList[index];

    }

    virtual protected bool IsNodeValid(Node node)
    {
        return node != null;
    }

    private void AddNode(List<Node> list, Node node)
    {
        if (IsNodeValid(node))
            list.Add(node);
    }

    #endregion

    virtual protected List<Node> GetNeighbours(Node node)
    {
        Vector2Int tileCoord = GetTileCoordFromPos(node.Position);
        int x = tileCoord.x;
        int y = tileCoord.y;

        List<Node> nodes = new List<Node>();

        if (x > 0)
        {
            if (y > 0)
            {
                AddNode(nodes, GetNode(x - 1, y - 1));
            }
            AddNode(nodes, NodeList[(x - 1) + y * NbTilesH]);
            if (y < NbTilesV - 1)
            {
                AddNode(nodes, NodeList[(x - 1) + (y + 1) * NbTilesH]);
            }
        }

        if (y > 0)
        {
            AddNode(nodes, NodeList[x + (y - 1) * NbTilesH]);
        }
        if (y < NbTilesV - 1)
        {
            AddNode(nodes, NodeList[x + (y + 1) * NbTilesH]);
        }

        if (x < NbTilesH - 1)
        {
            if (y > 0)
            {
                AddNode(nodes, NodeList[(x + 1) + (y - 1) * NbTilesH]);
            }
            AddNode(nodes, NodeList[(x + 1) + y * NbTilesH]);
            if (y < NbTilesV - 1)
            {
                AddNode(nodes, NodeList[(x + 1) + (y + 1) * NbTilesH]);
            }
        }

        return nodes;
    }


    #region Gizmos

    protected virtual void DrawGridGizmo()
    {
        float gridHeight = 0.01f;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < NbTilesV + 1; i++)
        {
            Vector3 startPos = new Vector3(-GridSizeH / 2f, gridHeight, -GridSizeV / 2f + i * SquareSize);
            Gizmos.DrawLine(startPos, startPos + Vector3.right * GridSizeV);

            for (int j = 0; j < NbTilesH + 1; j++)
            {
                startPos = new Vector3(-GridSizeH / 2f + j * SquareSize, gridHeight, -GridSizeV / 2f);
                Gizmos.DrawLine(startPos, startPos + Vector3.forward * GridSizeV);
            }
        }
    }
    protected virtual void DrawConnectionsGizmo()
    {
        foreach (Node crtNode in NodeList)
        {
            if (ConnectionGraph.ContainsKey(crtNode))
            {
                foreach (Connection c in ConnectionGraph[crtNode])
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(c.FromNode.Position, c.ToNode.Position);
                }
            }
        }
    }
    protected virtual void DrawNodesGizmo()
    {
        for (int i = 0; i < NodeList.Length; i++)
        {
            Node node = NodeList[i];
            Gizmos.color = Color.white;
            Gizmos.DrawCube(node.Position, Vector3.one * SquareSize * 0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (DrawGrid)
        {
            DrawGridGizmo();
        }
        if (DrawNodes)
        {
            DrawNodesGizmo();
        }
        if (DrawConnections)
        {
            DrawConnectionsGizmo();
        }
    }
    #endregion
}
