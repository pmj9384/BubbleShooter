using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField] private BasePanel[] panels;

    public void Show<T>() where T : BasePanel
    {
        foreach (var panel in panels)
        {
            if (panel is T)
                panel.Show();
        }
    }

    public void Hide<T>() where T : BasePanel
    {
        foreach (var panel in panels)
        {
            if (panel is T)
                panel.Hide();
        }
    }

    public void Initialize(OutGameManager outGameManager)
    {
        foreach (var panel in panels)
            panel.SetOutGameManager(outGameManager);
    }
}
