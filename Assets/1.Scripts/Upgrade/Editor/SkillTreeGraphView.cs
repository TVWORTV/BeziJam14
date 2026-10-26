using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Node graph for one SkillTreeAsset. The asset is the source of truth: every graph
/// operation (add / delete / connect / move) is written straight into it with Undo support.
///
/// An edge from node A to node B means A.dependant contains B, i.e. completing A unlocks B.
/// A node can have any number of incoming and outgoing edges.
/// </summary>
public class SkillTreeGraphView : GraphView
{
    private SkillTreeAsset asset;
    private bool loading;
    private IVisualElementScheduledItem pendingSelectionNotify;

    /// <summary>Raised (deferred) after nodes or edges were added or removed.</summary>
    public Action StructureChanged;

    /// <summary>Raised (deferred) when the set of selected nodes changes.</summary>
    public Action SelectionChanged;

    public SkillTreeGraphView()
    {
        style.flexGrow = 1;

        SetupZoom(0.25f, 2f);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        graphViewChanged = OnGraphViewChanged;
    }

    // ---------------------------------------------------------------- loading

    public void Load(SkillTreeAsset newAsset, bool frame)
    {
        loading = true;
        try
        {
            ClearSelection();
            foreach (GraphElement element in graphElements.ToList()) RemoveElement(element);

            asset = newAsset;
            if (asset == null) return;

            List<UpgradeOption> options = asset.tree.options;

            // Assets edited outside the tool (or created before layout data existed) get an automatic layout.
            if (asset.NodePositionCount != options.Count)
            {
                asset.SetAllNodePositions(SkillTreeEditorUtility.ComputeLayout(options));
                EditorUtility.SetDirty(asset);
            }

            var views = new List<UpgradeNodeView>();
            for (int i = 0; i < options.Count; i++)
            {
                UpgradeNodeView view = CreateNodeView(i);
                AddElement(view);
                views.Add(view);
            }

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i]?.dependant == null) continue;
                foreach (UpgradeOption dependant in options[i].dependant)
                {
                    int j = options.IndexOf(dependant);
                    if (j < 0 || views[j].Input == null) continue;
                    AddElement(views[i].Output.ConnectTo(views[j].Input));
                }
            }
        }
        finally
        {
            loading = false;
        }

        if (frame && asset != null) schedule.Execute(() => FrameAll()).ExecuteLater(30);
    }

    /// <summary>Rebuilds the graph from the asset (used after undo/redo) without moving the camera.</summary>
    public void Reload() => Load(asset, false);

    private UpgradeNodeView CreateNodeView(int index)
    {
        var view = new UpgradeNodeView(asset, index);
        view.SetPosition(new Rect(asset.GetNodePosition(index), Vector2.zero));
        return view;
    }

    // --------------------------------------------------------------- queries

    public List<UpgradeNodeView> SelectedNodes() => selection.OfType<UpgradeNodeView>().ToList();

    public void RefreshSummaries()
    {
        foreach (UpgradeNodeView view in nodes.ToList().OfType<UpgradeNodeView>()) view.Refresh();
    }

    // ------------------------------------------------------------ add / layout

    public void AddNodeAtViewCenter(Type nodeType)
    {
        Vector2 center = contentViewContainer.WorldToLocal(worldBound.center);
        AddNode(nodeType, center - new Vector2(100f, 40f));
    }

    public void AddNode(Type nodeType, Vector2 position)
    {
        if (asset == null) return;

        Undo.RecordObject(asset, "Add Skill Node");
        var option = (UpgradeOption)Activator.CreateInstance(nodeType);
        int index = asset.AddOption(option, position);
        EditorUtility.SetDirty(asset);

        UpgradeNodeView view = CreateNodeView(index);
        AddElement(view);
        ClearSelection();
        AddToSelection(view);

        NotifyStructureChanged();
    }

    public void AutoLayout()
    {
        if (asset == null) return;

        Undo.RecordObject(asset, "Auto Layout Skill Tree");
        Vector2[] layout = SkillTreeEditorUtility.ComputeLayout(asset.tree.options);
        asset.SetAllNodePositions(layout);
        EditorUtility.SetDirty(asset);

        foreach (UpgradeNodeView view in nodes.ToList().OfType<UpgradeNodeView>())
        {
            if (view.Index >= 0 && view.Index < layout.Length)
                view.SetPosition(new Rect(layout[view.Index], Vector2.zero));
        }

        schedule.Execute(() => FrameAll()).ExecuteLater(30);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (asset != null && evt.target is GraphView)
        {
            Vector2 position = contentViewContainer.WorldToLocal(evt.mousePosition);
            foreach (Type type in SkillTreeEditorUtility.GetNodeTypes())
            {
                Type captured = type;
                evt.menu.AppendAction($"Add Node/{SkillTreeEditorUtility.Nicify(captured)}", _ => AddNode(captured, position));
            }

            evt.menu.AppendSeparator();
        }

        base.BuildContextualMenu(evt);
    }

    // ------------------------------------------------------------ connections

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatible = new List<Port>();
        foreach (Port port in ports.ToList())
        {
            if (port == startPort || port.node == startPort.node || port.direction == startPort.direction) continue;

            bool startIsOutput = startPort.direction == Direction.Output;
            Node parent = startIsOutput ? startPort.node : port.node;
            Node child = startIsOutput ? port.node : startPort.node;

            if (parent is UpgradeNodeView parentView && child is UpgradeNodeView childView && CanConnect(parentView, childView))
                compatible.Add(port);
        }

        return compatible;
    }

    /// <summary>A connection must target a node with an input, not duplicate an edge, and not create a cycle.</summary>
    private bool CanConnect(UpgradeNodeView parent, UpgradeNodeView child)
    {
        UpgradeOption source = parent.Option;
        UpgradeOption target = child.Option;
        if (source == null || target == null || child.Input == null) return false;
        if (source.dependant != null && source.dependant.Contains(target)) return false;
        return !Reaches(target, source);
    }

    // Cycles would make Standart.Revert recurse forever, so they are blocked at edit time.
    private static bool Reaches(UpgradeOption start, UpgradeOption goal)
    {
        var visited = new HashSet<UpgradeOption>();
        var stack = new Stack<UpgradeOption>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            UpgradeOption current = stack.Pop();
            if (current == goal) return true;
            if (current == null || !visited.Add(current) || current.dependant == null) continue;
            foreach (UpgradeOption next in current.dependant) stack.Push(next);
        }

        return false;
    }

    private bool TryConnect(UpgradeNodeView parent, UpgradeNodeView child)
    {
        if (!CanConnect(parent, child)) return false;

        UpgradeOption source = parent.Option;
        source.dependant ??= new List<UpgradeOption>();
        source.dependant.Add(child.Option);
        return true;
    }

    private static void Disconnect(UpgradeNodeView parent, UpgradeNodeView child)
    {
        UpgradeOption source = parent.Option;
        UpgradeOption target = child.Option;
        if (source?.dependant == null || target == null) return;
        source.dependant.Remove(target);
    }

    private void RemoveNode(UpgradeNodeView view)
    {
        int removedIndex = view.Index;
        if (removedIndex < 0 || removedIndex >= asset.tree.options.Count) return;

        asset.RemoveOptionAt(removedIndex);
        view.Index = -1;

        // Keep the remaining views pointing at the same options.
        foreach (UpgradeNodeView other in nodes.ToList().OfType<UpgradeNodeView>())
        {
            if (other.Index > removedIndex) other.Index--;
        }
    }

    // ------------------------------------------------------- graph -> asset sync

    private GraphViewChange OnGraphViewChanged(GraphViewChange change)
    {
        if (loading || asset == null) return change;

        bool dirty = false;
        bool structural = false;

        if (change.elementsToRemove != null)
        {
            Undo.RecordObject(asset, "Delete Skill Tree Elements");

            // Edges first, while every node index is still valid.
            foreach (Edge edge in change.elementsToRemove.OfType<Edge>())
            {
                if (edge.output?.node is UpgradeNodeView parent && edge.input?.node is UpgradeNodeView child)
                    Disconnect(parent, child);
            }

            foreach (UpgradeNodeView view in change.elementsToRemove.OfType<UpgradeNodeView>().ToList())
                RemoveNode(view);

            dirty = true;
            structural = true;
        }

        if (change.edgesToCreate != null)
        {
            Undo.RecordObject(asset, "Connect Skills");

            foreach (Edge edge in change.edgesToCreate.ToList())
            {
                bool accepted = edge.output?.node is UpgradeNodeView parent
                                && edge.input?.node is UpgradeNodeView child
                                && TryConnect(parent, child);

                if (accepted) dirty = true;
                else change.edgesToCreate.Remove(edge);
            }
        }

        if (change.movedElements != null)
        {
            Undo.RecordObject(asset, "Move Skill Nodes");

            foreach (UpgradeNodeView view in change.movedElements.OfType<UpgradeNodeView>())
                asset.SetNodePosition(view.Index, view.GetPosition().position);

            dirty = true;
        }

        if (dirty) EditorUtility.SetDirty(asset);
        if (structural) NotifyStructureChanged();

        return change;
    }

    // ------------------------------------------------------------- notifications

    private void NotifyStructureChanged()
    {
        schedule.Execute(() => StructureChanged?.Invoke());
    }

    public override void AddToSelection(ISelectable selectable)
    {
        base.AddToSelection(selectable);
        NotifySelectionChanged();
    }

    public override void RemoveFromSelection(ISelectable selectable)
    {
        base.RemoveFromSelection(selectable);
        NotifySelectionChanged();
    }

    public override void ClearSelection()
    {
        base.ClearSelection();
        NotifySelectionChanged();
    }

    // Selecting several nodes fires many events in one frame, so they are coalesced.
    private void NotifySelectionChanged()
    {
        if (loading || pendingSelectionNotify != null) return;

        pendingSelectionNotify = schedule.Execute(() =>
        {
            pendingSelectionNotify = null;
            SelectionChanged?.Invoke();
        });
    }
}
