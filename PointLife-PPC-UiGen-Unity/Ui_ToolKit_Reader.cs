using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using UnityEngine.Assertions;
using Mono.TextTemplating;
using Microsoft.VisualStudio.TextTemplating;
using Codice.Client.BaseCommands;
using Codice.CM.Common.Matcher;
using UnityEditor;
using UnityEngine;

public class Ui_ToolKit_Reader
{
    public XmlDocument XmlDocument { get; set; }
    public string FilePath { get; private set; }

    private UIElementsInfo UiElementData;

    public class ReturnElement
    {
        public string GeneratedCode;
        public string Log;
        public List<Issue> Issues;

        public bool Success;
    }

    public class Issue
    {
        public string Original;
        public string Suggestion;
        public IssueType IssueType;
    }

    public Ui_ToolKit_Reader(string filePath)
    {
        FilePath = filePath;
        XmlDocument = new XmlDocument();
    }

    public ReturnElement ProcessUIElements()
    {
        var Log = new StringBuilder();
        var Issues = new List<Issue>();

        UiElementData = ParseUXMLDocument(File.ReadAllText(FilePath));
        //Console.WriteLine(JsonConvert.SerializeObject(uiElementsInfo, Newtonsoft.Json.Formatting.Indented));

        var fullName = Path.GetFileName(FilePath).Replace(".uxml", ""); ;
        var split = new string[3] {"ERROR-NULL-ERROR", "ERROR-NULL-ERROR", "ERROR-NULL-ERROR" };

        var split_x = fullName.Split('_');
    
        split_x.CopyTo(split, 0);

        var menuName = split[2];
        var folder = split[1];
        var plugin = split[0];
        var fullName_new = $"{plugin}_{folder}_{menuName}";

        if (fullName_new != fullName)
        {
            var Issue = new Issue();
            Issue.Original = fullName;
            Issue.Suggestion = fullName_new;
            Issue.IssueType = IssueType.FileName;
            Issues.Add(Issue);

            Log.AppendLine($"File Name Issue!");
            Log.AppendLine($"File name needs to be in Format PLUGIN_FOLDER_MENUNAME");

            return new ReturnElement() { GeneratedCode = "##    MAJOR ERROR    ##", Log = Log.ToString(), Issues = Issues, Success = false };
        }


        foreach (var elm in UiElementData.ButtonInfos)
        {
            if (!elm.ElementName.StartsWith(fullName))
            {
                var Issue = new Issue()
                {
                    Original = elm.ElementName,
                    Suggestion = fullName + "_" + elm.ElementName.Split('_').Last(),
                    IssueType = IssueType.Button
                };
                Log.AppendLine($"{elm.ElementName} has wrong name!");
                Issues.Add(Issue);
            }
        }

        foreach (var elm in UiElementData.LabelInfos)
        {
            if (!elm.ElementName.StartsWith(fullName))
            {
                var Issue = new Issue()
                {
                    Original = elm.ElementName,
                    Suggestion = fullName + "_" + elm.ElementName.Split('_').Last(),
                    IssueType = IssueType.Label
                };
                Log.AppendLine($"{elm.ElementName} has wrong name!");
                Issues.Add(Issue);
            }
        }

        foreach (var elm in UiElementData.TextFieldInfos)
        {
            if (!elm.ElementName.StartsWith(fullName))
            {
                var Issue = new Issue()
                {
                    Original = elm.ElementName,
                    Suggestion = fullName + "_" + elm.ElementName.Split('_').Last(),
                    IssueType = IssueType.TextField
                };
                Log.AppendLine($"{elm.ElementName} has wrong name!");
                Issues.Add(Issue);
            }
        }


        foreach (var elm in UiElementData.ToggleInfos)
        {
            if (!elm.ElementName.StartsWith(fullName))
            {
                var Issue = new Issue()
                {
                    Original = elm.ElementName,
                    Suggestion = fullName + "_" + elm.ElementName.Split('_').Last(),
                    IssueType = IssueType.Toggle
                };
                Log.AppendLine($"{elm.ElementName} has wrong name!");
                Issues.Add(Issue);
            }
        }

        foreach (var elm in UiElementData.ProgressBarInfos)
        {
            if (!elm.ElementName.StartsWith(fullName))
            {
                var Issue = new Issue()
                {
                    Original = elm.ElementName,
                    Suggestion = fullName + "_" + elm.ElementName.Split('_').Last(),
                    IssueType = IssueType.ProgressBar
                };
                Log.AppendLine($"{elm.ElementName} has wrong name!");
                Issues.Add(Issue);
            }
        }

        if (UiElementData.Menu.ElementName != fullName)
        {
            var Issue = new Issue()
            {
                Original = UiElementData.Menu.ElementName,
                Suggestion = fullName,
                IssueType = IssueType.MainElement
            };
            Log.AppendLine($"Main Element {UiElementData.Menu.ElementName} has wrong name!");
            Issues.Add(Issue);
        }

        var template = Activator.CreateInstance<Scaffold.Generator>();
        var session = new TextTemplatingSession();

        session["MenuName"] = UiElementData.Menu.ElementName;
        session["MenuFullName"] = $"{UiElementData.Menu.ElementName}";  // Todo write this to file before bpa

        session["ButtonInfos"] = UiElementData.ButtonInfos.Select(x => x.ElementName).ToList();

        session["TextFieldInfos"] = UiElementData.TextFieldInfos.Select(x => x.ElementName).ToList();
        session["LabelInfos"] = UiElementData.LabelInfos.Select(x => x.ElementName).ToList();
        session["ToggleInfos"] = UiElementData.ToggleInfos.Select(x => x.ElementName).ToList();
        session["ProgressBarInfos"] = UiElementData.ProgressBarInfos.Select(x => x.ElementName).ToList();

        session["MenuFilePath"] = FilePath;
        session["UnityVersion"] = Application.unityVersion;

        template.Session = session;

        template.Initialize();

        String pageContent = template.TransformText();

        return new ReturnElement() { GeneratedCode = pageContent.Trim(), Log = Log.ToString(), Issues = Issues, Success = Issues.Count == 0};
    }

