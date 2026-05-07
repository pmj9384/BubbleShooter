public interface IOutGameUIManager
{
    void OpenPage<T>() where T : UIPage;
    void ShowPopup<T>() where T : UIPopup;
    void HidePopup<T>() where T : UIPopup;
}
