using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class Node
    {
        private Vector2 Point;
        public Node Parent { get; }
        private float G;
        private float F;

        public Node(Vector2 Pos, Node parent)
        {
            Point = Pos;
            Parent = parent;
        }

        public void SetG(float distance)
        {
            if (Parent == null)
            {
                G = 0;
                return;
            }
            G = Parent.G + distance;
        }

        public float GetF()
        {
            return F;
        }

        public float GetG()
        {
            return G;
        }

        public void SetF(float evristic)
        {
            F = G + evristic;
        } 

        public Vector2 GetPosition()
        {
            return Point;
        }
    }

    public class PathfindingA
    {
        private int SizeX;
        private int SizeY;
        private int[,] Grid;
        private List<Node> Opened;
        private List<Node> Closed;
        private Vector2 EndPos;

        public PathfindingA(int SizeX, int SizeY, int[,] Grid)
        {
            this.SizeX = SizeX;
            this.SizeY = SizeY;

            Opened = new List<Node>();
            Closed = new List<Node>();


        }

        public void SetGrid(int [,] grid)
        {
            Grid = grid;
        }

        public List<Node> SimplifyPath(List<Node> list)
        {
            int index = 1;
            while(index < list.Count - 1)
            {
                Vector2 pos1 = list[index - 1].GetPosition();
                Vector2 pos2 = list[index].GetPosition();
                Vector3 pos3 = list[index + 1].GetPosition();

                if (pos1.x == pos2.x && pos2.x == pos3.x
                    || pos1.y == pos2.y && pos2.y == pos3.y
                    || Mathf.Abs(pos1.x - pos2.x) == Mathf.Abs(pos1.y - pos2.y)
                    && Mathf.Abs(pos2.x - pos3.x) == Mathf.Abs(pos2.y - pos3.y))
                {
                    list.RemoveAt(index);
                }
                else
                    index++;
            }

            return list;
        }

        public bool HasPath(Vector2 StartPos, Vector2 EndPos)
        {
            List<Node> list = FindPath(StartPos, EndPos);
            return list.Count != 0;           
        }

        public List<Node> FindPath(Vector2 StartPos, Vector2 EndPos, float minPosDistance = 0f)
        {
            Opened.Clear();
            Closed.Clear();
            this.EndPos = EndPos;
            Node StartNode = new Node(StartPos, null);
            StartNode.SetG(0);
            StartNode.SetF(GetEuristic(StartNode, EndPos));
            Opened.Add(StartNode);
            int Count = 0;
            while (Opened.Count != 0)
            {
                ++Count;
                int smallestIndex = Smallest(Opened);
                Node current = Opened[smallestIndex];
                //Debug.Log(String.Format("Node: {0}, G: {1}, F: {2}", current.GetPosition(), current.GetG(), current.GetF()));
                Opened.RemoveAt(smallestIndex);

                if (current.GetPosition() == EndPos)
                {
                    //WE FOUND PATH

                    List<Node> path = new List<Node>();
                    while (current != null)
                    {
                        path.Add(current);
                        current = current.Parent;
                    }

                    path.Reverse();

                    //Debug.Log("All done");
                    //Debug.Log(Count);
                    return SimplifyPath(path);
                }

                List<Node> newNodes = GetNearest(current);
                foreach (var node in newNodes)
                {
                    if (Closed.Exists(x => x.GetPosition() == node.GetPosition()))
                    {
                        continue;
                    }

                    //Debug.Log(String.Format("Add: {0}, G: {1}, F: {2}", node.GetPosition(), node.GetG(), node.GetF()));

                    Node same = Opened.Find(x => x.GetPosition() == node.GetPosition());
                    if (same == null) {
                        Opened.Add(node);
                    }
                    else
                    {
                        if (same.GetF() > node.GetF())
                        {
                            Opened.Remove(same);
                            Opened.Add(node);
                        }
                    }
                }
                Closed.Add(current);
            }
            //Debug.Log(Count);
            return new List<Node>();
        }

        private int Smallest(List<Node> Nodes)
        {
            int index = 0;
            float smallest = Nodes[0].GetF();

            for (int i = 1; i < Nodes.Count; ++i)
            {
                float curF = Nodes[i].GetF();
                if (curF < smallest)
                {
                    smallest = curF;
                    index = i;
                }
            }

            return index;
        }

        private float GetEuristic(Node node, Vector2 endPos)
        {
            return Mathf.Sqrt(Mathf.Pow(node.GetPosition().x - endPos.x, 2) + Mathf.Pow(node.GetPosition().y - endPos.y, 2));
        }

        private List<Node> GetNearest(Node node)
        {
            List<Node> nearest = new List<Node>();

            int x = (int)node.GetPosition().x;
            int y = (int)node.GetPosition().y;

            float Sqrt2 = Mathf.Sqrt(2);
            if (x > 0 && Grid[x - 1, y] == 0)
            {
                Node Left = new Node(new Vector2(x - 1, y), node);
                Left.SetG(1);
                Left.SetF(GetEuristic(Left, EndPos));
                nearest.Add(Left);

                if (y > 0 && Grid[x - 1, y - 1] == 0)
                {
                    Node DownLeft = new Node(new Vector2(x - 1, y - 1), node);
                    DownLeft.SetG(Sqrt2);
                    DownLeft.SetF(GetEuristic(DownLeft, EndPos));
                    nearest.Add(DownLeft);
                }

                if (y < SizeY - 1 && Grid[x - 1, y + 1] == 0)
                {
                    Node UpLeft = new Node(new Vector2(x - 1, y + 1), node);
                    UpLeft.SetG(Sqrt2);
                    UpLeft.SetF(GetEuristic(UpLeft, EndPos));
                    nearest.Add(UpLeft);
                }
            }

            if (x < SizeX - 1 && Grid[x + 1, y] == 0)
            {
                Node Right = new Node(new Vector2(x + 1, y), node);
                Right.SetG(1);
                Right.SetF(GetEuristic(Right, EndPos));
                nearest.Add(Right);

                if (y > 0 && Grid[x + 1, y - 1] == 0)
                {
                    Node DownRight = new Node(new Vector2(x + 1, y - 1), node);
                    DownRight.SetG(Sqrt2);
                    DownRight.SetF(GetEuristic(DownRight, EndPos));
                    nearest.Add(DownRight);
                }

                if (y < SizeY - 1 && Grid[x + 1, y + 1] == 0)
                {
                    Node UpRight = new Node(new Vector2(x + 1, y + 1), node);
                    UpRight.SetG(Sqrt2);
                    UpRight.SetF(GetEuristic(UpRight, EndPos));
                    nearest.Add(UpRight);
                }
            }

            if (y > 0 && Grid[x, y - 1] == 0)
            {
                Node Down = new Node(new Vector2(x, y - 1), node);
                Down.SetG(1);
                Down.SetF(GetEuristic(Down, EndPos));
                nearest.Add(Down);
            }


            if (y < SizeY - 1 && Grid[x, y + 1] == 0)
            {
                Node Up = new Node(new Vector2(x, y + 1), node);
                Up.SetG(1);
                Up.SetF(GetEuristic(Up, EndPos));
                nearest.Add(Up);
            }

            return nearest;
        }
    }
}
