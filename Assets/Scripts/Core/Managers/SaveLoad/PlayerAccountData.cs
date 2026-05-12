using System;
using UnityEngine;

public class PlayerAccountData : ISaveLoad
{
    public DataSourceType SaveDataSouceType => DataSourceType.Local;

    public int BestScore { get; private set; }

    public bool TryUpdateBestScore(int score)
    {
        if (score <= BestScore) return false;
        BestScore = score;
        return true;
    }

    private float bgmVolume;
    public float BgmVolume
    {
        get => bgmVolume;
        set { bgmVolume = Mathf.Clamp(value, 0.0001f, 1f); }
    }

    private float sfxVolume;
    public float SfxVolume
    {
        get => sfxVolume;
        set { sfxVolume = Mathf.Clamp(value, 0.0001f, 1f); }
    }

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnEnergyChanged;

    private int coins;
    public int Coins
    {
        get => coins;
        private set { coins = value; OnCoinsChanged?.Invoke(coins); }
    }

    public int MaxEnergy { get; } = 5;

    private int energy;
    public int Energy
    {
        get => energy;
        private set { energy = Mathf.Clamp(value, 0, MaxEnergy); OnEnergyChanged?.Invoke(energy); }
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
    }

    public bool TryConsumeEnergy()
    {
        if (Energy <= 0) return false;
        Energy--;
        return true;
    }

    public PlayerAccountData()
    {
        SaveLoadSystem.Instance.RegisterOnSaveAction(this);
    }

    public void Save()
    {
        var saveData = SaveLoadSystem.Instance.CurrentSaveData.playerAccountDataSave = new();
        saveData.bgmVolume = SoundManager.Instance.bgmVolume;
        saveData.sfxVolume = SoundManager.Instance.sfxVolume;
        saveData.bestScore = BestScore;
        saveData.coins = Coins;
        saveData.energy = Energy;
    }

    public void Load()
    {
        BgmVolume = 1f;
        SfxVolume = 1f;
        BestScore = 0;
        Coins = 0;
        Energy = MaxEnergy;
    }

    public void Load(PlayerAccountDataSave saveData)
    {
        if (saveData == null) { Load(); return; }
        BgmVolume = saveData.bgmVolume;
        SfxVolume = saveData.sfxVolume;
        BestScore = saveData.bestScore;
        Coins = saveData.coins;
        Energy = saveData.energy;
    }
}
