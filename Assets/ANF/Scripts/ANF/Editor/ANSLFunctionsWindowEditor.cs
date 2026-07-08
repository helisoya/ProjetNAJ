using ANF.Persistent;
using ANF.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using AYellowpaper.SerializedCollections;

/// <summary>
/// Represents the editor for the ANSL registered functions
/// </summary>
public class ANSLFunctionsWindowEditor : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("ANF/ANSL/Registered Functions")]
    public static void Open()
    {
        ANSLFunctionsWindowEditor wnd = GetWindow<ANSLFunctionsWindowEditor>();
        wnd.titleContent = new GUIContent("Registered ANSL Functions");
    }

    public void OnGUI()
    {
        ANFSettings settings = AssetDatabase.LoadAssetAtPath<ANFSettings>("Assets/Settings/ANF/ANFSettings.asset");

        if(!settings.FindAdditionalPart(out ANSLSettings anslSettings))
        {
            GUILayout.Label("No ANSLSettings detected in the ANFSettings. Did you forget to register it ?");
        }
        else
        {
            if(anslSettings.registeredFunctions == null)
                anslSettings.registeredFunctions = new List<ANSLSettings.ANSLFunctionSettingsData>();

            List<ANSLSettings.ANSLFunctionSettingsData> knownFunctions = anslSettings.registeredFunctions;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (knownFunctions.Count == 0)
            {
                GUILayout.Label("No functions found.");
            }
            else
            {
                float size = position.width / 3.0f;
                GUILayoutOption[] options = { GUILayout.Width(size) };

                GUILayout.BeginHorizontal();
                GUILayout.Label("Function name", options);
                GUILayout.Label("Id", options);
                GUILayout.Label("Active", options);
                GUILayout.EndHorizontal();

                for(int i = 0; i < knownFunctions.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(knownFunctions[i].typeName, options);
                    GUILayout.Label(knownFunctions[i].id.ToString(), options);

                    bool shouldEnable = GUILayout.Toggle(knownFunctions[i].enabled, "", options);
                    if(shouldEnable != knownFunctions[i].enabled)
                    {
                        knownFunctions[i] = new ANSLSettings.ANSLFunctionSettingsData() { 
                            enabled = shouldEnable, 
                            id = knownFunctions[i].id, 
                            typeName = knownFunctions[i].typeName 
                        };
                    }

                    GUILayout.EndHorizontal();
                    EditorGUILayout.Separator();

                    Rect rect = EditorGUILayout.GetControlRect(false, 1);
                    rect.height = 1;
                    EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
                    EditorGUILayout.Space(1);
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Toggle All"))
            {
                if(knownFunctions.Count > 0)
                {
                    bool targetEnabled = !knownFunctions[0].enabled;

                    for (int i = 0; i < knownFunctions.Count; i++)
                    {
                        knownFunctions[i] = new ANSLSettings.ANSLFunctionSettingsData()
                        {
                            enabled = targetEnabled,
                            id = knownFunctions[i].id,
                            typeName = knownFunctions[i].typeName
                        };
                    }
                }
            }
            if (GUILayout.Button("Refresh Functions"))
            {
                List<Type> types = ANSLUtils.GetANSLFunctionsList();
                Dictionary<string, bool> activeCache = new Dictionary<string, bool>();

                for (int i = 0; i < knownFunctions.Count; i++)
                {
                    if (!types.Contains(Type.GetType(knownFunctions[i].typeName)))
                    {
                        knownFunctions.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        activeCache.Add(knownFunctions[i].typeName, knownFunctions[i].enabled);
                    }
                }

                knownFunctions.Clear();

                uint id = 0;
                foreach(Type type in types)
                {
                    if(activeCache.ContainsKey(type.FullName))
                        knownFunctions.Add(new ANSLSettings.ANSLFunctionSettingsData() { enabled = activeCache[type.FullName], id = id, typeName = type.FullName });
                    else
                        knownFunctions.Add(new ANSLSettings.ANSLFunctionSettingsData() { enabled = true, id = id, typeName = type.FullName });
                    id++;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            EditorUtility.SetDirty(settings);
        }
    }
}
