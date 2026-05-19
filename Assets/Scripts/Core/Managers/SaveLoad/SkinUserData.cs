using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkinUserData : ISaveLoad
{
    public DataSourceType SaveDataSouceType => DataSourceType.Local;

    private readonly List<string> ownedSkinIds = new();

    private string equippedSkinId;
    public string EquippedSkinId => equippedSkinId;

    public Sprite GetEquippedSprite() =>
        Resources.Load<Sprite>($"Sprites/Skins/{equippedSkinId}");

    public event Action<string> OnSkinEquipped;

    public SkinUserData()
    {
        SaveLoadSystem.Instance.RegisterOnSaveAction(this);
    }

    public bool IsOwned(string skinId) => ownedSkinIds.Contains(skinId);

    public void Equip(string skinId)
    {
        if (!IsOwned(skinId)) return;
        equippedSkinId = skinId;
        OnSkinEquipped?.Invoke(equippedSkinId);
    }

    public void Save()
    {
        var saveData = SaveLoadSystem.Instance.CurrentSaveData.skinUserDataSave = new();
        saveData.ownedSkinIds = new(ownedSkinIds);
        saveData.equippedSkinId = equippedSkinId;
    }

    public void Load()
    {
        ownedSkinIds.Clear();
        foreach (var skin in DataTableManager.SkinDataTable.All)
            ownedSkinIds.Add(skin.SkinId);

        equippedSkinId = DataTableManager.SkinDataTable.All
            .FirstOrDefault(s => s.Grade == nameof(SkinGrade.Common))?.SkinId;
    }

    public void Load(SkinUserDataSave saveData)
    {
        if (saveData == null) { Load(); return; }

        ownedSkinIds.Clear();
        ownedSkinIds.AddRange(saveData.ownedSkinIds);
        equippedSkinId = saveData.equippedSkinId;
    }
}
