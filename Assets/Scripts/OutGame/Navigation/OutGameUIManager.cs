using UnityEngine;
using UnityEngine.Serialization;

public class OutGameUIManager : MonoBehaviour, IOutGameUIManager
{
    [FormerlySerializedAs("pages")]
    [SerializeField] private UIScreen[] screens;
    [SerializeField] private UIPopup[] popups;

    private UIScreen currentScreen;

    public void Initialize()
    {
        foreach (var screen in screens)
            screen.SetOutGameUIManager(this);
        foreach (var popup in popups)
            popup.SetOutGameUIManager(this);
    }

    public void OpenScreen<T>() where T : UIScreen
    {
        foreach (var screen in screens)
        {
            if (screen is T)
            {
                currentScreen = screen;
                screen.Open();
            }
            else
            {
                screen.Close();
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
