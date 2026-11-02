using System.Collections;
using TMPro;
using UnityEngine;

public abstract class Lesson : MonoBehaviour
{
    [SerializeField] protected LessonHUD hud;
    [SerializeField] protected GameObject blockPrefab;
    [SerializeField] GameObject controlsPanel;   // this lesson's buttons
    [SerializeField] string rootName;            // e.g. "StackRoot"

    Transform area;                // the lesson's object inside the lab
    protected Transform holder;    // runtime container for spawned blocks
    protected bool busy;

    protected virtual void Awake()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Called once, right after the lab is placed
    public void Bind(Transform lab)
    {
        area = lab.Find(rootName);
        if (area == null)
        {
            Debug.LogError($"{name}: '{rootName}' not found inside the lab prefab.");
            return;
        }
        holder = new GameObject("Items").transform;
        holder.SetParent(area, false);
        area.gameObject.SetActive(false);
    }

    public void Enter()
    {
        if (area == null) return;
        area.gameObject.SetActive(true);
        controlsPanel.SetActive(true);
        ClearHolder();
        busy = false;
        OnEnter();
    }

    public void Exit()
    {
        StopAllCoroutines();
        busy = false;
        controlsPanel.SetActive(false);
        if (area != null) area.gameObject.SetActive(false);
    }

    protected abstract void OnEnter();

    protected void ClearHolder()
    {
        for (int i = holder.childCount - 1; i >= 0; i--)
            Destroy(holder.GetChild(i).gameObject);
    }

    protected void SetLabel(GameObject block, int index)
    {
        var label = block.GetComponentInChildren<TextMeshPro>();
        if (label != null) label.text = ((char)('A' + index % 26)).ToString();
    }

    protected IEnumerator ScaleIn(Transform t)
    {
        busy = true;
        Vector3 target = t.localScale;
        for (float x = 0; x < 1; x += Time.deltaTime * 4f)
        {
            t.localScale = target * Mathf.SmoothStep(0, 1, x);
            yield return null;
        }
        t.localScale = target;
        busy = false;
    }

    protected IEnumerator ScaleOut(GameObject go)
    {
        busy = true;
        Vector3 start = go.transform.localScale;
        for (float x = 0; x < 1; x += Time.deltaTime * 4f)
        {
            go.transform.localScale = start * (1 - x);
            yield return null;
        }
        Destroy(go);
        busy = false;
    }
}