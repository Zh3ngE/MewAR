public class Question
{
    public readonly string text;
    public readonly string[] options;
    public readonly int correct;     // index into options
    public readonly string hint;

    public Question(string text, string[] options, int correct, string hint)
    {
        this.text = text;
        this.options = options;
        this.correct = correct;
        this.hint = hint;
    }
}

public static class QuizBank
{
    public static readonly Question[] All =
    {
        new Question("Which principle does a STACK follow?",
            new[] { "FIFO", "LIFO", "Random order", "Sorted order" }, 1,
            "A stack is like a pile of books: Last In, First Out (LIFO)."),

        new Question("A stack holds A, B, C (C is on top). You call Pop(). Which item is removed?",
            new[] { "A", "B", "C", "All of them" }, 2,
            "Pop always removes the TOP item, which was added last."),

        new Question("Which principle does a QUEUE follow?",
            new[] { "LIFO", "FIFO", "Random order", "Largest first" }, 1,
            "A queue is like people waiting for a bus: First In, First Out (FIFO)."),

        new Question("A queue holds A, B, C (A is at the front). You call Dequeue(). Which item is removed?",
            new[] { "A", "B", "C", "The middle one" }, 0,
            "Dequeue removes the oldest item, from the FRONT."),

        new Question("Besides its value, what does each linked list node store?",
            new[] { "Its index number", "A reference to the next node", "The size of the list", "A sorted copy of the data" }, 1,
            "Nodes are chained together through NEXT references."),

        new Question("When you remove the head of a linked list, what happens to the head pointer?",
            new[] { "It becomes null", "It moves to head.next", "It moves to the tail", "It stays the same" }, 1,
            "The head pointer moves on to the second node, head.next."),

        new Question("How many children can a node in a binary tree have, at most?",
            new[] { "1", "2", "3", "Unlimited" }, 1,
            "'Binary' means two: a LEFT child and a RIGHT child."),

        new Question("Root A has children B and C. B has children D and E. What is the PREORDER traversal?",
            new[] { "A B D E C", "D B E A C", "D E B C A", "A B C D E" }, 0,
            "Preorder visits the node first, then left, then right."),

        new Question("What does bubble sort do repeatedly?",
            new[] { "Splits the list in half", "Compares neighbours and swaps if out of order", "Picks a random element", "Reverses the list" }, 1,
            "Bubble sort compares adjacent elements and swaps them when needed."),

        new Question("Bubble sort on 4, 2, 3, 1. What does the list look like after the FIRST full pass?",
            new[] { "2, 3, 1, 4", "1, 2, 3, 4", "2, 4, 3, 1", "4, 2, 3, 1" }, 0,
            "In pass 1 the biggest value (4) bubbles all the way to the right.")
    };
}