using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSave : MonoBehaviour
{
    private static DataSave _instance;

    public static DataSave Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject singletonObject = new GameObject("SingletonExample");
                _instance = singletonObject.AddComponent<DataSave>();
            }

            return _instance;
        }
    }
    public float effectSound, mainSound;
    public int characterIndex;
    public int difficulty;
    public int winCount;
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    public void SoundUpdate(float effectSound, float mainSound)
    {
        if (effectSound >= 0)
        {
            this.effectSound = effectSound;
        }
        if (mainSound >= 0)
        {
            this.mainSound = mainSound;
        }
    }
    public void CharacterSelect(int index)
    {
        characterIndex = index;
    }
    public void WinUpdate(int winCount)
    {
        this.winCount = winCount;
    }
    public void DifficultySelect(int difficulty)
    {
        this.difficulty = difficulty;
    }
}
