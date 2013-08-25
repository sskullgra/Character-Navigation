using UnityEngine;
using System.Collections.Generic;

public class Coordinate
{
    public int x;
    public int y;

    public Coordinate(int X, int Y){
        x = X;
        y = Y;
    }
}

public class Node
{
    public float f;
    public readonly float h = 0; //estimated cost to goal
    public float g = 0; //cost, from start (shortest path)

    public List<Node> neighbors = null;

    public readonly int x = 0, y = 0;
    public readonly Vector3 pos;

    public Node parent = null;

    public readonly bool isGoal = false;

    public Node(Node Parent, int X, int Y, Vector3 Pos, Vector3 Goal)
    {
        if(Parent != null){
            parent = Parent;
            g = parent.g + estimate(parent.pos, Pos); ;
        }        
        x = X; y = Y;
        pos = Pos;
        isGoal = (Pos == Goal);
        h = estimate(Pos, Goal);
        f = g + h;
    }

    public Node(Node Parent, int X, int Y, Vector3? Pos, Vector3? Goal)
    {
        if (Parent != null)
        {
            parent = Parent;
            g = parent.g + estimate(parent.pos, Pos.Value); ;
        }
        x = X; y = Y;
        pos = Pos.Value;
        isGoal = (Pos == Goal.Value);
        h = estimate(Pos.Value, Goal.Value);
        f = g + h;
    }

    public static float estimate(Vector3 cur, Vector3 goal){
        float diagonal = Mathf.Min(Mathf.Abs(cur.x - goal.x), Mathf.Abs(cur.z - goal.z));
        return 1.414213562373095f * diagonal + ((Mathf.Abs(cur.x - goal.x) + Mathf.Abs(cur.z - goal.z)) - 2 * diagonal);
    }

    public bool Equals(Node item){
        if(item == null) return false;
        return (x == item.x && y == item.y);
    }
}

public class NodeComparer : IComparer<Node>
{
    public int Compare(Node x, Node y)
    {
        if (x.f > y.f)
        {
            return -1;
        }
        else if (x.f < y.f)
        {
            return 1;
        }
        else
        {
            if (x.h > y.h)
            {
                return -1;
            }
            else if (x.h < y.h)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}