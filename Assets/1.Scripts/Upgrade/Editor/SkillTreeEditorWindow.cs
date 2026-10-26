using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Node-based editor for SkillTreeAsset files in Assets/Resources/SkillTrees.
///   "+"        creates a new skill tree asset (asks for a name).
///   Dropdown   lists the existing trees and switches the one being edited.
///   Graph      right-click to add nodes, drag from "Unlocks" to "Requires" to add dependencies.
///   Right pane edits the effects of the selected node.
/// </summary>
public class SkillTreeEditorWindow : EditorWindow
{
    [SerializeField] private SkillTreeAsset current;

    private SkillTreeGraphView graph;
    private ToolbarMenu treeMenu;
    private ScrollView inspectorScroll;
    private SerializedObject serialized;

    // ----------------------------------------------------------------- opening

    [MenuItem("Tools/DD System/Skill Tree Editor")]
    public static void Open() => ShowWindow();

    public static void Open(SkillTreeAsset asset)
    {
        SkillTreeEditorWindow window = ShowWindow();
        // If the GUI is not built yet this just records the asset and CreateGUI loads it.
        window.Load(asset, true);
    }

    private static SkillTreeEditorWindow ShowWindow()
    {
        var window = GetWindow<SkillTreeEditorWindow>();
        window.titleContent = new GUIContent("Skill Tree Editor");
        window.minSize = new Vector2(760f, 420f);
        return window;
    }

    [MenuItem("Assets/Open in Skill Tree Editor")]
    private static void OpenSelected() => Open(Selection.activeObject as SkillTreeAsset);

    [MenuItem("Assets/Open in Skill Tree Editor", true)]
    private static bool OpenSelectedValidate() => Selection.activeObject is SkillTreeAsset;

    private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;

    private void OnDisable() => Undo.undoRedoPerformed -= OnUndoRedo;

    // ---------------------------------------------------------------------- UI

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.Clear();

        root.Add(BuildToolbar());

        var split = new TwoPaneSplitView(1, 320f, TwoPaneSplitViewOrientation.Horizontal);
        split.style.flexGrow = 1;

        graph = new SkillTreeGraphView();
        graph.StructureChanged = OnStructureChanged;
        graph.SelectionChanged = RebuildInspector;
        split.Add(graph);

        inspectorScroll = new ScrollView();
        inspectorScroll.style.paddingLeft = 8;
        inspectorScroll.style.paddingRight = 8;
        inspectorScroll.style.paddingTop = 8;
        split.Add(inspectorScroll);

        root.Add(split);

