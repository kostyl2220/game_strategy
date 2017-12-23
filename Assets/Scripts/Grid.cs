using Assets.Scripts;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour {

    private struct TempGrid
    {
        public int X;
        public int Z;
        public int SizeX;
        public int SizeZ;

        public TempGrid(int X, int Z, int SizeX, int SizeZ)
        {
            this.X = X;
            this.Z = Z;
            this.SizeX = SizeX;
            this.SizeZ = SizeZ;
        }

        public TempGrid(TempGrid other, int DeltaX, int DeltaZ)
        {
            this.X = other.X + DeltaX;
            this.Z = other.Z + DeltaZ;
            this.SizeX = other.SizeX;
            this.SizeZ = other.SizeZ;
        }

        public TempGrid(TempGrid other)
        {
            this.X = other.X;
            this.Z = other.Z;
            this.SizeZ = other.SizeX;
            this.SizeX = other.SizeZ;
        }
    }

    public PullableObject GreenPlane;
    public PullableObject RedPlane;
    public PullableObject GrayPlane;
    public GameObject ItemObjectPool;
    public GameObject TempStuff;
    public ItemInfo itemInfo;

    public GameObject BtnOk;
    public GameObject BtnCancel;
    public GameObject Btn90;
    public GameObject Btn_90;

    public float Size = 1f;
    public int CountInX = 20;
    public int CountInZ = 20;
   
    private GameObject PanelSon;

    private int[,] GridItems;
    private int[,] GridWithUnits;

    private TempGrid activeGrid;
    private int tempRotation;
    private Item ActiveItem;
    private List<PullableObject> activePlates;
    private bool CanPlace = false;

    private bool EditMode = false;
    private bool MoveActive = false;
    private Vector2 TouchPosition;

    //Object pools (Separate for Items and Planes)
    private List<PullableObject> PlaneObjectPool;

    public int[,] GetGrid()
    {
        return GridItems;
    }

    public void PlaceOnGrid(int item_id)
    {
        CreateItem(item_id);
        GridWithUnits = Managers.Units.AddUnitMask(GridItems);
        CreateCollideGrids();
        SetEnableButtons(true);
        itemInfo.OnInfoClose();
    }

    public void ShowGrid(bool show)
    {
        if (PanelSon)
            PanelSon.SetActive(show);
    }

    public bool IsEditing()
    {
        return EditMode;
    }

    public bool IsActiveItemMove()
    {
        return MoveActive;
    }

    public Vector3 GetPositionByXZ(float X, float Z, float OffsetY) // numeration from 1
    {
        var xPos = (X - (CountInX + 1) / 2f) * Size;
        var zPos = (Z - (CountInX + 1) / 2f) * Size;
        Vector3 center = transform.position;
        return new Vector3(center.x + xPos, center.y + OffsetY, center.x + zPos);
    }

    public void SetGrid(int [,] newGrid)
    {
        GridItems = newGrid;
    }

    // Use this for initialization
    void Start()
    {
        PlaneObjectPool = new List<PullableObject>();
        activePlates = new List<PullableObject>();
      

        PanelSon = transform.Find("Panels").gameObject;
        var xVar = (CountInX / 2f) * Size;
        var zVar = (CountInZ / 2f) * Size;
        for (float x = -xVar; x < xVar; x += Size)
        {
            for (float z = -zVar; z < zVar; z += Size)
            {
                var xPos = x + Size / 2f;
                var zPos = z + Size / 2f;

                Vector3 center = transform.position;
                Vector3 Position = new Vector3(center.x + xPos, center.y + 0.01f, center.z + zPos);
                PullableObject instance = GetPlaneFromPool(GrayPlane, Position);

                if (PanelSon)
                    instance.transform.parent = PanelSon.transform;
            }

        }
    }

    // Update is called once per frame
    void Update () {
        ControlPC();
        //ControlMobile();
    }

    /*void ControlMobile()
    {
        if (Input.touchCount == 1)
        {
            Touch LastTouch = Input.GetTouch(0);
            if (EditMode)
            {
                if (LastTouch.phase == TouchPhase.Began)
                {
                    PointToCell(LastTouch.position);
                }
                else if (MoveActive && LastTouch.phase == TouchPhase.Stationary)
                {
                    TryMoveItem(LastTouch.position);
                }
                if (LastTouch.phase == TouchPhase.Ended)
                {
                    MoveActive = false;
                }
            }
            else
            {
                if (LastTouch.phase == TouchPhase.Began)
                    PointToObject(LastTouch.position);
            }
        }
    }*/

    void ControlPC()
    {
        if (EditMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointToCell(Input.mousePosition);
            }
            else if (MoveActive && Input.GetMouseButton(0))
            {
                TryMoveItem(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                MoveActive = false;
            }
        }
    }

    void PointToCell(Vector2 TouchPosition)
    {
        int layer_mask = LayerMask.GetMask("Grid");
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(TouchPosition);
        if (Physics.Raycast(ray, out hit, 20f, layer_mask))
        {
            float X = GetXByPosition(hit.point);
            float Z = GetZByPosition(hit.point);
            if (OnActiveHit(X, Z))
            {
                this.TouchPosition = new Vector2(X, Z);
                MoveActive = true;
            }
        }
    }

    void TryMoveItem(Vector2 TouchPosition)
    {
        int layer_mask = LayerMask.GetMask("Grid");
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(TouchPosition);
        if (Physics.Raycast(ray, out hit, 20f, layer_mask))
        {
            float X = GetXByPosition(hit.point);
            float Z = GetZByPosition(hit.point);
            Vector2 NewPos = new Vector2(X, Z);
            if (this.TouchPosition != NewPos)
            {
                MoveItem(X - this.TouchPosition.x, Z - this.TouchPosition.y);
                this.TouchPosition = NewPos;
            }
        }
    }

    void MoveItem(float deltaX, float deltaZ)
    {
        TempGrid NewTempGrid = new TempGrid(activeGrid, (int)deltaX, (int)deltaZ);
        if (InRange(NewTempGrid))
        {
            activeGrid = NewTempGrid;
            Vector3 MoveVector = new Vector3(deltaX * Size, 0, deltaZ * Size);
            ActiveItem.transform.position += MoveVector;
            foreach (var plate in activePlates)
            {
                plate.transform.position += MoveVector;
            }
            CheckCollideGrids();
        }
    }

    public bool OnActiveHit(float X, float Z)
    {
        return X >= activeGrid.X && X <= activeGrid.X + activeGrid.SizeX - 1
            && Z >= activeGrid.Z && Z <= activeGrid.Z + activeGrid.SizeZ - 1;
    }

    bool InRange(TempGrid tempGrid)
    {
        return tempGrid.X >= 1 && tempGrid.X + tempGrid.SizeX - 1 <= CountInX
            && tempGrid.Z >= 1 && tempGrid.Z + tempGrid.SizeZ - 1<= CountInZ;
    }

    public bool InRangePoint(float X, float Z)
    {
        return X >= 1 && X <= CountInX
            && Z >= 1 && Z <= CountInZ;
    }

    void CheckCollideGrids()
    {
        int num = 0;
        CanPlace = true;
        for (int x = activeGrid.X; x < activeGrid.X + activeGrid.SizeX; ++x)
        {
            for (int z = activeGrid.Z; z < activeGrid.Z + activeGrid.SizeZ; ++z)
            {
                bool EnableToPlace = GridWithUnits[x - 1, z - 1] == 0;
                CanPlace = EnableToPlace == false ? false : CanPlace;
                activePlates[num].gameObject.SetActive(EnableToPlace);
                activePlates[num + 1].gameObject.SetActive(!EnableToPlace);
                num += 2;
            }
        }
    }

    void CreateCollideGrids()
    {
        DeleteActivePlates();
        CanPlace = true;
        for (int x = activeGrid.X; x < activeGrid.X + activeGrid.SizeX; ++x)
        {
            for (int z = activeGrid.Z; z < activeGrid.Z + activeGrid.SizeZ; ++z)
            {
                bool EnableToPlace = GridWithUnits[x - 1, z - 1] == 0;
                CanPlace = EnableToPlace == false ? false : CanPlace;
                PullableObject instanceGreen = GetPlaneFromPool(GreenPlane, GetPositionByXZ(x, z, 0.02f));
                PullableObject instanceRed = GetPlaneFromPool(RedPlane, GetPositionByXZ(x, z, 0.02f));
                instanceGreen.gameObject.SetActive(EnableToPlace);
                instanceRed.gameObject.SetActive(!EnableToPlace);
                activePlates.Add(instanceGreen);
                activePlates.Add(instanceRed);
            }
        }
    }

    public void CreateGreenLine(List<Node> nodes)
    {
        foreach(var node in nodes)
        {
            int x = (int)node.GetPosition().x + 1;
            int z = (int)node.GetPosition().y + 1;
            PullableObject instanceGreen = GetPlaneFromPool(GreenPlane, GetPositionByXZ(x, z, 0.02f));
        }
    }

    void MixGrids(int NewID)
    {
        for (int x = activeGrid.X; x < activeGrid.X + activeGrid.SizeX; ++x)
        {
            for (int z = activeGrid.Z; z < activeGrid.Z + activeGrid.SizeZ; ++z)
            {
                GridItems[x - 1, z - 1] = NewID;
            }
        }
    }

    void DeleteActivePlates()
    {
        if (activePlates.Count != 0)
        {
            foreach (var plate in activePlates)
                ReturnToPlanePool(plate);
            activePlates.Clear();
        }
    }

    void DeleteActiveItem()
    {
        if (ActiveItem)
        {
            ItemManager.ReturnToPool(ActiveItem);
        }
        ActiveItem = null;
    }

    void CreateItem(int item_id)
    {
        DeleteActiveItem();
        int CenterX = (int)Mathf.Floor((CountInX + 1) / 2f);
        int CenterZ = (int)Mathf.Floor((CountInZ + 1) / 2f);

        Item item = ItemManager.GetItemByID(item_id);
        activeGrid = new TempGrid(CenterX, CenterZ, item.SizeX, item.SizeZ);
        tempRotation = 0;
        ActiveItem = CreateItem(item_id, CenterX + (item.SizeX - 1) / 2f, CenterZ + (item.SizeZ - 1) / 2f);
        ActiveItem.GetComponent<Collider>().enabled = false;
    }

    Item CreateItem(int item_id, float X, float Z)
    {
        Vector3 Position = GetPositionByXZ(X, Z, 0);
        return ItemManager.GetItemFromPool(item_id, Position);
    }

    public int GetXByPosition(Vector3 Position)
    {
        Vector3 center = transform.position;
        float diff = (Position.x - center.x) / Size;
        return (int)Mathf.Ceil(CountInX / 2f + diff);
    }

    public int GetZByPosition(Vector3 Position)
    {
        Vector3 center = transform.position;
        float diff = (Position.z - center.z) / Size;
        return (int)Mathf.Ceil(CountInZ / 2f + diff);
    }

    public Vector2 GetPointByPosition(Vector3 Position) // numeration from 0
    {
        Vector3 center = transform.position;
        float diffX = (Position.x - center.x) / Size;
        float diffY = (Position.z - center.z) / Size;
        return new Vector2((int)Mathf.Ceil(CountInX / 2f + diffX - 1), (int)Mathf.Ceil(CountInZ / 2f + diffY - 1));
    }

    //methods for buttons Ok/Cancel
    public void OnConfirmPlace()
    {
        if (CanPlace)
        {
            CanPlace = false;
            int NewID = ItemManager.AddItemToPool(ActiveItem, new Vector2(activeGrid.X, activeGrid.Z), tempRotation);
            ActiveItem = null;
            MixGrids(NewID);

            DeleteActivePlates();
            SetEnableButtons(false);
        }
    }

    public void OnDenyPlace()
    {
        DeleteActiveItem();
        DeleteActivePlates();

        SetEnableButtons(false);
    }

    void SetEnableButtons(bool enable)
    {
        EditMode = enable;
        BtnCancel.SetActive(enable);
        BtnOk.SetActive(enable);
        Btn_90.SetActive(enable);
        Btn90.SetActive(enable);
    }

    public void Rotate90()
    {
        RotateOn90(true);
        tempRotation += 90;
        if (tempRotation == 360)
            tempRotation = 0;
    }

    public void Rotate_90()
    {
        RotateOn90(false);
        tempRotation -= 90;
        if (tempRotation == -90)
            tempRotation = 270;
    }

    void RotateOn90(bool plus)
    {
        if (activeGrid.SizeX == activeGrid.SizeZ)
        {
            ActiveItem.transform.rotation = Quaternion.Euler(0, (plus ? 1 : - 1) * 90f, 0) * ActiveItem.transform.rotation;
        }
        else
        {
            TempGrid NewGrid = new TempGrid(activeGrid);
            if (InRange(NewGrid))
            {
                activeGrid = NewGrid;
                CreateCollideGrids();
                ActiveItem.transform.rotation = Quaternion.Euler(0, (plus ? 1 : -1) * 90f, 0) * ActiveItem.transform.rotation;
                Vector3 Position = GetPositionByXZ(activeGrid.X + (activeGrid.SizeX - 1) / 2f, activeGrid.Z + (activeGrid.SizeZ - 1) / 2f, 0);
                ActiveItem.transform.position = Position;
            }
        }
    }

    //ObjectPool 

    PullableObject GetPlaneFromPool(PullableObject Plane, Vector3 Position)
    {
        for (int i = 0; i < PlaneObjectPool.Count; ++i)
        {
            PullableObject obj = PlaneObjectPool[i];
            if (obj.GetID() == Plane.GetID())
            {
                obj.transform.position = Position;
                obj.transform.rotation = transform.rotation;
                obj.transform.parent = TempStuff.transform;
                obj.gameObject.SetActive(true);
                PlaneObjectPool.RemoveAt(i);
                return obj;
            }
        }
        PullableObject instance = Instantiate(Plane, Position, transform.rotation, TempStuff.transform);
        instance.transform.localScale = new Vector3(Size / 10.5f, Size / 10.5f, Size / 10.5f);
        return instance;
    }

    void ReturnToPlanePool(PullableObject Plane)
    {
        Plane.gameObject.SetActive(false);
        PlaneObjectPool.Add(Plane);
    }
}
