using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public enum QuizMode { Pre, Post }

    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text progressText;
    [SerializeField] TMP_Text questionText;
    [SerializeField] TMP_Text feedbackText;
    [SerializeField] Button[] optionButtons;      // exactly 4
    [SerializeField] Button nextButton;
    [SerializeField] TMP_Text nextButtonLabel;

    [SerializeField] Color correctColor  = new Color(0.55f, 0.85f, 0.55f);
    [SerializeField] Color wrongColor    = new Color(0.95f, 0.5f, 0.5f);
    [SerializeField] Color chosenColor   = new Color(1f, 0.9f, 0.5f);

    QuizMode mode;
    Action<int> onFinished;
    int index, score;
    bool answered;
    readonly int[] order = new int[4];     // order[slot] = original option index
    TMP_Text[] optionLabels;
    Color[] baseColors;

    void Awake()
    {
        optionLabels = new TMP_Text[optionButtons.Length];
        baseColors = new Color[optionButtons.Length];
        for (int i = 0; i < optionButtons.Length; i++)
        {
            int slot = i;
            optionLabels[i] = optionButtons[i].GetComponentInChildren<TMP_Text>(true);
            baseColors[i] = optionButtons[i].image.color;
            optionButtons[i].onClick.AddListener(() => Answer(slot));
        }
        nextButton.onClick.AddListener(Next);
    }

    public void Begin(QuizMode m, Action<int> finished)
    {
        mode = m;
        onFinished = finished;
        index = 0;
        score = 0;
        titleText.text = m == QuizMode.Pre ? "Pre-Learning Quiz" : "Post-Learning Quiz";
        ShowQuestion();
    }

    void ShowQuestion()
    {
        var q = QuizBank.All[index];
        answered = false;
        progressText.text = $"Question {index + 1} / {QuizBank.All.Length}";
        questionText.text = q.text;
        feedbackText.text = "";
        nextButton.gameObject.SetActive(false);

        // shuffle option order (Fisher-Yates)
        for (int i = 0; i < order.Length; i++) order[i] = i;
        for (int i = order.Length - 1; i > 0; i--)
        {
            int k = UnityEngine.Random.Range(0, i + 1);
            (order[i], order[k]) = (order[k], order[i]);
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionLabels[i].text = q.options[order[i]];
            optionButtons[i].image.color = baseColors[i];
        }
    }

    void Answer(int slot)
    {
        if (answered) return;
        answered = true;

        var q = QuizBank.All[index];
        bool correct = order[slot] == q.correct;
        if (correct) score++;

        if (mode == QuizMode.Post)
        {
            for (int i = 0; i < optionButtons.Length; i++)
                if (order[i] == q.correct) optionButtons[i].image.color = correctColor;
            if (!correct) optionButtons[slot].image.color = wrongColor;

            feedbackText.text = correct
                ? "Mew: Correct! Well done!"
                : "Mew: Not quite. " + q.hint;
        }
        else
        {
            optionButtons[slot].image.color = chosenColor;
            feedbackText.text = "Mew: Answer saved!";
        }

        nextButtonLabel.text = index == QuizBank.All.Length - 1 ? "FINISH" : "NEXT";
        nextButton.gameObject.SetActive(true);
    }

    void Next()
    {
        if (!answered) return;
        index++;
        if (index >= QuizBank.All.Length)
        {
            if (mode == QuizMode.Pre) ScoreStore.SavePre(score);
            else ScoreStore.SavePost(score);
            onFinished?.Invoke(score);
            return;
        }
        ShowQuestion();
    }
}