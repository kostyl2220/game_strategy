using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour {
    public Grid grid;

    private bool isSelecting = false;
    private Vector3 OldMousePosition;
    private Vector3 OldMousePosition1;

    private float unitDistanceX = 1f;
    private float unitDistanceZ = 1f;

    private List<Unit> selected_units;
    public static PathfindingA pathfinding;
    // Use this for initialization
    void Start () {
        selected_units = new List<Unit>();
        pathfinding = new PathfindingA(grid.CountInX, grid.CountInZ, grid.GetGrid());
    }

    // Update is called once per frame
    void Update()
    {
        if (!grid.IsEditing() && !Managers.Store.store.button.IsStoreOpened())
        {
            // If we press the left mouse button, save mouse location and begin selection
            if (Input.GetMouseButtonDown(0))
            {
                isSelecting = true;
                OldMousePosition = Input.mousePosition;
            }
            // If we let go of the left mouse button, end selection
            if (Input.GetMouseButtonUp(0))
            {

                if (Input.mousePosition == OldMousePosition)
                {
                    PointToObject(Input.mousePosition);
                }
                else
                {
                    selected_units.Clear();
                    foreach (Unit unit in Managers.Units.GetAllUnits())
                    {
                        bool select = IsWithinSelectionBounds(unit.gameObject);
                        unit.Select(select);
                        if (select)
                        {
                            selected_units.Add(unit);
                        }
                    }
                }
                isSelecting = false;
            }

            if (Input.GetMouseButtonDown(1))
            {
                OldMousePosition1 = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                if (OldMousePosition1 == Input.mousePosition && selected_units.Count != 0)
                    PointToPosition(Input.mousePosition);
            }
        }
    }

    void PointToObject(Vector2 TouchPosition)
    {
        int layer_mask = LayerMask.GetMask("Item");
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(TouchPosition);
        if (Physics.Raycast(ray, out hit, 20f, layer_mask))
        {
            Item item = hit.collider.GetComponent<Item>();
            if (item)
            {
                OpenInfo(item);
            }

            Unit unit = hit.collider.GetComponent<Unit>();
            if (unit)
            {
                foreach (Unit sel_unit in selected_units)
                    sel_unit.Select(false);

                if (selected_units.Count == 1 &&
                    selected_units[0].GetUniqueID() == unit.GetUniqueID())
                {
                    selected_units.Clear();
                    return;
                }

                selected_units.Clear();
                selected_units.Add(unit);
                unit.Select(true);
            }
        }
    }

    void OpenInfo(Item item)
    {
        if (grid.itemInfo.SetInfo(item))
        {
            grid.itemInfo.Open();
        }
        else
        {
            grid.itemInfo.OnInfoClose();
        }
    }

    void PointToPosition(Vector2 TouchPosition)
    {
        int layer_mask = LayerMask.GetMask("Grid");
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(TouchPosition);
        if (Physics.Raycast(ray, out hit, 20f, layer_mask))
        {
            FindPath(hit.point);
            //MoveUnits(hit.point);
        }
    }

    public Vector3 GetUnitCenter()
    {
        Vector3 center = new Vector3();

        foreach(var unit in selected_units)
        {
            center += unit.transform.position;
        }
        center.x /= selected_units.Count;
        center.y /= selected_units.Count;

        return center;
    }

    public void FindPath(Vector3 EndPos)
    {
        Vector2 center_end_pos = grid.GetPointByPosition(EndPos);
        List<Vector3> Cells = GetMoveCells(selected_units[0].transform.position, EndPos, selected_units.Count);
        if (grid.InRangePoint(center_end_pos.x + 1, center_end_pos.y + 1))
        {
            pathfinding.SetGrid(grid.GetGrid());

            for (int i = 0; i < selected_units.Count; ++i)
            {
                Unit unit = selected_units[i];

                Vector2 start_pos = grid.GetPointByPosition(unit.transform.position);
                Vector2 end_pos = grid.GetPointByPosition(Cells[i]);
                Vector2 EndDirection = Cells[i] - unit.transform.position;
                List<Node> Nodes = pathfinding.FindPath(start_pos, end_pos);
                Nodes.Add(new Node(new Vector2(Cells[i].x, Cells[i].z), null));
                Nodes.RemoveAt(0);
                unit.SetGrid(grid);
                unit.SetEndDirection(EndDirection);
                unit.MoveByPoints(Nodes);
            }
        }
    }

    public List<Vector3> GetMoveCells(Vector3 start_pos, Vector3 end_pos, int unitCount)
    {
        List<Vector3> PosList = new List<Vector3>();
        Vector3 direction = (end_pos - start_pos).normalized;
        Vector3 right = Quaternion.Euler(0, 90f, 0) * direction;
        int countInLine = (int)Mathf.Ceil(Mathf.Sqrt(unitCount));
        Debug.Log(countInLine);
        for (int i = 0; i < countInLine; ++i)
        {
            float MoveX = i - (countInLine - 1) / 2;

            for (int j = 0; j < countInLine; ++j)
            {
                if (unitCount == 0)
                    return PosList;

                float MoveZ = j - (countInLine - 1) / 2;

                Vector3 pos = end_pos - direction * j + right * i;
                PosList.Add(pos);
                unitCount--;
            }
        }
        return PosList; 
    }

    public void MoveByPoints(List<Node> Nodes)
    {
        Vector2 StartMovePos = Nodes[0].GetPosition();
        Vector2 EndMovePos = Nodes[Nodes.Count - 1].GetPosition();
        foreach(var unit in selected_units)
        {
            Vector2 start_local_pos = grid.GetPointByPosition(unit.transform.position);
            Vector2 end_local_pos = start_local_pos + EndMovePos - StartMovePos;

            pathfinding.FindPath(start_local_pos, StartMovePos);
        }
    }

    public void MoveUnits(Vector3 position)
    {
        int unitCount = selected_units.Count;
        int countInLine = (int)Mathf.Ceil(Mathf.Sqrt(unitCount));


        for (int i = 0; i < countInLine; ++i)
        {
            float MoveX = i - (countInLine - 1) / 2;
            for (int j = 0; j < countInLine; ++j)
            {
                if (unitCount == 0)
                    return;

                float MoveZ = j - (countInLine - 1) / 2;
                Vector3 newUnitPos = new Vector3(position.x + MoveX * unitDistanceX, position.y, position.z + MoveZ * unitDistanceZ);
                selected_units[i * countInLine + j].Move(newUnitPos);

                unitCount--;
            }
        }
    }

    public bool IsWithinSelectionBounds(GameObject gameObject)
    {
        if (!isSelecting)
            return false;

        var camera = Camera.main;
        var viewportBounds =
            SelectionUtils.GetViewportBounds(camera, OldMousePosition, Input.mousePosition);

        return viewportBounds.Contains(
            camera.WorldToViewportPoint(gameObject.transform.position));
    }
    
    public void UnSelectAll()
    {
        foreach (var unit in selected_units)
            unit.Select(false);

        selected_units.Clear();
    }

    void OnGUI()
    {
        if (!grid.IsEditing() && 
            !Managers.Store.store.button.IsStoreOpened()
            && isSelecting)
        {
            // Create a rect from both mouse positions
            var rect = SelectionUtils.GetScreenRect(OldMousePosition, Input.mousePosition);
            SelectionUtils.DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
            SelectionUtils.DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
        }
    }
}
