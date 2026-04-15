public class GameOverUIElement : UIElement
{
    public override void Initialize() => gameObject.SetActive(false);

    public override void Show() => gameObject.SetActive(true);

    public override void Hide() => gameObject.SetActive(false);
}
