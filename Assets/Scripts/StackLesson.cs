using System.Collections.Generic;
using UnityEngine;

public class StackLesson : Lesson
{
    [SerializeField] float blockHeight = 0.05f;

    readonly Stack<GameObject> blocks = new();
    int nextLetter;

    static readonly string[] pushCode = { "stack.Push(item);", "// item is now on TOP" };
    static readonly string[] popCode  = { "if (stack.Count == 0) return;", "item = stack.Pop();", "// TOP item removed" };

    protected override void OnEnter()
    {
        blocks.Clear();
        nextLetter = 0;
        for (int i = 0; i < 3; i++) SpawnBlock(false);
        hud.Say("Hi, I'm Mew! A stack works like a pile of books. Try PUSH!");
        hud.Explain("Stack = Last In, First Out (LIFO).");
        hud.ShowCode(pushCode, -1);
    }

    public void Push()
    {
        if (busy || holder == null) return;
        SpawnBlock(true);
        hud.ShowCode(pushCode, 0);
        hud.Explain("PUSH adds the new item to the TOP.");
        hud.Say("Nice! The newest block sits on top.");
    }

    public void Pop()
    {
        if (busy || holder == null) return;
        if (blocks.Count == 0)
        {
            hud.ShowCode(popCode, 0);
            hud.Explain("The stack is empty. Popping now would be an underflow!");
            hud.Say("Hint: push something first!");
            return;
        }
        var top = blocks.Pop();
        nextLetter--;
        StartCoroutine(ScaleOut(top));
        hud.ShowCode(popCode, 1);
        hud.Explain("POP removes the most recently added item first (LIFO).");
        hud.Say("See? The last one in was the first one out!");
    }

    void SpawnBlock(bool animate)
    {
        var b = Instantiate(blockPrefab, holder);
        b.transform.localPosition = new Vector3(0, blocks.Count * blockHeight + blockHeight / 2f, 0);
        b.transform.localRotation = Quaternion.identity;
        SetLabel(b, nextLetter);
        nextLetter++;
        blocks.Push(b);
        if (animate) StartCoroutine(ScaleIn(b.transform));
    }
}