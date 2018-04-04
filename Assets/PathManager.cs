using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> NeighboringNodes { get; private set; }
    public Node Previous { get; set; }
    public float FScore { get; set; }
    public int GScore { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsObstacle { get; set; }

    public Node(
        int row,
        int column
    )
    {
        NeighboringNodes = new List<Node>();
        FScore = 0.0f;
        GScore = 0;
        Row = row;
        Column = column;
        Previous = null;
    }

    public void AddNeighboringNode(Node node)
    {
        NeighboringNodes.Add(node);
    }
};

public class PathManager : MonoBehaviour
{
    public List<Node[]> NodeMatrix { get; private set; }
    public List<Node> OptimalPath { get; private set; }
    public List<Node> OpenSet { get; private set; }
    public List<Node> ClosedSet { get; private set; }
    public Node Start { get; private set; }
    public Node End { get; private set; }
    public int CurrentGScore { get; private set; }
    private int currentRowIndex;
    private int currentColumnIndex;
    private float timer;
    private float columnTimer;
    private int multiplier;
    private int columnMultiplier;

    public int Width, Height;

    public static void DrawNode(
        Node node,
        Color color
    )
    {
        Color c = node.IsObstacle ? Color.black : color;
        Vector2 nodePosition = new Vector2(node.Column, node.Row);
        Debug.DrawRay(nodePosition, new Vector2(-0.05f, 0.05f), c);
        Debug.DrawRay(nodePosition, new Vector2(0.05f, 0.05f), c);
        Debug.DrawRay(nodePosition, new Vector2(0.05f, -0.05f), c);
        Debug.DrawRay(nodePosition, new Vector2(-0.05f, -0.05f), c);
    }

    public void PushPath(Node node)
    {
        Node temp = node;
        OptimalPath.Clear();
        OptimalPath.Add(node);
        while (temp.Previous != null)
        {
            OptimalPath.Add(temp.Previous);
            temp = temp.Previous;
        }
    }

    public void DrawPath(List<Node> list)
    {
        for (int i = 0; i < list.Count - 1; ++i)
        {
            Debug.DrawLine(new Vector2(list[i].Column, list[i].Row), new Vector2(list[i + 1].Column, list[i + 1].Row), Color.blue);
        }
    }

    public static float GetDistanceBetween(
        Node first,
        Node second
    )
    {
        return (new Vector2(second.Column, second.Row) - new Vector2(first.Column, first.Row)).magnitude;
    }

    private void Awake()
    {
        NodeMatrix = new List<Node[]>();
        OptimalPath = new List<Node>();

        if (Width <= 0 || Height <= 0)
        {
            return;
        };

        for (int i = 0; i < Height; ++i)
        {
            Node[] nodeRow = new Node[Width];

            for (int j = 0; j < Width; ++j)
            {
                Node node = new Node(i, j)
                {
                    IsObstacle = Random.Range(0.0f, 1.0f) < 0.40f
                };
                nodeRow[j] = node;
            }

            NodeMatrix.Add(nodeRow);
        }

        for (int i = 0; i < Height; ++i)
        {
            for (int j = 0; j < Width; ++j)
            {
                Node currentNode = NodeMatrix[i][j];

                if (i > 0)
                {
                    currentNode.AddNeighboringNode(NodeMatrix[i - 1][j]);
                }
                if (j > 0)
                {
                    currentNode.AddNeighboringNode(NodeMatrix[i][j - 1]);
                }
                if (j < Width - 1)
                {
                    currentNode.AddNeighboringNode(NodeMatrix[i][j + 1]);
                }
                if (i < Height - 1)
                {
                    currentNode.AddNeighboringNode(NodeMatrix[i + 1][j]);
                }
                if (i < Height - 1 && j < Width - 1)
                {
                    currentNode.AddNeighboringNode(NodeMatrix[i + 1][j + 1]);
                }
                if (j > 0 && i > 0)
                {
                    currentNode.AddNeighboringNode(NodeMatrix[i - 1][j - 1]);
                }
            }
        }

        ClosedSet = new List<Node>();
        OpenSet = new List<Node>
        {
            NodeMatrix[0][0]
        };

        Start = NodeMatrix[0][0];
        End = NodeMatrix[Width - 1][Height - 1];
        Start.GScore = 0;
        Start.FScore = float.MaxValue;
        Start.IsObstacle = false;
        End.IsObstacle = false;
        CurrentGScore = 0;
        currentColumnIndex = 0;
        currentRowIndex = 0;
        multiplier = 1;
        columnTimer = 0.0f;
        columnMultiplier = 1;
    }

