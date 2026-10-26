using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Small modal window that asks for the name of a new skill tree asset.</summary>
public class SkillTreeNamePrompt : EditorWindow
{
    private Action<string> onConfirm;
    private TextField nameField;
    private HelpBox errorBox;
    private Button createButton;

    public static void Show(Action<string> onConfirm)
    {
        var window = CreateInstance<SkillTreeNamePrompt>();
        window.onConfirm = onConfirm;
        window.titleContent = new GUIContent("New Skill Tree");
        window.minSize = new Vector2(340f, 130f);
        window.maxSize = window.minSize;
        window.ShowModalUtility();
    }

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        root.style.paddingLeft = 10;
        root.style.paddingRight = 10;
        root.style.paddingTop = 10;
        root.style.paddingBottom = 10;

        root.Add(new Label($"Created in {SkillTreeEditorUtility.TreeFolder}"));

        nameField = new TextField("Name") { value = "NewSkillTree" };
        nameField.style.marginTop = 6;
        nameField.RegisterValueChangedCallback(_ => Validate());
        nameField.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        root.Add(nameField);

        errorBox = new HelpBox(string.Empty, HelpBoxMessageType.Error);
        errorBox.style.display = DisplayStyle.None;
        root.Add(errorBox);

        var buttons = new VisualElement();
        buttons.style.flexDirection = FlexDirection.Row;
        buttons.style.justifyContent = Justify.FlexEnd;
        buttons.style.marginTop = 8;

        buttons.Add(new Button(Close) { text = "Cancel" });
        createButton = new Button(Confirm) { text = "Create" };
        buttons.Add(createButton);
        root.Add(buttons);

        Validate();
        nameField.schedule.Execute(() => nameField.Focus());
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            Confirm();
            evt.StopPropagation();
        }
        else if (evt.keyCode == KeyCode.Escape)
        {
            Close();
            evt.StopPropagation();
        }
    }

    private bool Validate()
    {
        bool valid = SkillTreeEditorUtility.IsValidTreeName(nameField.value, out string error);
        errorBox.text = error ?? string.Empty;
        errorBox.style.display = valid ? DisplayStyle.None : DisplayStyle.Flex;
        createButton.SetEnabled(valid);
        return valid;
    }

    private void Confirm()
    {
        if (!Validate()) return;

        string name = nameField.value.Trim();
        Action<string> callback = onConfirm;
        Close();
        callback?.Invoke(name);
    }
}
