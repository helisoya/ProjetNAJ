using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ANF.Editor
{
    /// <summary>
    /// Represents the editor of the Character class
    /// </summary>
    [CustomEditor(typeof(ANF.Scene.Character))]
    public class CharacterEditor : UnityEditor.Editor
    {
        private bool foldoutBody = true;
        private bool foldoutEye = true;
        private bool foldoutMouth = true;

        private bool foldoutAddBody = false;
        private AnimationClip bodyStateClip1 = null;
        private AnimationClip bodyStateClip2 = null;
        private string bodyStateName = "NewBodyState";

        private bool foldoutAddEye = false;
        private Sprite eyeStateTexture = null;
        private AnimationClip eyeStateClip1 = null;
        private AnimationClip eyeStateClip2 = null;
        private string eyeStateName = "NewEyeState";

        private bool foldoutAddMouth = false;
        private Sprite mouthStateTexture = null;
        private AnimationClip mouthStateClip1 = null;
        private AnimationClip mouthStateClip2 = null;
        private string mouthStateName = "NewMouthState";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(50);

            ANF.Scene.Character character = target.GetComponent<ANF.Scene.Character>();
            string pathToAnimations = "Assets/Resources/Animations/Characters/" + character.GetCharacterName() + "/";

            if (character.GetAnimator().runtimeAnimatorController &&
                character.GetAnimator().runtimeAnimatorController is AnimatorController)
            {
                AnimatorController animator = target.GetComponent<ANF.Scene.Character>().GetAnimator().runtimeAnimatorController as AnimatorController;
                DrawBodyAnimations(animator, pathToAnimations, 0);
                GUILayout.Space(15);
                DrawEyeAnimations(animator, pathToAnimations, 1);
                GUILayout.Space(15);
                DrawMouthAnimations(animator, pathToAnimations, 2);
            }
            else
            {
                if (GUILayout.Button("Create Controller"))
                {
                    CreateDefaultController(character, pathToAnimations);
                }
            }

            serializedObject.Update();
        }

        /// <summary>
        /// Creates a default animation controller for this character
        /// </summary>
        /// <param name="character">The character</param>
        public static void CreateDefaultController(ANF.Scene.Character character, string animationFolder)
        {
            string path = animationFolder + character.GetCharacterName() + ".controller";
            if (AssetDatabase.AssetPathExists(path))
                AssetDatabase.DeleteAsset(path);

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(animationFolder + character.GetCharacterName() + ".controller");

            controller.AddParameter("Talking", AnimatorControllerParameterType.Float);
            controller.RemoveLayer(0);

            string[] layerNames = { "Body", "Eye", "Mouth" };

            foreach (string layerName in layerNames)
            {
                controller.AddLayer(new AnimatorControllerLayer
                {
                    name = layerName,
                    defaultWeight = 1f,
                    stateMachine = new AnimatorStateMachine()
                });
            }

            AssetDatabase.AddObjectToAsset(controller.layers[0].stateMachine, AssetDatabase.GetAssetPath(controller));
            AssetDatabase.AddObjectToAsset(controller.layers[1].stateMachine, AssetDatabase.GetAssetPath(controller));
            AssetDatabase.AddObjectToAsset(controller.layers[2].stateMachine, AssetDatabase.GetAssetPath(controller));

            character.GetAnimator().runtimeAnimatorController = controller;
            EditorUtility.SetDirty(character.GetAnimator());
            EditorUtility.SetDirty(controller);

            AssetDatabase.SaveAssetIfDirty(controller);
        }

        /// <summary>
        /// Opens the animator window to a new clip
        /// </summary>
        /// <param name="clip">The clip to show</param>
        private void OpenAnimatorWindow(AnimationClip clip)
        {
            AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
            if (!window)
                window = EditorWindow.CreateWindow<AnimationWindow>();

            window.animationClip = clip;
        }

        /// <summary>
        /// Draws a state machine
        /// </summary>
        /// <param name="animator">The linked animator controller</param>
        /// <param name="stateMachine">The state machine</param>
        private void DrawStateMachine(AnimatorController animator, AnimatorStateMachine stateMachine)
        {
            foreach (ChildAnimatorState state in stateMachine.states)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20f * 1f);
                GUILayout.Label(state.state.name);

                Motion motion = state.state.motion;
                List<AnimationClip> clips = new List<AnimationClip>();

                if (motion != null && motion is AnimationClip)
                {
                    clips.Add(motion as AnimationClip);
                }
                else if (motion != null && motion is BlendTree)
                {
                    BlendTree blendTree = motion as BlendTree;

                    foreach (ChildMotion childMotion in blendTree.children)
                        if (childMotion.motion is AnimationClip)
                            clips.Add(childMotion.motion as AnimationClip);
                }

                if (stateMachine.defaultState == state.state)
                {
                    GUILayout.Label("Default State");
                }
                else
                {
                    if (GUILayout.Button("Make Default"))
                    {
                        stateMachine.defaultState = state.state;
                    }
                }

                if (GUILayout.Button("Remove"))
                {
                    stateMachine.RemoveState(state.state);
                    AssetDatabase.SaveAssetIfDirty(animator);
                    GUILayout.EndHorizontal();
                    return;
                }

                GUILayout.EndHorizontal();

                foreach (AnimationClip clip in clips)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20f * 2f);
                    GUILayout.Label("->");
                    GUILayout.Label(clip.name);
                    if (GUILayout.Button("Show Clip"))
                    {
                        OpenAnimatorWindow(clip);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10f);
            }
        }

        /// <summary>
        /// Creates an empty state with an animation clip inside
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="clip">The linked clip (can be null)</param>
        /// <param name="createIfNull">True if a clip should be created if the provided one is null</param>
        public static void CreateFromAnimationClip(AnimatorController animator, AnimatorStateMachine stateMachine, string pathToAnimations, string newStateName, AnimationClip clip, bool createIfNull)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            if (clip == null && createIfNull)
            {
                clip = new AnimationClip();
                clip.name = newStateName;
                AssetDatabase.CreateAsset(clip, pathToAnimations + newStateName + ".anim");
            }

            AnimatorState createdState = stateMachine.AddState(newStateName);
            createdState.motion = clip;

            EditorUtility.SetDirty(createdState);
            EditorUtility.SetDirty(clip);
            EditorUtility.SetDirty(animator);
            AssetDatabase.SaveAssetIfDirty(clip);
            AssetDatabase.SaveAssetIfDirty(createdState);
            AssetDatabase.SaveAssetIfDirty(animator);
        }

        /// <summary>
        /// Creates a new state with an empty blend tree
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="layer">The blend tree's layer</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="clipNormal">The normal clip (can be null)</param>
        /// <param name="clipTalking">The talking clip (can be null)</param>
        /// <param name="createIfNull">True if clips should be created if null</param>
        public static void CreateFromBlendTree(AnimatorController animator, AnimatorStateMachine stateMachine, int layer, string pathToAnimations, string newStateName, AnimationClip clipNormal, AnimationClip clipTalking, bool createIfNull)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            if (clipNormal == null && createIfNull)
            {
                clipNormal = new AnimationClip();
                clipNormal.name = newStateName + "_Idle";
                AssetDatabase.CreateAsset(clipNormal, pathToAnimations + newStateName + "_Idle.anim");
            }


            if (clipTalking == null && createIfNull)
            {
                clipTalking = new AnimationClip();
                clipTalking.name = newStateName + "_Speak";
                AssetDatabase.CreateAsset(clipTalking, pathToAnimations + newStateName + "_Speak.anim");
            }

            animator.CreateBlendTreeInController(newStateName, out BlendTree blendTree, layer);
            blendTree.blendParameter = "Talking";
            blendTree.AddChild(clipNormal, 0);
            blendTree.AddChild(clipTalking, 1);

            if (clipNormal)
            {
                EditorUtility.SetDirty(clipNormal);
                AssetDatabase.SaveAssetIfDirty(clipNormal);
            }

            if (clipTalking)
            {
                EditorUtility.SetDirty(clipTalking);
                AssetDatabase.SaveAssetIfDirty(clipTalking);
            }

            EditorUtility.SetDirty(animator);
            AssetDatabase.SaveAssetIfDirty(animator);
        }

        /// <summary>
        /// Creates a new body animation using a template
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        public static void CreateBodyFromTemplate(AnimatorController animator, AnimatorStateMachine stateMachine, string pathToAnimations, string newStateName)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            // Copy Template Animations
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Body/Normal_Idle.anim", pathToAnimations + newStateName + "_Idle.anim");
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Body/Normal_Speak.anim", pathToAnimations + newStateName + "_Speak.anim");

            CreateFromBlendTree(animator, stateMachine, 0, pathToAnimations, newStateName,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Idle.anim"),
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Speak.anim"), false);
        }

        /// <summary>
        /// Creates a new eye animation using a template
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="eyeTexture">The eye texture</param>
        public static void CreateEyeFromTemplate(AnimatorController animator, AnimatorStateMachine stateMachine,
            string pathToAnimations, string newStateName, Sprite eyeTexture)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            // Copy Template Animations
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Eye/Normal_Idle.anim", pathToAnimations + newStateName + "_Idle.anim");


            // Change Sprites
            Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
            string templateName = eyeTexture.name.Substring(0, eyeTexture.name.Length - 2);

            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(eyeTexture));
            foreach (Object sprite in sprites)
            {
                if (sprite is Sprite && sprite.name.Substring(0, sprite.name.Length - 2).Equals(templateName))
                    dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite);
            }

            if (!dicSprites.ContainsKey('0') || !dicSprites.ContainsKey('1') || !dicSprites.ContainsKey('2'))
                return;

            Sprite defaultEyeSprite = dicSprites['0'];

            AnimationClip eyeNormalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Idle.anim");
            EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(eyeNormalClip)[0];
            ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(eyeNormalClip, binding);
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
            }
            AnimationUtility.SetObjectReferenceCurve(eyeNormalClip, binding, frames);

            AssetDatabase.SaveAssetIfDirty(eyeNormalClip);


            CreateFromAnimationClip(animator, stateMachine, pathToAnimations, newStateName, eyeNormalClip, false);
        }

        /// <summary>
        /// Creates a new mouth animation using a template
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="stateMachine">The state machine</param>
        /// <param name="pathToAnimations">The path to the animations</param>
        /// <param name="newStateName">The new state's name</param>
        /// <param name="mouthTexture">The mouth texture</param>
        public static void CreateMouthFromTemplate(AnimatorController animator, AnimatorStateMachine stateMachine,
            string pathToAnimations, string newStateName, Sprite mouthTexture)
        {
            if (string.IsNullOrEmpty(newStateName))
                return;

            foreach (ChildAnimatorState state in stateMachine.states)
                if (state.state.name.Equals(newStateName))
                    return;

            // Copy Template Animations
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Mouth/Normal_Idle.anim", pathToAnimations + newStateName + "_Idle.anim");
            AssetDatabase.CopyAsset("Assets/Settings/ANF/Templates/Animations/Mouth/Normal_Speak.anim", pathToAnimations + newStateName + "_Speak.anim");

            // Change Sprites
            Dictionary<char, Sprite> dicSprites = new Dictionary<char, Sprite>();
            string templateName = mouthTexture.name.Substring(0, mouthTexture.name.Length - 2);
            Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mouthTexture));

            foreach (Object sprite in sprites)
            {
                if (sprite is Sprite && sprite.name.Substring(0, sprite.name.Length - 2).Equals(templateName))
                    dicSprites.Add(sprite.name[sprite.name.Length - 1], sprite as Sprite);
            }

            if (!dicSprites.ContainsKey('0') || !dicSprites.ContainsKey('1') || !dicSprites.ContainsKey('2'))
                return;

            Sprite defaultMouthSprite = dicSprites['0'];

            string[] animNames = new string[] { newStateName + "_Idle", newStateName + "_Speak" };

            foreach (string animName in animNames)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + animName + ".anim");
                EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)[0];
                ObjectReferenceKeyframe[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                for (int i = 0; i < frames.Length; i++)
                {
                    frames[i].value = dicSprites[frames[i].value.name[frames[i].value.name.Length - 1]];
                }
                AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
                AssetDatabase.SaveAssetIfDirty(clip);
            }

            CreateFromBlendTree(animator, stateMachine, 2, pathToAnimations, newStateName,
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Idle.anim"),
                AssetDatabase.LoadAssetAtPath<AnimationClip>(pathToAnimations + newStateName + "_Speak.anim"), false);
        }

        /// <summary>
        /// Draws the body animations and the operations linked to them
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="pathToAnimations">The path to the body animations</param>
        /// <param name="layer">The layer in the animator</param>
        private void DrawBodyAnimations(AnimatorController animator, string pathToAnimations, int layer)
        {
            foldoutBody = EditorGUILayout.Foldout(foldoutBody, "Body Animations");
            if (foldoutBody)
            {
                if (animator.layers.Length <= layer)
                {
                    GUILayout.Label("No body layer");
                    return;
                }

                AnimatorStateMachine stateMachine = animator.layers[layer].stateMachine;
                DrawStateMachine(animator, stateMachine);

                GUILayout.Space(5);
                foldoutAddBody = EditorGUILayout.Foldout(foldoutAddBody, "Add new state");
                if (foldoutAddBody)
                {
                    bodyStateName = EditorGUILayout.TextField("State name", bodyStateName);

                    bodyStateClip1 = EditorGUILayout.ObjectField("Clip1", bodyStateClip1, typeof(AnimationClip), false) as AnimationClip;
                    bodyStateClip2 = EditorGUILayout.ObjectField("Clip2", bodyStateClip2, typeof(AnimationClip), false) as AnimationClip;

                    if (GUILayout.Button("Create New (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Body/", bodyStateName, null, true);
                    }

                    if (GUILayout.Button("Create New (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, layer, pathToAnimations + "Body/", bodyStateName, null, null, true);
                    }

                    if (GUILayout.Button("Create from clip (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Body/", bodyStateName, bodyStateClip1, false);
                    }

                    if (GUILayout.Button("Create from clips (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, layer, pathToAnimations + "Body/", bodyStateName, bodyStateClip1, bodyStateClip2, false);
                    }

                    if (GUILayout.Button("Create from template"))
                    {
                        CreateBodyFromTemplate(animator, stateMachine, pathToAnimations + "Body/", bodyStateName);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the eye animations and the operations linked to them
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="pathToAnimations">The path to the eye animations</param>
        /// <param name="layer">The layer in the animator</param>
        private void DrawEyeAnimations(AnimatorController animator, string pathToAnimations, int layer)
        {
            foldoutEye = EditorGUILayout.Foldout(foldoutEye, "Eye Animations");
            if (foldoutEye)
            {
                if (animator.layers.Length <= layer)
                {
                    GUILayout.Label("No eye layer");
                    return;
                }

                AnimatorStateMachine stateMachine = animator.layers[layer].stateMachine;
                DrawStateMachine(animator, stateMachine);

                GUILayout.Space(5);
                foldoutAddEye = EditorGUILayout.Foldout(foldoutAddEye, "Add new state");
                if (foldoutAddEye)
                {
                    GUILayout.Label("If creating from a template. The animation's sprites must : ");
                    GUILayout.Label("1) Be strictly in 3 parts. (3 sprites for one animation)");
                    GUILayout.Label("2) Have the same path. (Part of the same texture)");
                    GUILayout.Label("3) Have the same name, with a _0,_1,_2 attached at the end");
                    GUILayout.Label("The used sprites will be infered from the given sprite name.");
                    GUILayout.Label("Ex : Test_Eye_0, Test_Eye_1, Test_Eye_2");

                    eyeStateName = EditorGUILayout.TextField("State name", eyeStateName);

                    eyeStateClip1 = EditorGUILayout.ObjectField("Clip1", eyeStateClip1, typeof(AnimationClip), false) as AnimationClip;
                    eyeStateClip2 = EditorGUILayout.ObjectField("Clip2", eyeStateClip2, typeof(AnimationClip), false) as AnimationClip;
                    eyeStateTexture = EditorGUILayout.ObjectField("Sprite (for Template)", eyeStateTexture, typeof(Sprite), false) as Sprite;

                    if (GUILayout.Button("Create New (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Eye/", eyeStateName, null, true);
                    }

                    if (GUILayout.Button("Create New (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, layer, pathToAnimations + "Eye/", eyeStateName, null, null, true);
                    }

                    if (GUILayout.Button("Create from clip (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Eye/", eyeStateName, eyeStateClip1, false);
                    }

                    if (GUILayout.Button("Create from clips (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, layer, pathToAnimations + "Eye/", eyeStateName, eyeStateClip1, eyeStateClip2, false);
                    }

                    if (GUILayout.Button("Create from template"))
                    {
                        CreateEyeFromTemplate(animator, stateMachine, pathToAnimations + "Eye/", eyeStateName, eyeStateTexture);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the mouth animations and the operations linked to them
        /// </summary>
        /// <param name="animator">The animator</param>
        /// <param name="pathToAnimations">The path to the mouth animations</param>
        /// <param name="layer">The layer in the animator</param>
        private void DrawMouthAnimations(AnimatorController animator, string pathToAnimations, int layer)
        {
            foldoutMouth = EditorGUILayout.Foldout(foldoutMouth, "Mouth Animations");
            if (foldoutMouth)
            {
                if (animator.layers.Length <= layer)
                {
                    GUILayout.Label("No mouth layer");
                    return;
                }

                AnimatorStateMachine stateMachine = animator.layers[layer].stateMachine;
                DrawStateMachine(animator, stateMachine);

                GUILayout.Space(5);
                foldoutAddMouth = EditorGUILayout.Foldout(foldoutAddMouth, "Add new state");
                if (foldoutAddMouth)
                {
                    GUILayout.Label("If creating from a template. The animation's sprites must : ");
                    GUILayout.Label("1) Be strictly in 3 parts. (3 sprites for one animation)");
                    GUILayout.Label("2) Have the same path. (Part of the same texture)");
                    GUILayout.Label("3) Have the same name, with a _0,_1,_2 attached at the end");
                    GUILayout.Label("The used sprites will be infered from the given sprite name.");
                    GUILayout.Label("Ex : Test_Eye_0, Test_Eye_1, Test_Eye_2");

                    mouthStateName = EditorGUILayout.TextField("State name", mouthStateName);

                    mouthStateClip1 = EditorGUILayout.ObjectField("Clip1", mouthStateClip1, typeof(AnimationClip), false) as AnimationClip;
                    mouthStateClip2 = EditorGUILayout.ObjectField("Clip2", mouthStateClip2, typeof(AnimationClip), false) as AnimationClip;
                    mouthStateTexture = EditorGUILayout.ObjectField("Sprite (for Template)", mouthStateTexture, typeof(Sprite), false) as Sprite;

                    if (GUILayout.Button("Create New (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Mouth/", mouthStateName, null, true);
                    }

                    if (GUILayout.Button("Create New (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, layer, pathToAnimations + "Mouth/", mouthStateName, null, null, true);
                    }

                    if (GUILayout.Button("Create from clip (Animation Clip)"))
                    {
                        CreateFromAnimationClip(animator, stateMachine, pathToAnimations + "Mouth/", mouthStateName, mouthStateClip1, false);
                    }

                    if (GUILayout.Button("Create from clips (Talking Blend Tree)"))
                    {
                        CreateFromBlendTree(animator, stateMachine, layer, pathToAnimations + "Mouth/", mouthStateName, mouthStateClip1, mouthStateClip2, false);
                    }

                    if (GUILayout.Button("Create from template"))
                    {
                        CreateMouthFromTemplate(animator, stateMachine, pathToAnimations + "Mouth/", mouthStateName, mouthStateTexture);
                    }
                }
            }
        }
    }
}
