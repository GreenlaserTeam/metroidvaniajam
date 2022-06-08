﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using scr_Interfaces;
public class DataPersistenceManager : MonoBehaviour
{
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public enum SaveSlots
    {
        profile1,
        profile2,
        profile3
    }
 
    public SaveSlots currentSave;
    public static DataPersistenceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one Data Persistence Manager in the scene.");
        }
        Instance = this;

    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Equals)) SaveGame();
        //if (Input.GetKeyDown(KeyCode.Minus)) LoadGame();
    }

    private void Start()
    {
        this.dataHandler = new FileDataHandler();
        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
        LoadGame((int)currentSave);
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame(int currentSlot)
    {
        // load any saved data from a file using the data handler
        if (currentSlot >= 0 && currentSlot < 3)
        {
            this.currentSave = (SaveSlots)currentSlot;
        }
            this.gameData = dataHandler.Load();

        // if no data can be loaded, initialize to a new game
        if (this.gameData == null)
        {
            Debug.Log("No data was found. Initializing data to defaults.");
            NewGame();
        }

        // push the loaded data to all other scripts that need it
        foreach (IDataPersistence dataObject in dataPersistenceObjects)
        {
            dataObject.LoadData(gameData);
        }
    }

    public void SaveGame(int currentSlot)
    {
        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataObject in dataPersistenceObjects)
        {
            dataObject.SaveData(gameData);
        }

        // if we don't have any data to save, log a warning here
        if (this.gameData == null)
        {
            Debug.LogWarning("No data was found. A New Game needs to be started before data can be saved.");
            return;
        }

        if (currentSlot >= 0 && currentSlot < 3)
        {
            this.currentSave = (SaveSlots)currentSlot;
        }
        // save that data to a file using the data handler
        dataHandler.Save(gameData);
    }

    private void OnApplicationQuit()
    {
       // SaveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>()
            .OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}
