using System.Collections;
using TMPro;
using UnityEngine;

public class BubbleSortLesson : Lesson
{
    [SerializeField] float spacing = 0.1f;
    [SerializeField] float blockScale = 0.8f;
    [SerializeField] Color compareColor = new Color(1f, 0.84f, 0.31f);
    [SerializeField] Color swapColor    = new Color(1f, 0.55f, 0.2f);
    [SerializeField] Color sortedColor  = new Color(0.45f, 0.8f, 0.5f);

    const int N = 4;

    static readonly string[] code =
    {
        "for (int i = 0; i < n - 1; i++)",
        "  for (int j = 0; j < n - i - 1; j++)",
        "    if (arr[j] > arr[j + 1])",
        "      Swap(arr[j], arr[j + 1]);"
    };

    int[] vals = new int[N];
    readonly GameObject[] blocks = new GameObject[N];
    readonly bool[] sorted = new bool[N];
    int pass, j;
    bool done;
    Color baseColor;

    Vector3 Slot(int i) => new Vector3((i - (N - 1) / 2f) * spacing, 0.02f, 0);

    protected override void OnEnter()
    {
        Setup(new[] { 4, 2, 3, 1 });   // the example from the brief
        hud.Say("Bubble sort compares neighbours and swaps them if they are in the wrong order!");
        hud.Explain("Tap NEXT STEP to compare the first two blocks.");
        hud.ShowCode(code, -1);
    }

    void Setup(int[] values)
    {
        vals = (int[])values.Clone();
        pass = 0; j = 0; done = false;
        for (int i = 0; i < N; i++) sorted[i] = false;

        for (int i = 0; i < N; i++)
        {
            var b = Instantiate(blockPrefab, holder);
            b.transform.localPosition = Slot(i);
            b.transform.localRotation = Quaternion.identity;
            b.transform.localScale = blockPrefab.transform.localScale * blockScale;
            var label = b.GetComponentInChildren<TextMeshPro>();
            if (label != null) label.text = vals[i].ToString();
            blocks[i] = b;
        }
        baseColor = blocks[0].GetComponent<Renderer>().sharedMaterial.color;
        RefreshColors();
    }

    // ---------- Buttons ----------

    public void NextStep()
    {
        if (busy || holder == null) return;
        if (done)
        {
            hud.Say("All sorted! Tap RESET to try new numbers.");
            return;
        }
        StartCoroutine(StepRoutine());
    }

    public void ResetNumbers()
    {
        if (busy || holder == null) return;
        ClearHolder();

        // random distinct values 1-9
        var pool = new System.Collections.Generic.List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var v = new int[N];
        for (int i = 0; i < N; i++)
        {
            int k = Random.Range(0, pool.Count);
            v[i] = pool[k];
            pool.RemoveAt(k);
        }
        Setup(v);
        hud.ShowCode(code, -1);
        hud.Explain("New numbers! Tap NEXT STEP to start sorting.");
        hud.Say("Let's sort these too!");
    }

    // ---------- Routines ----------

    IEnumerator StepRoutine()
    {
        busy = true;
        int a = j, b = j + 1;

        SetColor(a, compareColor);
        SetColor(b, compareColor);
        hud.ShowCode(code, 2);
        hud.Explain($"Compare {vals[a]} and {vals[b]}: is {vals[a]} > {vals[b]}?");
        yield return new WaitForSeconds(0.6f);

        if (vals[a] > vals[b])
        {
            hud.ShowCode(code, 3);
            hud.Explain($"Yes, {vals[a]} > {vals[b]}, so the two blocks SWAP.");
            hud.Say("The bigger number bubbles to the right!");
            SetColor(a, swapColor);
            SetColor(b, swapColor);
            yield return SwapRoutine(a, b);
        }
        else
        {
            hud.Explain($"No, {vals[a]} <= {vals[b]}. They are already in order, so no swap.");
            hud.Say("These two are fine. On to the next pair!");
            yield return new WaitForSeconds(0.4f);
        }

        // advance indices
        j++;
        if (j >= N - pass - 1)
        {
            sorted[N - pass - 1] = true;     // biggest of this pass is in place
            pass++;
            j = 0;
            if (pass >= N - 1)
            {
                for (int i = 0; i < N; i++) sorted[i] = true;
                done = true;
                hud.ShowCode(code, -1);
                hud.Explain("Sorted! Bubble sort repeated compare-and-swap until no more swaps were needed.");
                hud.Say("Great job! The list is in order.");
            }
            else
            {
                hud.ShowCode(code, 0);
                hud.Say($"Pass {pass} done! The biggest value is locked in on the right.");
            }
        }
        RefreshColors();
        busy = false;
    }

    IEnumerator SwapRoutine(int a, int b)
    {
        Transform ta = blocks[a].transform, tb = blocks[b].transform;
        Vector3 pa = Slot(a), pb = Slot(b);

        for (float x = 0; x < 1; x += Time.deltaTime * 2.5f)
        {
            float s = Mathf.SmoothStep(0, 1, x);
            float arc = Mathf.Sin(s * Mathf.PI);
            ta.localPosition = Vector3.Lerp(pa, pb, s) + new Vector3(0, 0.03f * arc, 0.04f * arc);
            tb.localPosition = Vector3.Lerp(pb, pa, s) + new Vector3(0, 0, -0.04f * arc);
            yield return null;
        }
        ta.localPosition = pb;
        tb.localPosition = pa;

        (blocks[a], blocks[b]) = (blocks[b], blocks[a]);
        (vals[a], vals[b]) = (vals[b], vals[a]);
    }

    // ---------- Helpers ----------

    void SetColor(int i, Color c) => blocks[i].GetComponent<Renderer>().material.color = c;

    void RefreshColors()
    {
        for (int i = 0; i < N; i++)
            SetColor(i, sorted[i] ? sortedColor : baseColor);
    }
}