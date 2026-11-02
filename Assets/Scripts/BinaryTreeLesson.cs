using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BinaryTreeLesson : Lesson
{
    [SerializeField] Material edgeMaterial;
    [SerializeField] float nodeScale = 0.8f;
    [SerializeField] Color selectedColor = new Color(1f, 0.84f, 0.31f);
    [SerializeField] Color currentColor  = new Color(1f, 0.55f, 0.2f);
    [SerializeField] Color visitedColor  = new Color(0.45f, 0.8f, 0.5f);

    enum Mode { Pre, In, Post }

    // Tree layout: 0=A 1=B 2=C 3=D 4=E
    static readonly int[] left   = {  1,  3, -1, -1, -1 };
    static readonly int[] right  = {  2,  4, -1, -1, -1 };
    static readonly int[] parent = { -1,  0,  0,  1,  1 };
    static readonly Vector3[] pos =
    {
        new Vector3( 0.00f, 0.28f, 0),   // A
        new Vector3(-0.12f, 0.18f, 0),   // B
        new Vector3( 0.12f, 0.18f, 0),   // C
        new Vector3(-0.18f, 0.08f, 0),   // D
        new Vector3(-0.06f, 0.08f, 0),   // E
    };

    static readonly string[] preCode =
    {
        "void Preorder(Node n) {",
        "  if (n == null) return;",
        "  Visit(n);            // FIRST",
        "  Preorder(n.left);",
        "  Preorder(n.right);",
        "}"
    };
    static readonly string[] inCode =
    {
        "void Inorder(Node n) {",
        "  if (n == null) return;",
        "  Inorder(n.left);",
        "  Visit(n);            // MIDDLE",
        "  Inorder(n.right);",
        "}"
    };
    static readonly string[] postCode =
    {
        "void Postorder(Node n) {",
        "  if (n == null) return;",
        "  Postorder(n.left);",
        "  Postorder(n.right);",
        "  Visit(n);            // LAST",
        "}"
    };

    readonly List<GameObject> nodes = new();
    readonly List<int> order = new();
    Mode mode;
    bool traversing;
    int step;          // how many nodes have been visited
    int selected = -1;
    Color baseColor;

    protected override void OnEnter()
    {
        nodes.Clear();
        order.Clear();
        traversing = false;
        step = 0;
        selected = -1;

        for (int i = 0; i < 5; i++)
        {
            var n = Instantiate(blockPrefab, holder);
            n.transform.localPosition = pos[i];
            n.transform.localRotation = Quaternion.identity;
            n.transform.localScale = blockPrefab.transform.localScale * nodeScale;
            SetLabel(n, i);
            nodes.Add(n);
        }
        baseColor = nodes[0].GetComponent<Renderer>().sharedMaterial.color;

        for (int i = 1; i < 5; i++) MakeEdge(parent[i], i);

        hud.Say("This is a binary tree! Tap any node to learn about it.");
        hud.Explain("Each node has at most two children: LEFT and RIGHT.");
        hud.ShowCode(preCode, -1);
        RefreshColors();
    }

    // ---------- Tap to select ----------

    void Update()
    {
        if (holder == null || !holder.gameObject.activeInHierarchy) return;

        var ts = Touchscreen.current;
        if (ts == null || !ts.primaryTouch.press.wasPressedThisFrame) return;
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(ts.primaryTouch.touchId.ReadValue())) return;

        Ray ray = Camera.main.ScreenPointToRay(ts.primaryTouch.position.ReadValue());
        if (!Physics.Raycast(ray, out var hit, 5f)) return;

        int i = nodes.IndexOf(hit.collider.gameObject);
        if (i < 0) return;

        selected = i;
        RefreshColors();
        StartCoroutine(Pulse(nodes[i].transform));
        hud.Explain(Describe(i));
        hud.Say("That's node " + Letter(i) + "!");
    }

    string Describe(int i)
    {
        bool isLeaf = left[i] < 0 && right[i] < 0;
        string role = i == 0 ? "the ROOT (no parent)" : isLeaf ? "a LEAF (no children)" : "an INTERNAL node";
        string p = parent[i] < 0 ? "none" : Letter(parent[i]);
        string kids = isLeaf ? "none"
            : (left[i] >= 0 ? Letter(left[i]) : "-") + " (left), " + (right[i] >= 0 ? Letter(right[i]) : "-") + " (right)";
        return $"Node {Letter(i)} is {role}.\nParent: {p}\nChildren: {kids}";
    }

    // ---------- Buttons ----------

    public void StartPreorder()  => StartTraversal(Mode.Pre);
    public void StartInorder()   => StartTraversal(Mode.In);
    public void StartPostorder() => StartTraversal(Mode.Post);

    void StartTraversal(Mode m)
    {
        if (busy || holder == null) return;
        mode = m;
        order.Clear();
        Walk(0, m);
        traversing = true;
        step = 0;
        selected = -1;
        RefreshColors();

        hud.ShowCode(CodeFor(m), -1);
        hud.Explain(m == Mode.Pre ? "Preorder: visit the node FIRST, then its left subtree, then its right subtree."
                  : m == Mode.In  ? "Inorder: the left subtree first, then the node, then the right subtree."
                                  : "Postorder: the left subtree, the right subtree, and the node LAST.");
        hud.Say("Tap NEXT STEP to visit one node at a time.");
    }

    public void NextStep()
    {
        if (busy || holder == null) return;
        if (!traversing)
        {
            hud.Say("Pick PREORDER, INORDER or POSTORDER first!");
            return;
        }
        if (step >= order.Count)
        {
            hud.Say("All done! Pick a traversal to run it again.");
            return;
        }

        step++;
        int current = order[step - 1];
        selected = -1;
        RefreshColors();
        StartCoroutine(Pulse(nodes[current].transform));

        hud.ShowCode(CodeFor(mode), VisitLine(mode));
        hud.Explain("Visited: " + VisitedText());
        hud.Say(step == order.Count
            ? "Traversal complete! That is the " + mode.ToString().ToLower() + " order."
            : "Visiting " + Letter(current) + "...");
    }

    // ---------- Helpers ----------

    void Walk(int n, Mode m)
    {
        if (n < 0) return;
        if (m == Mode.Pre) order.Add(n);
        Walk(left[n], m);
        if (m == Mode.In) order.Add(n);
        Walk(right[n], m);
        if (m == Mode.Post) order.Add(n);
    }

    static string[] CodeFor(Mode m) => m == Mode.Pre ? preCode : m == Mode.In ? inCode : postCode;
    static int VisitLine(Mode m) => m == Mode.Pre ? 2 : m == Mode.In ? 3 : 4;
    static string Letter(int i) => ((char)('A' + i)).ToString();

    string VisitedText()
    {
        var parts = new List<string>();
        for (int i = 0; i < step; i++) parts.Add(Letter(order[i]));
        return string.Join(" → ", parts);
    }

    void RefreshColors()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Color c = baseColor;
            int visitedIndex = traversing ? order.IndexOf(i) : -1;
            if (visitedIndex >= 0 && visitedIndex < step) c = visitedColor;
            if (traversing && step > 0 && order[step - 1] == i) c = currentColor;
            if (i == selected) c = selectedColor;
            nodes[i].GetComponent<Renderer>().material.color = c;
        }
    }

    void MakeEdge(int a, int b)
    {
        Vector3 d = pos[b] - pos[a];
        var e = GameObject.CreatePrimitive(PrimitiveType.Cube);
        e.name = "Edge";
        e.transform.SetParent(holder, false);
        e.transform.localPosition = (pos[a] + pos[b]) / 2f;
        e.transform.localRotation = Quaternion.LookRotation(d);
        e.transform.localScale = new Vector3(0.006f, 0.006f, d.magnitude);
        Destroy(e.GetComponent<Collider>());
        if (edgeMaterial != null) e.GetComponent<Renderer>().sharedMaterial = edgeMaterial;
    }

    IEnumerator Pulse(Transform t)
    {
        Vector3 baseScale = blockPrefab.transform.localScale * nodeScale;
        for (float x = 0; x < 1; x += Time.deltaTime * 4f)
        {
            t.localScale = baseScale * (1f + 0.3f * Mathf.Sin(x * Mathf.PI));
            yield return null;
        }
        t.localScale = baseScale;
    }
}