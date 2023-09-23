using UnityEngine;

public class BubbleMenuOption : MonoBehaviour
{
    private BubbleMenuController _controller;
    private TowerUpgrade _upgrade;

    public void InitializeUpgrade(BubbleMenuController controller, TowerUpgrade upgrade)
    {
        gameObject.SetActive(true);

        _controller = controller;
        _upgrade = upgrade;
    }
    
    public void OnHoverStart()
    {
        Debug.Log("On Hover Start");
    }

    public void OnHoverEnd()
    {
        Debug.Log("On Hover Exit");
    }

    public void OnConfirm()
    {
        Debug.Log("Confirming bubble menu option");
    }

    public void Upgrade()
    {
        _controller.Upgrade(_upgrade);
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}