    public void ChangeElementName(string oldName, string newName, IssueType issueType)
    {

        switch (issueType)
        {
            case IssueType.FileName:
                EditorUtility.DisplayDialog("File Name", "File Name needs to be changed manually.\nPlease change it by renaming the File in Unity.", "OK");
                break;
            case IssueType.MainElement:
                UiElementData.Menu.ElementName = newName;
                break;
            case IssueType.Button:
                UiElementData.ButtonInfos.First(x => x.ElementName == oldName).ElementName = newName;
                break;
            case IssueType.Label:
                UiElementData.LabelInfos.First(x => x.ElementName == oldName).ElementName = newName;
                break;
            case IssueType.Toggle:
                UiElementData.ToggleInfos.First(x => x.ElementName == oldName).ElementName = newName;
                break;
            case IssueType.TextField:
                UiElementData.TextFieldInfos.First(x => x.ElementName == oldName).ElementName = newName;
                break;
            case IssueType.ProgressBar:
                UiElementData.ProgressBarInfos.First(x => x.ElementName == oldName).ElementName = newName;
                break;
            default:
                break;
        }

        XmlDocument.Save(FilePath);
    }


    private UIElementsInfo ParseUXMLDocument(string uxmlDocument)
    {

        XmlDocument.LoadXml(uxmlDocument);

        var namespaceManager = new XmlNamespaceManager(XmlDocument.NameTable);
        namespaceManager.AddNamespace("ui", "UnityEngine.UIElements");

        var menu = new ElementInfo { XmlNode = XmlDocument.SelectSingleNode("//ui:VisualElement", namespaceManager) };

        var buttonInfos = new List<ElementInfo>();
        var buttonNodes = XmlDocument.SelectNodes("//ui:Button", namespaceManager);
        foreach (XmlNode buttonNode in buttonNodes)
        {
            buttonInfos.Add(new ElementInfo { XmlNode = buttonNode });
        }

        var textFieldInfos = new List<ElementInfo>();
        var textFieldNodes = XmlDocument.SelectNodes("//ui:TextField", namespaceManager);
        foreach (XmlNode textFieldNode in textFieldNodes)
        {
            textFieldInfos.Add(new ElementInfo { XmlNode = textFieldNode });
        }


        var labelInfos = new List<ElementInfo>();
        var labelNodes = XmlDocument.SelectNodes("//ui:Label", namespaceManager);
        foreach (XmlNode labelNode in labelNodes)
        {
            if (labelNode.Attributes["name"] == null)
            {
                Console.Write("Warning: Skipping Label due to no name!");
                continue;
            }


            var x = new ElementInfo { XmlNode = labelNode };
            labelInfos.Add(x);
        }

        var toggleInfos = new List<ElementInfo>();
        var toggleNodes = XmlDocument.SelectNodes("//ui:Toggle", namespaceManager);
        foreach (XmlNode toggleNode in toggleNodes)
        {
            var x = new ElementInfo { XmlNode = toggleNode };
            toggleInfos.Add(x);
        }

        var progressBarInfos = new List<ElementInfo>();
        var progressBarNodes = XmlDocument.SelectNodes("//ui:ProgressBar", namespaceManager);
        foreach (XmlNode elm in progressBarNodes)
        {
            var x = new ElementInfo { XmlNode = elm };
            progressBarInfos.Add(x);
        }

        return new UIElementsInfo
        {
            Menu = menu,
            ButtonInfos = buttonInfos,
            TextFieldInfos = textFieldInfos,
            LabelInfos = labelInfos,
            ToggleInfos = toggleInfos,
            ProgressBarInfos = progressBarInfos
        };

    }
}

public enum IssueType
{
    FileName,
    MainElement,
    Button,
    Label,
    TextField,
    Toggle,
    ProgressBar,
}

class UIElementsInfo
{
    public ElementInfo Menu { get; set; }

    public List<ElementInfo> ButtonInfos { get; set; }
    public List<ElementInfo> TextFieldInfos { get; set; }
    public List<ElementInfo> LabelInfos { get; set; }
    public List<ElementInfo> ToggleInfos { get; set; }
    public List<ElementInfo> ProgressBarInfos { get; internal set; }
}

class ElementInfo
{
    public string ElementName
    {
        get
        {
            return XmlNode?.Attributes["name"]?.Value ?? "##NULL###ERROR##";
        }
        set
        {
            var nameAttribute = XmlNode.Attributes["name"];
            if (nameAttribute != null)
            {
                nameAttribute.Value = value;
            }
            else
            {
                XmlNode.Attributes.Append(XmlNode.OwnerDocument.CreateAttribute("name"));
                XmlNode.Attributes["name"].Value = value;
            }
        }
    }

    public XmlNode XmlNode { get; set; }
}
