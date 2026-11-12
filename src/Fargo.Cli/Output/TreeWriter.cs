namespace Fargo.Cli.Output;

/// <summary>
/// A node in a renderable tree: a label plus zero or more child nodes.
/// </summary>
public sealed class TreeNode(string label)
{
    public string Label { get; } = label;

    public List<TreeNode> Children { get; } = new();

    /// <summary>Adds an existing child node and returns it for chaining.</summary>
    public TreeNode Add(TreeNode child)
    {
        Children.Add(child);
        return child;
    }

    /// <summary>Adds a child node by label and returns the created node.</summary>
    public TreeNode Add(string label)
    {
        var child = new TreeNode(label);
        Children.Add(child);
        return child;
    }
}

/// <summary>
/// Renders a <see cref="TreeNode"/> hierarchy to standard output using
/// box-drawing branch connectors, e.g.:
///
/// <code>
/// ├─ group-a
/// │  ├─ user-1
/// │  └─ user-2
/// └─ group-b
///    └─ user-3
/// </code>
/// </summary>
public static class TreeWriter
{
    /// <summary>
    /// Writes a forest (multiple top-level nodes with no shared root).
    /// </summary>
    public static void WriteForest(IReadOnlyList<TreeNode> roots)
    {
        for (var i = 0; i < roots.Count; i++)
        {
            WriteNode(roots[i], indent: "", isLast: i == roots.Count - 1);
        }
    }

    private static void WriteChildren(TreeNode node, string indent)
    {
        for (var i = 0; i < node.Children.Count; i++)
        {
            WriteNode(node.Children[i], indent, isLast: i == node.Children.Count - 1);
        }
    }

    private static void WriteNode(TreeNode node, string indent, bool isLast)
    {
        var connector = isLast ? "\u2514\u2500 " : "\u251C\u2500 ";
        Console.WriteLine($"{indent}{connector}{node.Label}");

        // Children of a last node are indented with spaces; otherwise keep the
        // vertical guide so deeper levels stay visually connected.
        var childIndent = indent + (isLast ? "   " : "\u2502  ");
        WriteChildren(node, childIndent);
    }
}
