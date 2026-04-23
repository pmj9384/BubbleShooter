using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    protected OutGameManager outGameManager;

    public void SetOutGameManager(OutGameManager outGameManager)
    {
        this.outGameManager = outGameManager;
    }

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
