using ANF.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ANF.Editor
{

    /// <summary>
    /// Represents a popup for the ANSL Errors
    /// </summary>
    public class ANSLErrorListPopup : EditorWindow
    {

        private ANSLUtils.ANSLCompileStats data;

        private Vector2 scrollPosition;

        /// <summary>
        /// Initialize the popup
        /// </summary>
        /// <param name="data">The ANSL error's data</param>
        public void Init(ANSLUtils.ANSLCompileStats data)
        {
            this.data = data;
            this.scrollPosition = Vector2.zero;
        }

        /// <summary>
        /// Shows the popup
        /// </summary>
        /// <param name="data">The error's data</param>
        public static void Show(ANSLUtils.ANSLCompileStats data)
        {
            ANSLErrorListPopup wnd = GetWindow<ANSLErrorListPopup>();
            wnd.titleContent = new GUIContent("Errors");
            wnd.minSize = new Vector2(600, 300);
            wnd.maxSize = new Vector2(750, 600);
            wnd.Init(data);
        }

        public void OnGUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"Compiled : {data.compilationGood}, Failed : {data.compilationFailed}, Skipped : {data.compilationSkipped}");

            GUILayout.EndHorizontal();

            if (data.errors == null || data.errors.Count == 0)
            {
                GUILayout.Label("No errors detected");
            }
            else
            {
                GUILayoutOption[] options2 = { GUILayout.Width(50) };
                GUILayoutOption[] options = { GUILayout.Width(250) };
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);
                for (int i = 0; i < data.errors.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(data.errors[i].type.ToString(), options);
                    if (data.errors[i].type == ANSLUtils.ANSLErrorType.FUNCTION)
                    {
                        GUILayout.Label(data.errors[i].filePath, EditorStyles.wordWrappedLabel, options);
                    }
                    else
                    {
                        GUILayout.Label($"{data.errors[i].filePath}, {data.errors[i].line}", EditorStyles.wordWrappedLabel, options);
                    }

                    GUILayout.Label(data.errors[i].errorMessage, EditorStyles.wordWrappedLabel, options);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }
        }
    }
}
