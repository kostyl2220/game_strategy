using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System;
using System.Data;
using Mono.Data.Sqlite;

public class DatabaseManager : MonoBehaviour, IGameManager
{
    //private NpgsqlConnection conn;
    private SqliteConnection sqliteconn;
    private SqliteCommand com;
    private String sqConnectionString = "URI=file:data.db;";

    public ManagerStatus status { get; private set;}
    // Use this for initialization
    void Start () {
		
	}

    void OnFocus()
    {
        Debug.Log("Hello");
    }

    public void Startup()
    {
  
        try
        {
            //string connstring = System.IO.File.ReadAllText("dbConfig.txt");
            //conn = new NpgsqlConnection(connstring);
            //conn.Open();

            using (sqliteconn = new SqliteConnection(sqConnectionString)) { 
                sqliteconn.Open();


                com = new SqliteCommand(sqliteconn);
                com.CommandText = "INSERT INTO Sessions(Name, user_id) VALUES(\"session2\", 1);";
                com.ExecuteNonQuery();
                com.Dispose();
            }

            sqliteconn = new SqliteConnection(sqConnectionString);
            sqliteconn.Open();

        }
        catch (Exception msg)
        {
            // something went wrong, and you wanna know why
            Debug.Log(msg.ToString());
        }
        status = ManagerStatus.Started;
    } 

    public IDataReader GetQuery(String sql)
    {
        //NpgsqlCommand command = new NpgsqlCommand(sql, conn);
        //IDataReader dr = command.ExecuteReader();
        return null;
    }

    public IDataReader GetSQLiteQuery(String sql)
    {
        SqliteCommand com = sqliteconn.CreateCommand();
        com.CommandText = sql;
        SqliteDataReader dr = com.ExecuteReader();
        //command.Dispose();
        return dr;
    }

    public void PutSQLiteQuery(String sql)
    {
        SqliteCommand com = sqliteconn.CreateCommand();
        com.CommandText = sql;
        SqliteDataReader dr = com.ExecuteReader();
        while (dr.Read())
        {

        }
        dr.Close();
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void Shutdowm()
    {
        //conn.Close();
        sqliteconn.Close();
    }
}
