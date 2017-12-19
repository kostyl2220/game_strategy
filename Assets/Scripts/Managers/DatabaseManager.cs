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
            string connstring = String.Format("Server={0};Port={1};" +
                "User Id={2};Password={3};Database={4};",
                "127.0.0.1", "5433", "postgres", "Zkos567895kosZ", "Kursach");
            // Making connection with Npgsql provider
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