    void Update()
    {
        timer += Time.deltaTime;
        columnTimer += Time.deltaTime;
        Node[] currentRow = NodeMatrix[currentRowIndex];
        Node currentNode = currentRow[currentColumnIndex];
        DrawNode(currentNode, Color.yellow);

        foreach (Node n in currentNode.NeighboringNodes.ToArray())
        {
            DrawNode(n, Color.red);
        }

        //if (columnTimer > columnMultiplier * 1.0f)
        //{
        //    if (currentColumnIndex < Width - 1)
        //    {
        //        ++currentColumnIndex;
        //    }
        //    else
        //    {
        //        currentColumnIndex = 0;
        //    }

        //    ++columnMultiplier;
        //}

        //if (timer > multiplier * 5.0f)
        //{
        //    if (currentRowIndex < Height - 1)
        //    {
        //        ++currentRowIndex;
        //    }
        //    else
        //    {
        //        currentRowIndex = 0;
        //    }

        //    ++multiplier;
        //}

        foreach (Node[] row in NodeMatrix)
        {
            foreach (Node n in row)
            {
                DrawNode(n, Color.yellow);
            }
        }

        DrawPath(OptimalPath);

        int openSetCount = OpenSet.Count;

        if (openSetCount > 0)
        {
            foreach(Node n in OpenSet.ToArray())
            {
                DrawNode(n, Color.green);
            }

            foreach (Node n in ClosedSet.ToArray())
            {
                DrawNode(n, Color.red);
            }

            Node current = OpenSet[0];

            for (int i = 0; i < openSetCount; ++i)
            {
                if (OpenSet[i].FScore < current.FScore)
                {
                    current = OpenSet[i];
                }
            }

            if (current == End)
            {
                //PushPath(current);
                return;
            }

            Debug.Log(OpenSet.Count);
            OpenSet.Remove(current);
            Debug.Log(OpenSet.Count);
            ClosedSet.Add(current);

            for (int i = 0; i < current.NeighboringNodes.Count; ++i)
            {
                Node currentNeighbor = current.NeighboringNodes[i];
                bool isNewPath = false;

                if (ClosedSet.Contains(currentNeighbor) || currentNeighbor.IsObstacle)
                {
                    continue;
                }

                // currentNeighbor is not evaluated yet

                int tentativeGScore = current.GScore + 1;

                if (OpenSet.Contains(currentNeighbor))
                {
                    // The neighbor exists in the open set. Check its GSCore against tentative

                    if (tentativeGScore < currentNeighbor.GScore)
                    {
                        currentNeighbor.GScore = tentativeGScore;
                        isNewPath = true;
                    }
                }
                else
                {
                    currentNeighbor.GScore = tentativeGScore;
                    OpenSet.Add(currentNeighbor);
                    isNewPath = true;
                }

                if (isNewPath)
                {
                    currentNeighbor.FScore = currentNeighbor.GScore + GetDistanceBetween(currentNeighbor, End);
                    currentNeighbor.Previous = current;
                    PushPath(currentNeighbor);
                }

                //int tentativeGScore = current.GScore + 1;

                //if (tentativeGScore >= currentNeighbor.GScore)
                //{
                //    continue;
                //}

                //currentNeighbor.GScore = tentativeGScore;
                //currentNeighbor.FScore = currentNeighbor.GScore + GetDistanceBetween(currentNeighbor, End);
            }
        }
    }
};