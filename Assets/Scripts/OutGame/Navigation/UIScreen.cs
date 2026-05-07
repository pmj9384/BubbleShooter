using UnityEngine;

public abstract class UIScreen : MonoBehaviour
{
    protected IOutGameUIManager uiManager;

    public void SetOutGameUIManager(IOutGameUIManager uiManager)
    {
        this.uiManager = uiManager;
    }

    public virtual void Open() => gameObject.SetActive(true);
    public virtual void Close() => gameObject.SetActive(false);
}
