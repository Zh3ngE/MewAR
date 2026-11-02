using TMPro;
using UnityEngine;

public class LessonManager : MonoBehaviour
{
    [SerializeField] GameObject hubPanel;
    [SerializeField] GameObject hudPanel;
    [SerializeField] GameObject backButton;
    [SerializeField] TMP_Text hubMessage;
    [SerializeField] Lesson[] lessons;   // 0 Stack, 1 Queue, 2 Linked List, 3 Tree, 4 Sorting

    Lesson current;
    public bool LabPlaced { get; private set; }

    void Awake()
    {
        hubPanel.SetActive(false);
        hudPanel.SetActive(false);
        backButton.SetActive(false);
    }

    // Called by ARPlacementManager once the lab is placed
    public void Begin(GameObject lab)
    {
        foreach (var l in lessons)
            if (l != null) l.Bind(lab.transform);
        LabPlaced = true;
        ShowHub();
    }

    public void OpenLesson(int index)
    {
        if (index < 0 || index >= lessons.Length || lessons[index] == null)
        {
            hubMessage.text = "Mew: That lesson is coming soon!";
            return;
        }
        hubPanel.SetActive(false);
        hudPanel.SetActive(true);
        backButton.SetActive(true);
        current = lessons[index];
        current.Enter();
        ScoreStore.MarkLesson(index);
    }

    public void BackToHub()
    {
        if (current != null) current.Exit();
        current = null;
        ShowHub();
    }

    // Hide every lesson UI (used when a quiz or menu takes over the screen)
    public void HideAll()
    {
        if (current != null) current.Exit();
        current = null;
        hubPanel.SetActive(false);
        hudPanel.SetActive(false);
        backButton.SetActive(false);
    }

    public void ShowHub()
    {
        hudPanel.SetActive(false);
        backButton.SetActive(false);
        hubPanel.SetActive(true);
        hubMessage.text = "Mew: What shall we learn today?";
    }
}
