using UnityEngine;

public abstract class UIWidget : MonoBehaviour
{
    protected virtual void OnEnable() => Subscribe();
    protected virtual void OnDisable() => Unsubscribe();
    protected abstract void Subscribe();
    protected abstract void Unsubscribe();
    public abstract void Refresh();
}
