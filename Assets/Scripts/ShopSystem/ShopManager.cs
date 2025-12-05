using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int cost;
        public Button button;        // The UI Button for this weapon
        public TMP_Text nameText;    // Text showing weapon name
        public TMP_Text costText;    // Text showing price
        public TMP_Text statusText;  // Shows "EQUIPPED"
    }

    // Assign these in the Inspector
    public ShopItem[] shopItems;

    public TMP_Text goldText;
    public TMP_Text messageText;

    // Starting gold (can replace with PlayerStats later)
    public int gold = 200;

    // Save equipped weapon name
    private string equippedItem = "";

    void Start()
    {
        goldText.text = "Gold: " + gold;

        // Setup each item in the Inspector
        foreach (ShopItem item in shopItems)
        {
            item.nameText.text = item.itemName;
            item.costText.text = item.cost + "G";
            item.statusText.text = "";

            // Ensure each button triggers BuyItem(item)
            item.button.onClick.AddListener(() => BuyItem(item));
        }
    }

    //buy + equip items
    void BuyItem(ShopItem item)
    {
        // Prevent re-buying same weapon
    if (equippedItem == item.itemName)
    {
        messageText.text = $"{item.itemName} is already equipped!";
        return;
    }

    // Check gold
    if (gold < item.cost)
    {
        messageText.text = $"Not enough gold for {item.itemName}!";
        return;
    }

    // Deduct cost
    gold -= item.cost;
    goldText.text = "Gold: " + gold;

    // Equip in UI
    equippedItem = item.itemName;
    messageText.text = $"Equipped: {item.itemName}!";

    // Reset all EQUIPPED texts
    foreach (ShopItem s in shopItems)
        s.statusText.text = "";

    item.statusText.text = "SELECTED";

    //update th user;s equipped weapon -> later hook w PlayerController to equip on scene
    if (item.itemName == "Water Gun")
        PlayerWeapon.equippedWeapon = PlayerWeapon.WeaponType.WaterGun;
    else if (item.itemName == "Harpoon")
        PlayerWeapon.equippedWeapon = PlayerWeapon.WeaponType.Harpoon;
    else if (item.itemName == "Torpedo")
        PlayerWeapon.equippedWeapon = PlayerWeapon.WeaponType.Torpedo;
    }
}
