using UnityEngine;

public abstract class UIPanel : MonoBehaviour
{
    protected IUIManager uiManager;

    public void SetUIManager(IUIManager uiManager)
    {
        this.uiManager = uiManager;
    }
}
