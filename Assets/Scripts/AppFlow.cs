using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class AppFlow : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject quizPanel;
    [SerializeField] GameObject resultsPanel;
    [SerializeField] GameObject progressPanel;
    [SerializeField] GameObject howToPanel;
    [SerializeField] GameObject scanHint;

    [Header("Texts")]
    [SerializeField] TMP_Text resultsText;
    [SerializeField] TMP_Text progressText;

    [Header("References")]
    [SerializeField] QuizManager quiz;
    [SerializeField] LessonManager lessonManager;
    [SerializeField] ARPlacementManager placement;
    [SerializeField] ARPlaneManager planeManager;

    void Start() => ShowMenu();

    void HideAllPanels()
    {
        menuPanel.SetActive(false);
        quizPanel.SetActive(false);
        resultsPanel.SetActive(false);
        progressPanel.SetActive(false);
        howToPanel.SetActive(false);
        scanHint.SetActive(false);
    }

    // ---------- Menu buttons ----------

    public void ShowMenu()
    {
        HideAllPanels();
        lessonManager.HideAll();
        placement.enabled = false;
        planeManager.enabled = false;
        menuPanel.SetActive(true);
    }

    public void StartLearning()
    {
        HideAllPanels();
        if (lessonManager.LabPlaced)
        {
            lessonManager.ShowHub();
            return;
        }
        placement.enabled = true;
        planeManager.enabled = true;
        scanHint.SetActive(true);
    }

    public void StartPreQuiz()
    {
        HideAllPanels();
        quizPanel.SetActive(true);
        quiz.Begin(QuizManager.QuizMode.Pre, _ => StartLearning());
    }

    public void StartPostQuiz()
    {
        HideAllPanels();
        lessonManager.HideAll();
        quizPanel.SetActive(true);
        quiz.Begin(QuizManager.QuizMode.Post, _ => ShowResults());
    }

    public void ShowProgress()
    {
        HideAllPanels();
        progressText.text = BuildSummary();
        progressPanel.SetActive(true);
    }

    public void ResetProgress()
    {
        ScoreStore.ResetAll();
        progressText.text = BuildSummary();
    }

    public void ShowHowTo()
    {
        HideAllPanels();
        howToPanel.SetActive(true);
    }

    void ShowResults()
    {
        HideAllPanels();

        string msg = "Mew: Thanks for learning with me!";
        if (ScoreStore.HasPre && ScoreStore.HasPost)
        {
            int d = ScoreStore.Post - ScoreStore.Pre;
            if (d > 0)       msg = $"Mew: Amazing! You improved by {d} point{(d == 1 ? "" : "s")}!";
            else if (d == 0) msg = "Mew: Same score as before. Try the lessons once more!";
            else             msg = "Mew: Let's review the lessons and try again!";
        }
        else if (!ScoreStore.HasPre)
        {
            msg = "Mew: Take the pre-quiz next time so we can compare!";
        }

        resultsText.text = BuildSummary() + "\n\n" + msg;
        resultsPanel.SetActive(true);
    }

    string BuildSummary()
    {
        int total = QuizBank.All.Length;
        string pre  = ScoreStore.HasPre  ? $"{ScoreStore.Pre} / {total}"  : "not taken yet";
        string post = ScoreStore.HasPost ? $"{ScoreStore.Post} / {total}" : "not taken yet";

        string improve = "";
        if (ScoreStore.HasPre && ScoreStore.HasPost)
        {
            int d = ScoreStore.Post - ScoreStore.Pre;
            string sign = d > 0 ? "+" : "";
            improve = $"\n<b>Improvement:</b> {sign}{d}";
        }

        return $"<b>Pre-Quiz:</b> {pre}\n<b>Post-Quiz:</b> {post}{improve}\n<b>Lessons explored:</b> {ScoreStore.LessonsDone} / 5";
    }
}
//refresh