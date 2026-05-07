using UnityEngine;

public class OutGameManager : MonoBehaviour
{
    [SerializeField] private OutGameUIManager uiManager;

    private void Start()
    {
        uiManager.Initialize();
    }
}
