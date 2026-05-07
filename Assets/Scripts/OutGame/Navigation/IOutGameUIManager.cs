public interface IOutGameUIManager
{
    void OpenScreen<T>() where T : UIScreen;
    void ShowPopup<T>() where T : UIPopup;
    void HidePopup<T>() where T : UIPopup;
}