        if (current != null && AssetDatabase.Contains(current)) Load(current, true);
        else Load(null, false);
    }

    private Toolbar BuildToolbar()
    {
        var toolbar = new Toolbar();

        // "+" : create a new skill tree asset.
        var plus = new ToolbarButton(OnNewTreeClicked) { text = "+", tooltip = "Create a new skill tree" };
        plus.style.unityFontStyleAndWeight = FontStyle.Bold;
        plus.style.minWidth = 26;
        toolbar.Add(plus);

        // Dropdown : pick an existing skill tree. Rebuilt on every click so new assets always show up.
        treeMenu = new ToolbarMenu { text = "Select Skill Tree" };
        treeMenu.style.minWidth = 180;
        treeMenu.tooltip = "Skill trees in " + SkillTreeEditorUtility.TreeFolder;
        treeMenu.RegisterCallback<PointerDownEvent>(_ => PopulateTreeMenu(), TrickleDown.TrickleDown);
        PopulateTreeMenu();
        toolbar.Add(treeMenu);

        toolbar.Add(new ToolbarSpacer { flex = true });

        var addNode = new ToolbarMenu { text = "Add Node" };
        foreach (Type type in SkillTreeEditorUtility.GetNodeTypes())
        {
            Type captured = type;
            addNode.menu.AppendAction(SkillTreeEditorUtility.Nicify(captured), _ => graph?.AddNodeAtViewCenter(captured));
        }
        toolbar.Add(addNode);

        toolbar.Add(new ToolbarButton(() => graph?.AutoLayout()) { text = "Auto Layout" });
        toolbar.Add(new ToolbarButton(() => graph?.FrameAll()) { text = "Frame All" });
        toolbar.Add(new ToolbarButton(Save) { text = "Save" });

        return toolbar;
    }

    private void PopulateTreeMenu()
    {
        treeMenu.menu.ClearItems();

        var trees = SkillTreeEditorUtility.FindAllTrees();
        if (trees.Count == 0)
        {
            treeMenu.menu.AppendAction("No skill trees yet - press +", _ => { }, DropdownMenuAction.Status.Disabled);
            return;
        }

        foreach (SkillTreeAsset tree in trees)
        {
            SkillTreeAsset captured = tree;
            treeMenu.menu.AppendAction(
                captured.name,
                _ => Load(captured, true),
                _ => captured == current ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
        }
    }

    private void UpdateToolbar()
    {
        treeMenu.text = current != null ? current.name : "Select Skill Tree";
    }

    // ------------------------------------------------------------ actions

    private void OnNewTreeClicked()
    {
        SkillTreeNamePrompt.Show(name =>
        {
            SkillTreeAsset created = SkillTreeEditorUtility.CreateTreeAsset(name);
            PopulateTreeMenu();
            Load(created, true);
        });
    }

    private void Save()
    {
        if (current == null) return;
        EditorUtility.SetDirty(current);
        AssetDatabase.SaveAssetIfDirty(current);
    }

    private void Load(SkillTreeAsset asset, bool frame)
    {
        current = asset;
        if (graph == null) return;

        serialized = asset != null ? new SerializedObject(asset) : null;

        graph.Load(asset, frame);
        UpdateToolbar();
        RebuildInspector();
    }

    private void OnUndoRedo()
    {
        if (graph == null || current == null) return;

        serialized?.Update();
        graph.Reload();
        RebuildInspector();
        Repaint();
    }

    private void OnStructureChanged()
    {
        serialized?.Update();
        RebuildInspector();
    }

    // -------------------------------------------------------------- inspector

    private void RebuildInspector()
    {
        if (inspectorScroll == null) return;
        inspectorScroll.Clear();

        if (current == null || serialized == null)
        {
            inspectorScroll.Add(new Label("No skill tree open.\nPress + to create one, or pick one from the dropdown."));
            return;
        }

        var selected = graph.SelectedNodes();
        if (selected.Count == 0 || selected[0].Index < 0 || selected[0].Option == null)
        {
            inspectorScroll.Add(new Label("Select a node to edit its effects.\nRight-click the graph to add nodes."));
            return;
        }

        UpgradeNodeView view = selected[0];
        int optionIndex = view.Index;

        serialized.Update();
        SerializedProperty effects = GetEffectsProperty(optionIndex);

        // A fresh container each time keeps the value tracker from piling up across rebuilds.
        var content = new VisualElement();

        var heading = new Label($"{SkillTreeEditorUtility.Nicify(view.Option.GetType())}  (node {optionIndex})");
        heading.style.unityFontStyleAndWeight = FontStyle.Bold;
        heading.style.fontSize = 13;
        content.Add(heading);

        if (selected.Count > 1)
        {
            var note = new Label($"{selected.Count} nodes selected - showing the first one.");
            note.style.opacity = 0.7f;
            content.Add(note);
        }

        if (effects == null)
        {
            var create = new Button(() => CreateApplicationStrategy(optionIndex)) { text = "Create effect list" };
            create.style.marginTop = 8;
            content.Add(create);
            inspectorScroll.Add(content);
            return;
        }

        var effectsHeader = new VisualElement();
        effectsHeader.style.flexDirection = FlexDirection.Row;
        effectsHeader.style.alignItems = Align.Center;
        effectsHeader.style.marginTop = 10;

        var effectsLabel = new Label("Effects when completed");
        effectsLabel.style.flexGrow = 1;
        effectsHeader.Add(effectsLabel);

        var addButton = new Button { text = "Add Effect" };
        addButton.clicked += () => ShowAddEffectMenu(addButton, optionIndex);
        effectsHeader.Add(addButton);
        content.Add(effectsHeader);

        if (effects.arraySize == 0)
        {
            var empty = new Label("No effects yet.");
            empty.style.opacity = 0.7f;
            empty.style.marginTop = 6;
            content.Add(empty);
        }

        for (int i = 0; i < effects.arraySize; i++)
        {
            content.Add(BuildEffectBox(effects.GetArrayElementAtIndex(i), optionIndex, i));
        }

        inspectorScroll.Add(content);
        content.Bind(serialized);

        // Fires after any bound field changes, so the node summaries stay current.
        content.TrackSerializedObjectValue(serialized, _ =>
        {
            EditorUtility.SetDirty(current);
            graph.RefreshSummaries();
        });
    }

    private VisualElement BuildEffectBox(SerializedProperty effect, int optionIndex, int effectIndex)
    {
        var box = new VisualElement();
        box.style.marginTop = 6;
        box.style.paddingLeft = 6;
        box.style.paddingRight = 6;
        box.style.paddingTop = 4;
        box.style.paddingBottom = 6;
        box.style.borderTopWidth = 1;
        box.style.borderBottomWidth = 1;
        box.style.borderLeftWidth = 1;
        box.style.borderRightWidth = 1;
        box.style.borderTopColor = box.style.borderBottomColor =
            box.style.borderLeftColor = box.style.borderRightColor = new Color(0f, 0f, 0f, 0.35f);
        box.style.borderTopLeftRadius = box.style.borderTopRightRadius =
            box.style.borderBottomLeftRadius = box.style.borderBottomRightRadius = 3;

        object value = effect.managedReferenceValue;

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.alignItems = Align.Center;

        var title = new Label(value != null ? SkillTreeEditorUtility.Nicify(value.GetType()) : "(empty effect)");
        title.style.flexGrow = 1;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.Add(title);

        var remove = new Button(() => RemoveEffect(optionIndex, effectIndex)) { text = "Remove", tooltip = "Remove this effect" };
        header.Add(remove);
        box.Add(header);

        if (value != null)
        {
            // Draw the effect's fields without the extra foldout PropertyField adds for a managed reference.
            SerializedProperty child = effect.Copy();
            SerializedProperty end = effect.GetEndProperty();
            bool enterChildren = true;
            while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
            {
                box.Add(new PropertyField(child.Copy()));
                enterChildren = false;
            }
        }

        return box;
    }

    // --------------------------------------------------- inspector operations
    // Properties are looked up again by index on every operation instead of being cached.

    private SerializedProperty GetEffectsProperty(int optionIndex)
    {
        SerializedProperty options = serialized.FindProperty("tree.options");
        if (options == null || optionIndex < 0 || optionIndex >= options.arraySize) return null;

        SerializedProperty application = options.GetArrayElementAtIndex(optionIndex).FindPropertyRelative("application");
        return application?.FindPropertyRelative("effects");
    }

    private void CreateApplicationStrategy(int optionIndex)
    {
        serialized.Update();
        SerializedProperty options = serialized.FindProperty("tree.options");
        if (options == null || optionIndex < 0 || optionIndex >= options.arraySize) return;

        SerializedProperty application = options.GetArrayElementAtIndex(optionIndex).FindPropertyRelative("application");
        if (application == null) return;

        application.managedReferenceValue = new ApplicationStrategy();
        CommitInspectorChange();
    }

    private void ShowAddEffectMenu(VisualElement anchor, int optionIndex)
    {
        var menu = new GenericMenu();
        var types = SkillTreeEditorUtility.GetEffectTypes();

        if (types.Count == 0) menu.AddDisabledItem(new GUIContent("No ApplicationEffect types found"));

        foreach (Type type in types)
        {
            Type captured = type;
            menu.AddItem(new GUIContent(SkillTreeEditorUtility.Nicify(captured)), false, () => AddEffect(optionIndex, captured));
        }

        menu.DropDown(anchor.worldBound);
    }

    private void AddEffect(int optionIndex, Type effectType)
    {
        serialized.Update();
        SerializedProperty effects = GetEffectsProperty(optionIndex);
        if (effects == null) return;

        int newIndex = effects.arraySize;
        effects.InsertArrayElementAtIndex(newIndex);
        // Growing the array can duplicate the previous element, so always assign a fresh instance.
        effects.GetArrayElementAtIndex(newIndex).managedReferenceValue = Activator.CreateInstance(effectType);
        CommitInspectorChange();
    }

    private void RemoveEffect(int optionIndex, int effectIndex)
    {
        serialized.Update();
        SerializedProperty effects = GetEffectsProperty(optionIndex);
        if (effects == null || effectIndex < 0 || effectIndex >= effects.arraySize) return;

        int sizeBefore = effects.arraySize;
        effects.DeleteArrayElementAtIndex(effectIndex);
        // Some property types need a second delete to actually remove a non-null element.
        if (effects.arraySize == sizeBefore) effects.DeleteArrayElementAtIndex(effectIndex);
        CommitInspectorChange();
    }

    private void CommitInspectorChange()
    {
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(current);
        graph.RefreshSummaries();
        RebuildInspector();
    }
}
