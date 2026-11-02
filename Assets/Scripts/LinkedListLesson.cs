using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedListLesson : Lesson
{
    [SerializeField] Material arrowMaterial;
    [SerializeField] int capacity = 4;
    [SerializeField] float spacing = 0.12f;
    [SerializeField] float nodeScale = 0.7f;
    [SerializeField] float floatHeight = 0.07f;

    enum Pending { None, Created }
    Pending pending;
    GameObject pendingNode;

    readonly List<GameObject> nodes = new();    // index 0 = HEAD
    readonly List<GameObject> arrows = new();   // arrows[i] links nodes[i] -> nodes[i+1]
    int nextLetter;

    static readonly string[] addCode =
    {
        "Node n = new Node(value);",
        "tail.next = n;   // link it",
        "tail = n;        // update tail"
    };
    static readonly string[] removeCode =
    {
        "if (head == null) return;",
        "head = head.next;",
        "// old head is discarded"
    };

    Vector3 SlotPos(int i) => new Vector3(-0.18f + i * spacing, 0.02f, 0);
    Vector3 ArrowPos(int i) => (SlotPos(i) + SlotPos(i + 1)) / 2f;

    protected override void OnEnter()
    {
        nodes.Clear();
        arrows.Clear();
        pending = Pending.None;
        pendingNode = null;
        nextLetter = 0;

        for (int i = 0; i < 3; i++)
        {
            nodes.Add(SpawnNode(SlotPos(i), false));
            if (i > 0) arrows.Add(MakeArrow(i - 1));
        }

        hud.Say("A linked list is a chain of nodes. Each node points to the next one!");
        hud.Explain("Each node stores a value and a reference to the NEXT node.");
        hud.ShowCode(addCode, -1);
    }

    // ---------- Buttons ----------

    public void AddNode()
    {
        if (busy || holder == null) return;
        if (pending != Pending.None)
        {
            hud.Say("Hint: tap NEXT STEP to finish adding the node!");
            return;
        }
        if (nodes.Count >= capacity)
        {
            hud.Explain("The demo list is full. Remove the head to make room!");
            hud.Say("Hint: try REMOVE HEAD.");
            return;
        }

        pendingNode = SpawnNode(SlotPos(nodes.Count) + Vector3.up * floatHeight, true);
        pending = Pending.Created;
        hud.ShowCode(addCode, 0);
        hud.Explain("Step 1: a NEW node is created, but it is not connected to the list yet.");
        hud.Say("See the floating node? Tap NEXT STEP to link it in.");
    }

    public void NextStep()
    {
        if (busy || holder == null) return;
        if (pending == Pending.None)
        {
            hud.Say("Tap ADD NODE or REMOVE HEAD first!");
            return;
        }
        StartCoroutine(LinkRoutine());
    }

    public void RemoveHead()
    {
        if (busy || holder == null) return;
        if (pending != Pending.None)
        {
            hud.Say("Hint: finish adding the node first with NEXT STEP!");
            return;
        }
        if (nodes.Count == 0)
        {
            hud.ShowCode(removeCode, 0);
            hud.Explain("The list is empty, so there is no head to remove.");
            hud.Say("Hint: add a node first!");
            return;
        }

        var head = nodes[0];
        nodes.RemoveAt(0);
        GameObject arrow = null;
        if (arrows.Count > 0) { arrow = arrows[0]; arrows.RemoveAt(0); }

        StartCoroutine(RemoveRoutine(head, arrow));
        hud.ShowCode(removeCode, 1);
        hud.Explain("The HEAD pointer moves to head.next. The old head is no longer in the list.");
        hud.Say("The second node is the new head now!");
    }

    // ---------- Routines ----------

    IEnumerator LinkRoutine()
    {
        int idx = nodes.Count;
        pending = Pending.None;

        // Step 2: link
        hud.ShowCode(addCode, 1);
        if (idx > 0)
        {
            hud.Explain("Step 2: the old tail's NEXT reference now points to the new node.");
            var arrow = MakeArrow(idx - 1);
            arrows.Add(arrow);
            yield return ScaleIn(arrow.transform);
        }
        else
        {
            hud.Explain("Step 2: the list was empty, so the new node simply becomes the head.");
        }
        yield return MoveNode(pendingNode.transform, SlotPos(idx));
        nodes.Add(pendingNode);
        pendingNode = null;

        // Step 3: update tail
        hud.ShowCode(addCode, 2);
        hud.Explain("Step 3: the TAIL pointer is updated. The list has grown by one node!");
        hud.Say("Nice! Connected nodes make a chain.");
        yield return Pulse(nodes[nodes.Count - 1].transform);
    }

    IEnumerator RemoveRoutine(GameObject head, GameObject arrow)
    {
        if (arrow != null) StartCoroutine(ScaleOut(arrow));
        yield return ScaleOut(head);

        busy = true;
        var nStart = new Vector3[nodes.Count];
        var aStart = new Vector3[arrows.Count];
        for (int i = 0; i < nodes.Count; i++) nStart[i] = nodes[i].transform.localPosition;
        for (int i = 0; i < arrows.Count; i++) aStart[i] = arrows[i].transform.localPosition;

        for (float x = 0; x < 1; x += Time.deltaTime * 4f)
        {
            float s = Mathf.SmoothStep(0, 1, x);
            for (int i = 0; i < nodes.Count; i++)
                nodes[i].transform.localPosition = Vector3.Lerp(nStart[i], SlotPos(i), s);
            for (int i = 0; i < arrows.Count; i++)
                arrows[i].transform.localPosition = Vector3.Lerp(aStart[i], ArrowPos(i), s);
            yield return null;
        }
        for (int i = 0; i < nodes.Count; i++) nodes[i].transform.localPosition = SlotPos(i);
        for (int i = 0; i < arrows.Count; i++) arrows[i].transform.localPosition = ArrowPos(i);
        busy = false;
    }

    IEnumerator MoveNode(Transform t, Vector3 to)
    {
        busy = true;
        Vector3 from = t.localPosition;
        for (float x = 0; x < 1; x += Time.deltaTime * 3f)
        {
            t.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0, 1, x));
            yield return null;
        }
        t.localPosition = to;
        busy = false;
    }

    IEnumerator Pulse(Transform t)
    {
        busy = true;
        Vector3 baseScale = t.localScale;
        for (float x = 0; x < 1; x += Time.deltaTime * 3f)
        {
            t.localScale = baseScale * (1f + 0.3f * Mathf.Sin(x * Mathf.PI));
            yield return null;
        }
        t.localScale = baseScale;
        busy = false;
    }

    // ---------- Builders ----------

    GameObject SpawnNode(Vector3 localPos, bool animate)
    {
        var b = Instantiate(blockPrefab, holder);
        b.transform.localPosition = localPos;
        b.transform.localRotation = Quaternion.identity;
        b.transform.localScale = blockPrefab.transform.localScale * nodeScale;
        SetLabel(b, nextLetter);
        nextLetter++;
        if (animate) StartCoroutine(ScaleIn(b.transform));
        return b;
    }

    GameObject MakeArrow(int i)
    {
        var root = new GameObject("Arrow");
        root.transform.SetParent(holder, false);
        root.transform.localPosition = ArrowPos(i);

        var shaft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shaft.transform.SetParent(root.transform, false);
        shaft.transform.localPosition = new Vector3(-0.0075f, 0, 0);
        shaft.transform.localScale = new Vector3(0.035f, 0.008f, 0.008f);

        var tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.transform.SetParent(root.transform, false);
        tip.transform.localPosition = new Vector3(0.012f, 0, 0);
        tip.transform.localRotation = Quaternion.Euler(0, 45, 0);
        tip.transform.localScale = new Vector3(0.016f, 0.008f, 0.016f);

        foreach (var go in new[] { shaft, tip })
        {
            Destroy(go.GetComponent<Collider>());
            if (arrowMaterial != null) go.GetComponent<Renderer>().sharedMaterial = arrowMaterial;
        }
        return root;
    }
}
