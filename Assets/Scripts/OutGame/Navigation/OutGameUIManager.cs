using UnityEngine;

public class OutGameUIManager : MonoBehaviour, IOutGameUIManager
{
    [SerializeField] private UIPage[] pages;
    [SerializeField] private UIPopup[] popups;

    private UIPage currentPage;

    public void Initialize()
    {
        foreach (var page in pages)
            page.SetOutGameUIManager(this);
        foreach (var popup in popups)
            popup.SetOutGameUIManager(this);

        OpenPage<LobbyPage>();
    }

    public void OpenPage<T>() where T : UIPage
    {
        foreach (var page in pages)
        {
            if (page is T)
            {
                currentPage = page;
                page.Open();
            }
            else
            {
                page.Close();
            }
        }
    }

    public void ShowPopup<T>() where T : UIPopup
    {
        foreach (var popup in popups)
        {
            if (popup is T)
                popup.Show();
        }
    }

    public void HidePopup<T>() where T : UIPopup
    {
        foreach (var popup in popups)
        {
            if (popup is T)
                popup.Hide();
        }
    }
}
