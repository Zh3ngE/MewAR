using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueLesson : Lesson
{
    [SerializeField] int capacity = 5;
    [SerializeField] float spacing = 0.09f;
    [SerializeField] float blockScale = 0.8f;

    readonly List<GameObject> queue = new();   // index 0 = FRONT
    int nextLetter;

    static readonly string[] enqueueCode = { "queue.Enqueue(item);", "// item joins the REAR" };
    static readonly string[] dequeueCode = { "if (queue.Count == 0) return;", "item = queue.Dequeue();", "// FRONT item removed" };

    Vector3 SlotPos(int i) => new Vector3(-0.18f + i * spacing, 0.02f, 0);

    protected override void OnEnter()
    {
        queue.Clear();
        nextLetter = 0;
        for (int i = 0; i < 3; i++) SpawnBlock(false);
        hud.Say("Hi! A queue is like a line of people waiting for a bus. Try ENQUEUE!");
        hud.Explain("Queue = First In, First Out (FIFO).");
        hud.ShowCode(enqueueCode, -1);
    }

    public void Enqueue()
    {
        if (busy || holder == null) return;
        if (queue.Count >= capacity)
        {
            hud.ShowCode(enqueueCode, 0);
            hud.Explain("The queue is full. Adding more would be an overflow!");
            hud.Say("Hint: dequeue someone first!");
            return;
        }
        SpawnBlock(true);
        hud.ShowCode(enqueueCode, 0);
        hud.Explain("ENQUEUE adds the new item at the REAR.");
        hud.Say("The newest block joins the back of the line!");
    }

    public void Dequeue()
    {
        if (busy || holder == null) return;
        if (queue.Count == 0)
        {
            hud.ShowCode(dequeueCode, 0);
            hud.Explain("The queue is empty. Nothing to dequeue (underflow)!");
            hud.Say("Hint: enqueue something first!");
            return;
        }
        var front = queue[0];
        queue.RemoveAt(0);
        StartCoroutine(DequeueRoutine(front));
        hud.ShowCode(dequeueCode, 1);
        hud.Explain("DEQUEUE removes the OLDEST item, from the FRONT (FIFO).");
        hud.Say("The first one in line was the first one out!");
    }

    void SpawnBlock(bool animate)
    {
        var b = Instantiate(blockPrefab, holder);
        b.transform.localPosition = SlotPos(queue.Count);
        b.transform.localRotation = Quaternion.identity;
        b.transform.localScale = blockPrefab.transform.localScale * blockScale;
        SetLabel(b, nextLetter);
        nextLetter++;
        queue.Add(b);
        if (animate) StartCoroutine(ScaleIn(b.transform));
    }

    IEnumerator DequeueRoutine(GameObject front)
    {
        yield return ScaleOut(front);

        busy = true;
        var starts = new Vector3[queue.Count];
        for (int i = 0; i < queue.Count; i++) starts[i] = queue[i].transform.localPosition;

        for (float x = 0; x < 1; x += Time.deltaTime * 4f)
        {
            float s = Mathf.SmoothStep(0, 1, x);
            for (int i = 0; i < queue.Count; i++)
                queue[i].transform.localPosition = Vector3.Lerp(starts[i], SlotPos(i), s);
            yield return null;
        }
        for (int i = 0; i < queue.Count; i++) queue[i].transform.localPosition = SlotPos(i);
        busy = false;
    }
} 