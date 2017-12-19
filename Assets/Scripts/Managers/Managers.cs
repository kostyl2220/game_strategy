using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ManagerStatus
{
    Shutdown,
    Initializing,
    Started
}

public interface IGameManager
{
    ManagerStatus status { get; }
    void Startup();
    void Shutdowm();
}

[RequireComponent(typeof(DatabaseManager))]
[RequireComponent(typeof(ItemManager))]
[RequireComponent(typeof(StorageManager))]
[RequireComponent(typeof(StoreManager))]
[RequireComponent(typeof(SessionManager))]
[RequireComponent(typeof(FactoryManager))]
[RequireComponent(typeof(TowerManager))]
[RequireComponent(typeof(UnitManager))]
[RequireComponent(typeof(BarackManager))]
public class Managers : MonoBehaviour
{
    public static DatabaseManager Database { get; private set; }
    public static ItemManager Items { get; private set; }
    public static StoreManager Store { get; private set; }
    public static StorageManager Storage { get; private set; }
    public static SessionManager Session { get; private set; }
    public static FactoryManager Factory { get; private set; }
    public static TowerManager Tower { get; private set; }
    public static UnitManager Units { get; private set; }
    public static BarackManager Barack { get; private set; }
    private List<IGameManager> _startSequence;
    void Awake()
    {
        Database = GetComponent<DatabaseManager>();
        Session = GetComponent<SessionManager>();
        Items = GetComponent<ItemManager>();
        Storage = GetComponent<StorageManager>();
        Store = GetComponent<StoreManager>();
        Factory = GetComponent<FactoryManager>();
        Tower = GetComponent<TowerManager>();
        Units = GetComponent<UnitManager>();
        Barack = GetComponent<BarackManager>();

        _startSequence = new List<IGameManager>();
        _startSequence.Add(Database);
        _startSequence.Add(Items);
        _startSequence.Add(Session);
        _startSequence.Add(Storage);
        _startSequence.Add(Store);
        _startSequence.Add(Factory);
        _startSequence.Add(Tower);
        _startSequence.Add(Units);
        _startSequence.Add(Barack);

        StartCoroutine(StartupManagers());
    }

    private IEnumerator StartupManagers()
    {
        foreach (IGameManager manager in _startSequence)
        {
            manager.Startup();
        }
        yield return null;
        int numModules = _startSequence.Count;
        int numReady = 0;
        while (numReady < numModules)
        {
            int lastReady = numReady;
            numReady = 0;
            foreach (IGameManager manager in _startSequence)
            {
                if (manager.status == ManagerStatus.Started)
                {
                    numReady++;
                }
            }
            if (numReady > lastReady)
                Debug.Log("Progress: " + numReady + "/" + numModules);
            yield return null;
        }
        Debug.Log("All managers started up");
    }

    void OnApplicationQuit()
    {
        _startSequence.Reverse();
        foreach (IGameManager manager in _startSequence)
        {
            manager.Shutdowm();
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
