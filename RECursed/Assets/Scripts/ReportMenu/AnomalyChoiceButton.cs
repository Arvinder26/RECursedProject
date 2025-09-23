using UnityEngine;

public class AnomalyChoiceButton : MonoBehaviour
{
    public enum Kind { Room, Type }
    public Kind kind;
    public string value;
    public AnomalyMenuController menu;

    public void Choose()
    {
        if (!menu) return;
        if (kind == Kind.Room) menu.SelectRoom(value);
        else                   menu.SelectType(value);
    }
}
