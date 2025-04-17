using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEngine.UIElements;
using System.Runtime.Serialization.Formatters.Binary;

public class UXMLExporterWithDialog
{

    [MenuItem("PointLife-PPC-UiGen/Generate UI Code")]
    public static void ExportSelectedUXML()
    {
        // Find all project UXML files
        string[] guids = AssetDatabase.FindAssets("t:VisualTreeAsset");
        List<string> projectUXMLPaths = new List<string>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.StartsWith("Assets/") && !path.StartsWith("Assets/ExportExample")) // Ignore Libaries and BP Export
            {
                projectUXMLPaths.Add(path);
            }
        }

        // If no UXML files found, notify the user
        if (projectUXMLPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("No UXML Files Found", "There are no UXML files in your project.", "OK");
            return;
        }

        UXMLSelectionWindow.ShowWindow(projectUXMLPaths);
    }
}

public class UXMLSelectionWindow : EditorWindow
{
    private static List<string> uxmlPaths;
    private VisualElement m_PreviewPane;
    private int m_SelectedIndex;
    private VisualElement m_InfoPane;

    public TemplateContainer PreviewRender { get; private set; }
    public Button ToggleTextNameButton { get; private set; }
    public Ui_ToolKit_Reader CurrentUi_ToolKit_Reader { get; private set; }

    public static void ShowWindow(List<string> paths)
    {
        uxmlPaths = paths;
        UXMLSelectionWindow window = GetWindow<UXMLSelectionWindow>("Select UXML Files");
        window.minSize = new Vector2(650, 300);
    }

    public void CreateGUI()
    {

        var allObjects = new List<VisualTreeAsset>();
        if (uxmlPaths == null)
        {
            Close(); return;
        }
        foreach (var path in uxmlPaths)
        {
            allObjects.Add(AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path));
        }


