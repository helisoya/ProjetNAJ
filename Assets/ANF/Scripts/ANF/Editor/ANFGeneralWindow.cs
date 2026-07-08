using ANF.Persistent;
using ANF.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ANF.Editor
{

    /// <summary>
    /// Represents the Base ANF Window. Used to edit settings and compile ANSL functions
    /// </summary>
    public class ANFGeneralWindow : EditorWindow
    {
        private ANFSettings settings;
        private UnityEditor.Editor settingsEditor;

        private bool settingsOpen = true;
        private bool anslOpen = true;
        private Vector2 scrollPosition = Vector2.zero;

        [MenuItem("ANF/General")]
        public static void OpenWindow()
        {
            ANFGeneralWindow wnd = GetWindow<ANFGeneralWindow>();
            wnd.titleContent = new GUIContent("ANF");
        }

        ANFGeneralWindow()
        {
        }

        private void OnEnable()
        {
            settings = AssetDatabase.LoadAssetAtPath<ANFSettings>("Assets/Settings/ANF/ANFSettings.asset");
            settingsEditor = UnityEditor.Editor.CreateEditor(settings);
        }



        public void OnGUI()
        {
            if (settings == null)
                return;

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            settingsOpen = EditorGUILayout.Foldout(settingsOpen, "Settings");
            if (settingsOpen)
            {
                settingsEditor.OnInspectorGUI();
            }

            EditorGUILayout.Separator();

            anslOpen = EditorGUILayout.Foldout(anslOpen, "ANSL");
            if (anslOpen)
            {
                if (GUILayout.Button("Regenerate VS Code Snippets"))
                {
                    RegenerateVSCodeSnippets();
                }

                if (GUILayout.Button("Compile ANSL Files"))
                {
                    CompileANSLFiles();
                }
            }
            EditorGUILayout.EndScrollView();
        }


        [MenuItem("ANF/ANSL/Regenerate VS Code Snippets")]
        private static void RegenerateVSCodeSnippets()
        {
            ANFSettings settings = AssetDatabase.LoadAssetAtPath<ANFSettings>("Assets/Settings/ANF/ANFSettings.asset");
            if(settings != null)
                ANSLUtils.RegenerateVSCodeSnippets(settings);
        }

        [MenuItem("ANF/ANSL/Compile ANSL Files")]
        private static void CompileANSLFiles()
        {
            ANFSettings settings = AssetDatabase.LoadAssetAtPath<ANFSettings>("Assets/Settings/ANF/ANFSettings.asset");

            if (settings != null)
            {
                List<ANSLUtils.ANSLError> errors = ANSLUtils.CompileAll(settings);
                ANSLErrorListPopup.Show(errors);
                AssetDatabase.Refresh();
            }
        }
    }
}
