using TMPro;
using UnityEngine;

public class LessonHUD : MonoBehaviour
{
    [SerializeField] TMP_Text mewText;
    [SerializeField] TMP_Text explanationText;
    [SerializeField] TMP_Text codeText;

    public static event System.Action MewSpoke;
    public static void RaiseMewSpoke() => MewSpoke?.Invoke();

    public void Say(string msg)
    {
        mewText.text = "Mew: " + msg;
        MewSpoke?.Invoke();
    }
    public void Explain(string msg) => explanationText.text = msg;

    public void ShowCode(string[] lines, int highlight)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < lines.Length; i++)
            sb.AppendLine(i == highlight
                ? $"<mark=#FFD54F88><color=#000000>{lines[i]}</color></mark>"
                : lines[i]);
        codeText.text = sb.ToString();
    }
}