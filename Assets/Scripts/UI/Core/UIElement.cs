using UnityEngine;

public class UIElement : MonoBehaviour
{
    protected GameUIManager gameUIManager;

    public void SetUIManager(GameManager gameManager, GameUIManager uIManager)
    {
        gameUIManager = uIManager;
    }

    public virtual void Initialize() { }

    public virtual void Show() { }

    public virtual void Hide() { }
}