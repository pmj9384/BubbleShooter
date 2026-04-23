using UnityEngine;

public abstract class BaseScreen : MonoBehaviour
{
    protected OutGameManager outGameManager;

    public void SetOutGameManager(OutGameManager outGameManager)
    {
        this.outGameManager = outGameManager;
    }

    public virtual void OnEnter() => gameObject.SetActive(true);
    public virtual void OnExit() => gameObject.SetActive(false);
}
