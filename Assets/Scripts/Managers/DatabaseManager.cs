using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System;
using System.Data;

public class DatabaseManager : MonoBehaviour, IGameManager
{
    private NpgsqlConnection conn;

    public ManagerStatus status { get; private set;}
    // Use this for initialization
    void Start () {
		
	}

    public void Startup()
    {

        try
        {
            string connstring = System.IO.File.ReadAllText("dbConfig.txt");
            conn = new NpgsqlConnection(connstring);
            conn.Open();
        }
        catch (Exception msg)
        {
            // something went wrong, and you wanna know why
            Debug.Log(msg.ToString());
        }
        status = ManagerStatus.Started;
    } 

    public NpgsqlDataReader GetQuery(String sql)
    {
        NpgsqlCommand command = new NpgsqlCommand(sql, conn);
        NpgsqlDataReader dr = command.ExecuteReader();
        return dr;
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Shutdowm()
    {
        conn.Close();
    }
}
