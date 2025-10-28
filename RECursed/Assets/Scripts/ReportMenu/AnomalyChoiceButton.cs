using UnityEngine;

// Small, resusable button used by the anomaly report UI.
public class AnomalyChoiceButton : MonoBehaviour
{
    public enum Kind { Room, Type }
    public Kind kind;
    public string value;
    public AnomalyMenuController menu;

    public void Choose()
    {
        if (!menu) return; // Acts as a safety

	//Route the value into the correct selection bucket.
        if (kind == Kind.Room) menu.SelectRoom(value); 
        else                   menu.SelectType(value);
    }
}
