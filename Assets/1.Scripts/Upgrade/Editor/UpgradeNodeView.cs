using System.Text;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Visual node for one UpgradeOption. It deliberately stores the option's index in
/// SkillTree.options rather than the object itself, so it stays valid even if Unity
/// re-creates managed-reference instances while applying SerializedObject edits.
/// </summary>
public class UpgradeNodeView : Node
{
    private readonly SkillTreeAsset asset;
    private readonly Label summary;

    /// <summary>Index into asset.tree.options, or -1 once the node has been deleted.</summary>
    public int Index;

    /// <summary>"Requires" port. Null for Initial nodes, which are always available.</summary>
    public Port Input { get; }

    /// <summary>"Unlocks" port. Connecting A to B means A.dependant contains B.</summary>
    public Port Output { get; }

    public UpgradeOption Option
    {
        get
        {
            var options = asset.tree.options;
            return Index >= 0 && Index < options.Count ? options[Index] : null;
        }
    }

    public UpgradeNodeView(SkillTreeAsset asset, int index)
    {
        this.asset = asset;
        Index = index;

        UpgradeOption option = Option;
        bool isRoot = option is Initial;

        title = option == null ? "Missing Node" : SkillTreeEditorUtility.Nicify(option.GetType());
        if (isRoot) titleContainer.style.backgroundColor = new Color(0.16f, 0.46f, 0.26f);

        if (!isRoot)
        {
            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(UpgradeOption));
            Input.portName = "Requires";
            inputContainer.Add(Input);
        }

        Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(UpgradeOption));
        Output.portName = "Unlocks";
        outputContainer.Add(Output);

        summary = new Label();
        summary.style.whiteSpace = WhiteSpace.Normal;
        summary.style.marginLeft = 8;
        summary.style.marginRight = 8;
        summary.style.marginTop = 4;
        summary.style.marginBottom = 6;
        summary.style.minWidth = 150;
        extensionContainer.Add(summary);

        RefreshExpandedState();
        RefreshPorts();
        Refresh();
    }

    /// <summary>Re-reads the option's effects into the node's summary text.</summary>
    public void Refresh()
    {
        UpgradeOption option = Option;
        var effects = option?.application?.effects;

        if (option == null)
        {
            summary.text = "Node data is missing.";
            return;
        }

        if (effects == null || effects.Count == 0)
        {
            summary.text = "No effects";
            return;
        }

        const int maxShown = 4;
        int shown = Mathf.Min(effects.Count, maxShown);
        var text = new StringBuilder();
        for (int i = 0; i < shown; i++)
        {
            if (i > 0) text.Append('\n');
            text.Append(SkillTreeEditorUtility.Describe(effects[i]));
        }

        if (effects.Count > shown) text.Append($"\n+{effects.Count - shown} more");
        summary.text = text.ToString();
    }
}
