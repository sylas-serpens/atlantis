using UnityEngine;
using TMPro;

public class PlayerGoldUI : MonoBehaviour
{
    [Header("Gold")]
    public int currentGold = 0;

    [Header("UI")]
    public TextMeshProUGUI goldText;

    [System.Obsolete]
    void Start()
    {
        if (goldText == null)
        {
            goldText = FindObjectOfType<TextMeshProUGUI>();
        }

        UpdateGoldUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        if (currentGold < 0)
            currentGold = 0;

        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = "" + currentGold;
        }
    }
}
