using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DataCreator_Window : EditorWindow
{
    [SerializeField] Data data = new Data();
    private Vector2 scrollPosition = Vector2.zero;

    SerializedObject serializedObject = null;
    SerializedProperty questionsProp = null;

    string path;

    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        data.Questions = new Question[0];
        questionsProp = serializedObject.FindProperty("data").FindPropertyRelative("Questions");
    }

    [MenuItem("Game/Data creator")]
    public static void OpenWindow()    
    {
        var window = EditorWindow.GetWindow<DataCreator_Window>("Creator");

        window.minSize = new Vector2(510.0f, 344.0f);
        window.Show();
    }

    private void OnGUI()
    {
        #region Header Section

        Rect headerRect = new Rect(15, 15, this.position.width - 30, 65);
        GUI.Box(headerRect, GUIContent.none);

        GUIStyle headerStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontSize = 26,
            alignment = TextAnchor.UpperLeft
        };
        headerRect.x += 5;
        headerRect.width -= 10;
        headerRect.y += 5;
        headerRect.height -= 10;
        GUI.Label(headerRect, "Data to XML Creator", headerStyle);

        Rect summaryRect = new Rect(headerRect.x + 25, (headerRect.y + headerRect.height) - 20, headerRect.width - 50, 15);
        GUI.Label(summaryRect, "Create the data that needs to be included into the XML file");

        #endregion
    
        #region Body Section

        Rect bodyRect = new Rect(15, (headerRect.y + headerRect.height) + 20, this.position.width -30, this.position.height - (headerRect.y + headerRect.height) -80);
        GUI.Box(bodyRect, GUIContent.none);

        var arraySize = data.Questions.Length;

        Rect viewRect = new Rect(bodyRect.x +10, bodyRect.y +10, bodyRect.width -20, EditorGUI.GetPropertyHeight(questionsProp));
        Rect scrollPosRect = new Rect(viewRect)
        {
            height = bodyRect.height - 20
        };

        scrollPosition = GUI.BeginScrollView(scrollPosRect, scrollPosition, viewRect, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

        var drawSlider = viewRect.height > scrollPosRect.height;

        Rect propertyRect = new Rect(bodyRect.x + 10, bodyRect.y + 10, bodyRect.width - (drawSlider ? 40 : 20), 17);
        EditorGUI.PropertyField(propertyRect, questionsProp, true);

        serializedObject.ApplyModifiedProperties();

        GUI.EndScrollView();
        #endregion
    
        #region Navigation

        Rect ButtonRect = new Rect(bodyRect.x + bodyRect.width - 85, bodyRect.height + 115, 85, 30);
        bool pressed = GUI.Button(ButtonRect, "Create", EditorStyles.miniButtonLeft);
        if (pressed)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.SaveFilePanel("Save", "Assets", GameUtility.FileName, "xml");
            }
            Data.Write(data, path);
        }
        ButtonRect.x -= ButtonRect.width;
        pressed = GUI.Button(ButtonRect, "Fetch", EditorStyles.miniButtonLeft);
        if (pressed)
        {
            var path = EditorUtility.OpenFilePanel("Select", "Assets", "xml");
            if (string.IsNullOrEmpty(path) == false)
            {
                var d = Data.Fetch(out bool result, path);
                if (result)
                {
                    data = d;
                }

                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();

            }
        }


        #endregion

    }

}
