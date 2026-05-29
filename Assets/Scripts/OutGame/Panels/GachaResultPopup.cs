using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultPopup : UIPopup
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private Button closeButton;
    [SerializeField] private GachaResultItemUI gachaResultItemUI;
    private readonly List<GachaResultItemUI> items = new();

    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
    }

    public void ShowWithResults(List<(SkinDataTable.SkinRawData skin, bool isNew)> results)
    {
        foreach (var item in items)
            Destroy(item.gameObject);
        items.Clear();
        foreach (var data in results)
        {
            var item = Instantiate(gachaResultItemUI, contentParent);
            item.Setup(data.skin, data.isNew);
            items.Add(item);
        }
        Show();
    }
}
