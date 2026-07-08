using ANF.GUI;
using ANF.Utils;
using UnityEngine;

namespace ANF.Scene
{
    [CreateAssetMenu(fileName = "ANFSceneData", menuName = "ANF/SceneData")]
    public class ANFSceneData : ScriptableObject
    {
        [Tooltip("World components are responsible for individual features. (Ex: Background Manager, ANSL Manager)")]
        public ComponentRegisterEntry<WorldComponent>[] registeredWorldComponents;
        [Tooltip("GUI components are responsible for drawing things on screen. (Ex : Pause menu, Fade, Dialogs, ...)")]
        public GUIRegisterEntry<GUIComponent>[] registeredGUIComponents;

        [Tooltip("True if scene change should use a Fade Component")]
        public bool changeSceneUseFading = true;
        [Tooltip("The Fade Component's ID (if used)")]
        public string changeSceneFadingName = "fadeAll";
    }

}
