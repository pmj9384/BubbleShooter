using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIManager : InGameManager
{
    public List<UIElement> uiElements;

    public override void Initialize()
    {
        base.Initialize();
        foreach (var element in uiElements)
        {
            element.SetUIManager(GameManager, this);
        }

        // TODO: 게임 상태에 따른 UI 표시/숨김 등록
        // GameManager.AddGameStateEnterAction(GameManager.GameState.GameReady, () =>
        // {
        //     ShowUIElement(UIElementEnums.TitlePanel);
        // });

        // GameManager.AddGameStateEnterAction(GameManager.GameState.GamePlay, () =>
        // {
        //     HideUIElement(UIElementEnums.TitlePanel);
        // });

        // GameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, () =>
        // {
        //     StartCoroutine(ShowDelayed(UIElementEnums.GameOverPanel, 1.5f));
        // });

        GameManager.AddGameStateEnterAction(GameManager.GameState.GameReady, () =>
        {
            ShowUIElement(UIElementEnums.HUD);
        });

        GameManager.AddGameStateEnterAction(GameManager.GameState.GamePlay, () =>
        {
            ShowUIElement(UIElementEnums.HUD);
        });

        GameManager.AddGameStateEnterAction(GameManager.GameState.GameStop, () =>
        {
            ShowUIElement(UIElementEnums.PausePanel);
        });

        GameManager.AddGameStateExitAction(GameManager.GameState.GameStop, () =>
        {
            HideUIElement(UIElementEnums.PausePanel);
            HideUIElement(UIElementEnums.SettingsPanel);
        });

        GameManager.AddGameStateEnterAction(GameManager.GameState.GameOver, () =>
        {
            HideUIElement(UIElementEnums.HUD);
            ShowUIElement(UIElementEnums.GameOverPanel);
        });
    }

    public void InitializedUIElements()
    {
        foreach (var element in uiElements)
        {
            element.Initialize();
        }
    }

    public void ShowUIElement(UIElementEnums type)
    {
        uiElements[(int)type].Show();
    }

    public void HideUIElement(UIElementEnums type)
    {
        uiElements[(int)type].Hide();
    }

    private IEnumerator ShowDelayed(UIElementEnums type, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowUIElement(type);
    }
}