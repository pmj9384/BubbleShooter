using UnityEngine;

public abstract class UIPopup : MonoBehaviour
{
    protected IOutGameUIManager uiManager;

    public void SetOutGameUIManager(IOutGameUIManager uiManager)
    {
        this.uiManager = uiManager;
    }

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
