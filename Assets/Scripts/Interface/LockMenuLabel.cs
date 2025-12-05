using UnityEngine;
using TMPro;

public class LockMenuLabel : MonoBehaviour
{
    public string fixedText = "Load Game";

    TMP_Text label;

    void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    void Start()
    {
        if (label != null)
            label.text = fixedText;
    }

    void Update()
    {
        if (label == null) return;

        if (label.text == "0" || string.IsNullOrEmpty(label.text))
            label.text = fixedText;
    }
}