        var spitView3 = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Vertical);
        rootVisualElement.Add(spitView3);

        ExportLog = new ScrollView();

        var ExportSettings = new VisualElement();
        
        ExportSettings.Add(new Label("Export Settings"));
        var textField = new TextField("Output Path");
        textField.value = outputPath;
        textField.isReadOnly = true;
        ExportSettings.Add(textField);
        ExportSettings.Add(new Button(() => {
            GUI.FocusControl(null); // Else Text field won't be updated
            var newPath = EditorUtility.OpenFolderPanel("Bundle Folder", outputPath, string.Empty);
            if (!string.IsNullOrEmpty(newPath))
            {
                outputPath = newPath;
                textField.value = outputPath;
            }
        }) { text = "Browse" });

        ExportSettings.Add(new Button(BuildFiles) { text = "Build and Export Files" });

        ExportSettings.Add(ExportLog);
        spitView3.Add(ExportSettings);

        var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
        spitView3.Add(splitView);

        var leftPane = new ListView();
        splitView.Add(leftPane);
        m_PreviewPane = new VisualElement();

        var spitView2 = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Vertical);


        spitView2.Add(m_PreviewPane);
        m_InfoPane = new ScrollView();
        spitView2.Add(m_InfoPane);

        splitView.Add(spitView2);

        // Initialize the list view with all sprites' names.
        leftPane.makeItem = () => new Label();
        leftPane.bindItem = (item, index) => { (item as Label).text = allObjects[index].name; };
        leftPane.itemsSource = allObjects;

        // React to the user's selection.
        leftPane.selectionChanged += OnUiSelectionChanged;

        // Restore the selection index from before the hot reload.
        leftPane.selectedIndex = m_SelectedIndex;

        // Store the selection index when the selection changes.
        leftPane.selectionChanged += (items) => { m_SelectedIndex = leftPane.selectedIndex; };

    }

    private void BuildFiles()
    {
        ExportLog.Clear();
        ExportLog.Add(new Label($"Starting Export"));


        if (string.IsNullOrWhiteSpace(outputPath))
        {
            EditorUtility.DisplayDialog("No Output Path", "Please select an output path", "OK");
            return;
        }
        foreach (var path in uxmlPaths)
        {
            var bundle = AssetDatabase.GetImplicitAssetBundleName(path);
            if (string.IsNullOrWhiteSpace(bundle))
            {
               // ExportLog.Add(new Label($"No asset bundle for {path}"));
                Debug.LogWarning($"No asset bundle for {path}");
                continue;
            }

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
            var reader = new Ui_ToolKit_Reader(path);
            var data = reader.ProcessUIElements();

            if (!data.Success)
            {
                ExportLog.Add(new Label($"Error in {path}"));
                Debug.LogError($"Error in {path} of Asset Bundle {bundle}");
                continue;
            }

            var outputDir = Path.Combine(this.outputPath, bundle);
            Directory.CreateDirectory(outputDir);
            var outputPath = Path.Combine(outputDir, uxml.name + ".cs");

            ExportLog.Add(new Label($"Exported {bundle}/{uxml.name}"));


            File.WriteAllText(outputPath, data.GeneratedCode.Trim());
        }

        ExportLog.Add(new Label($"Done"));

        

    }

    private void OnUiSelectionChanged(IEnumerable<object> selectedItems)
    {

        var enumerator = selectedItems.GetEnumerator();
        if (enumerator.MoveNext())
        {
            RefreshPreview(enumerator.Current as VisualTreeAsset);
        }
    }

    private void RefreshPreview(VisualTreeAsset selectedUI)
    {
        // Clear all previous content from the pane.
        m_PreviewPane.Clear();
        if (selectedUI != null)
        {
            // Get Path
            var i = 0;
            while (uxmlPaths[i].Split('/').Last() != selectedUI.name + ".uxml")
            {
                i++;
            }
            var path = uxmlPaths[i];

            // Refresh Changes in Case Suggestion was accepted
            AssetDatabase.Refresh();
            selectedUI = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);

            m_InfoPane.Clear();
            m_PreviewPane.Clear();

            PreviewRender = selectedUI.Instantiate();
            PreviewRender.StretchToParentSize();
            m_PreviewPane.Add(PreviewRender);

            ToggleTextNameButton = new Button(ToggleTextName) { text = "DISABLE TEXT" };

            m_InfoPane.Add(ToggleTextNameButton);

            m_InfoPane.Add(new Label($"UI Name: {selectedUI.name}"));

            m_InfoPane.Add(new Label($"UI Path: {path}"));

            m_InfoPane.Add(new Label($"Asset Bundle: {AssetDatabase.GetImplicitAssetBundleName(path)}"));

            CurrentUi_ToolKit_Reader = new Ui_ToolKit_Reader(path);
            var data = CurrentUi_ToolKit_Reader.ProcessUIElements();

            m_InfoPane.Add(new Label("=== ISSUES =="));

            foreach (var issue in data.Issues)
            {
                var suggestion = new TextField(issue.Original);
                suggestion.value = issue.Suggestion;
                m_InfoPane.Add(suggestion);


                m_InfoPane.Add(new Button(() =>
                {

                    Debug.Log($"Changing {issue.Original} to {suggestion.value}");
                    CurrentUi_ToolKit_Reader.ChangeElementName(issue.Original, suggestion.value, issue.IssueType);

                    RefreshPreview(selectedUI);
                })
                { text = "Change" });

            }

            m_InfoPane.Add(new Label("=== LOG =="));

            m_InfoPane.Add(new Label(data.Log));

            m_InfoPane.Add(new Label("=== CODE =="));

            m_InfoPane.Add(new Label(data.GeneratedCode.Trim()));

            if (!TextNameEnabled)
            {
                TextNameEnabled = true;
                ToggleTextName();
            }

        }
    }

    private bool TextNameEnabled = true;

    private string GetProjectPath => Path.GetFullPath(".");

    private string GetDataPath => Path.Combine(GetProjectPath, "Library/PointLife-PPC-UiGen.dat");

    public VisualElement ExportLog { get; private set; }

    private readonly BinaryFormatter bf = new();

    private void OnEnable()
    {
        try
        {
            using var file = File.Open(GetDataPath, FileMode.Open);
            outputPath = bf.Deserialize(file) as string;
        }
        catch (Exception) { }
    }

    public void OnDisable()
    {

        try
        {
            using var file = File.Create(GetDataPath);
            bf.Serialize(file, outputPath);
        }
        catch (Exception) { }
    }

    [SerializeField]
    private string outputPath = "./Generated_Scaffold";

    private void ToggleTextName()
    {
        if (TextNameEnabled)
        {
            var labels = PreviewRender.Query<TextElement>().Build();
            labels.ForEach(label =>
            {
                label.viewDataKey = label.text;
                if (string.IsNullOrWhiteSpace(label.name))
                {
                    var d = PreviewRender.Children().First().name;
                    if (string.IsNullOrWhiteSpace(label.parent.name) || label.parent.name == PreviewRender.Children().First().name)
                    {
                        label.text = "## NO NAME ##";
                        label.style.color = Color.red;
                    }
                    else
                    {
                        label.text = label.parent.name;
                    }
                }
                else
                {
                    label.text = label.name;
                }

                label.style.overflow = Overflow.Visible;
            });

        }
        else
        {
            var labels = PreviewRender.Query<TextElement>().Build();
            labels.ForEach(label =>
            {
                label.text = label.viewDataKey;
            });
           
        }
        TextNameEnabled = !TextNameEnabled;
        ToggleTextNameButton.text = TextNameEnabled ? "DISABLE TEXT" : "ENABLE TEXT";
    }
}
