using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ANF.Editor
{
    /// <summary>
    /// Represents an editor window capable of generating a character
    /// </summary>
    public class CharacterGeneratorEditor : EditorWindow
    {
        private TextField characterIdField;
        private TextField hierarchyField;
        private ObjectField modelField;
        private ObjectField interactionIconField;
        private ObjectField eyesField;
        private ObjectField mouthField;
        private ObjectField spriteMaterialField;
        private Vector3Field spriteScaleField;
        private Vector3Field spritePositionField;
        private Vector3Field interactionBoxCenterField;
        private Vector3Field interactionBoxSizeField;
        private Toggle createTemplateAnimationsToggle;
        private TextField animationsPathField;
        private CharacterGeneratorType type;

        public enum CharacterGeneratorType
        {
            Default,
            MouthEyeSprites
        }

        public void Init(CharacterGeneratorType type)
        {
            this.type = type;
            CreateGUIElements();
        }


        [MenuItem("ANF/Character Generator/Mouth & Eyes Sprites")]
        public static void ShowHQGenerator()
        {
            CharacterGeneratorEditor wnd = CreateWindow<CharacterGeneratorEditor>();
            wnd.Init(CharacterGeneratorType.MouthEyeSprites);
            wnd.titleContent = new GUIContent("Character Generator");
        }

        [MenuItem("ANF/Character Generator/Default")]
        public static void ShowDefaultGenerator()
        {
            CharacterGeneratorEditor wnd = CreateWindow<CharacterGeneratorEditor>();
            wnd.Init(CharacterGeneratorType.Default);
            wnd.titleContent = new GUIContent("Character Generator");
        }

        public void CreateGUIElements()
        {
            VisualElement root = rootVisualElement;
            root.style.flexWrap = Wrap.Wrap;

            Label label = new Label("Character Generator");
            root.Add(label);

            if (type == CharacterGeneratorType.Default)
            {
                Label desc = new Label("Generates a character. This character will not have eyes / mouths setup by default.");
                desc.style.flexWrap = Wrap.Wrap;
                root.Add(desc);
            }
            else if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                Label desc = new Label("Generates a character. Eyes & Mouth sprite renderer will be generated as well. Check the default test character if you want to see the end result.");
                desc.style.flexWrap = Wrap.Wrap;
                root.Add(desc);
            }

            characterIdField = new TextField("Character's ID");
            root.Add(characterIdField);

            modelField = new ObjectField("Character's model");
            modelField.objectType = typeof(GameObject);
            root.Add(modelField);

            interactionIconField = new ObjectField("Interaction Icon");
            interactionIconField.objectType = typeof(Texture2D);
            root.Add(interactionIconField);

            interactionBoxCenterField = new Vector3Field("Interaction Box Center");
            root.Add(interactionBoxCenterField);

            interactionBoxSizeField = new Vector3Field("Interaction Box Size");
            root.Add(interactionBoxSizeField);

            interactionBoxCenterField.SetValueWithoutNotify(
                new Vector3(
                    EditorPrefs.GetFloat("ANF_CG_INTERACTION_BOX_CENTER_X", -0.01400936f),
                    EditorPrefs.GetFloat("ANF_CG_INTERACTION_BOX_CENTER_Y", 3.24f),
                    EditorPrefs.GetFloat("ANF_CG_INTERACTION_BOX_CENTER_Z", 0.0f)
                )
            );

            interactionBoxSizeField.SetValueWithoutNotify(
                new Vector3(
                    EditorPrefs.GetFloat("ANF_CG_INTERACTION_BOX_SIZE_X", 3.171302f),
                    EditorPrefs.GetFloat("ANF_CG_INTERACTION_BOX_SIZE_Y", 9.5621f),
                    EditorPrefs.GetFloat("ANF_CG_INTERACTION_BOX_SIZE_Z", 1.0f)
                )
            );

            characterIdField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_NAME", ""));
            modelField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<GameObject>(EditorPrefs.GetString("ANF_CG_MODEL", null)));
            interactionIconField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Texture2D>(EditorPrefs.GetString("ANF_CG_INTERACTION", null)));


            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                hierarchyField = new TextField("Path to Head");
                root.Add(hierarchyField);

                root.Add(new Label("Mouth & Eyes sprites must :"));
                root.Add(new Label("1) Be strictly in 3 parts. (3 sprites for one animation)"));
                root.Add(new Label("2) Have the same path. (Part of the same texture)"));
                root.Add(new Label("3) Have the same name, with a _0,_1,_2 attached at the end"));
                root.Add(new Label("The used sprites will be infered from the given sprite name."));
                root.Add(new Label("Ex : Test_Eye_0, Test_Eye_1, Test_Eye_2"));

                eyesField = new ObjectField("Eyes sprite");
                eyesField.objectType = typeof(Sprite);
                root.Add(eyesField);

                mouthField = new ObjectField("Mouth Sprite");
                mouthField.objectType = typeof(Sprite);
                root.Add(mouthField);

                spriteMaterialField = new ObjectField("Sprite Material");
                spriteMaterialField.objectType = typeof(Material);
                root.Add(spriteMaterialField);

                spriteScaleField = new Vector3Field("Sprite Scale");
                root.Add(spriteScaleField);

                spritePositionField = new Vector3Field("Sprite position");
                root.Add(spritePositionField);

                spriteScaleField.SetValueWithoutNotify(
                    new Vector3(
                        EditorPrefs.GetFloat("ANF_CG_SPRITE_SCALE_X", 0.7f),
                        EditorPrefs.GetFloat("ANF_CG_SPRITE_SCALE_Y", 0.7f),
                        EditorPrefs.GetFloat("ANF_CG_SPRITE_SCALE_Z", 0.7f)
                    )
                );

                spritePositionField.SetValueWithoutNotify(
                    new Vector3(
                        EditorPrefs.GetFloat("ANF_CG_SPRITE_POSITION_X", 0.0f),
                        EditorPrefs.GetFloat("ANF_CG_SPRITE_POSITION_Y", -0.411f),
                        EditorPrefs.GetFloat("ANF_CG_SPRITE_POSITION_Z", 0.196f)
                    )
                );

                hierarchyField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_HEADPATH",
                    "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"));
                mouthField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Sprite>(EditorPrefs.GetString("ANF_CG_MOUTH", null)));
                eyesField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Sprite>(EditorPrefs.GetString("ANF_CG_EYE", null)));
                spriteMaterialField.SetValueWithoutNotify(AssetDatabase.LoadAssetAtPath<Material>(EditorPrefs.GetString("ANF_CG_MATERIAL", null)));

            }

            animationsPathField = new TextField("Animation's path (A subfolder will be created)");
            root.Add(animationsPathField);

            createTemplateAnimationsToggle = new Toggle("Create Template Animations");
            root.Add(createTemplateAnimationsToggle);

            createTemplateAnimationsToggle.SetValueWithoutNotify(EditorPrefs.GetBool("ANF_CG_CREATETEMPLATE", true));
            animationsPathField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_ANIM_FOLDER", "Assets/Animations/Characters/"));


            Button button = new Button();
            button.name = "button";
            button.text = "Generate";
            button.clicked += OnGenerate;
            root.Add(button);

            Button resetButton = new Button();
            resetButton.name = "buttonReset";
            resetButton.text = "Reset Values";
            resetButton.clicked += ResetValues;
            root.Add(resetButton);
        }

        void ResetValues()
        {
            EditorPrefs.SetString("ANF_CG_NAME", "");

            characterIdField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_NAME", ""));

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                EditorPrefs.SetString("ANF_CG_HEADPATH", "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End");
                hierarchyField.SetValueWithoutNotify(EditorPrefs.GetString("ANF_CG_HEADPATH",
                    "mixamorig:Hips/mixamorig:Spine/mixamorig:Spine1/mixamorig:Spine2/mixamorig:Neck/mixamorig:Head/mixamorig:HeadTop_End"));
            }
        }

        public void OnGenerate()
        {
            if (type == CharacterGeneratorType.MouthEyeSprites && (spriteMaterialField.value == null || eyesField.value == null ||
                mouthField.value == null || string.IsNullOrEmpty(hierarchyField.value)) && string.IsNullOrEmpty(animationsPathField.value))
                return;

            if (modelField.value == null || interactionIconField.value == null || string.IsNullOrEmpty(characterIdField.value))
                return;


            EditorPrefs.SetString("ANF_CG_NAME", characterIdField.value);
            EditorPrefs.SetString("ANF_CG_NAME", characterIdField.value);
            EditorPrefs.SetString("ANF_CG_MODEL", AssetDatabase.GetAssetPath(modelField.value));
            EditorPrefs.SetString("ANF_CG_INTERACTION", AssetDatabase.GetAssetPath(interactionIconField.value));
            EditorPrefs.SetBool("ANF_CG_CREATETEMPLATE", createTemplateAnimationsToggle.value);
            EditorPrefs.SetString("ANF_CG_ANIM_FOLDER", animationsPathField.value);

            EditorPrefs.SetFloat("ANF_CG_INTERACTION_BOX_CENTER_X", interactionBoxCenterField.value.x);
            EditorPrefs.SetFloat("ANF_CG_INTERACTION_BOX_CENTER_Y", interactionBoxCenterField.value.y);
            EditorPrefs.SetFloat("ANF_CG_INTERACTION_BOX_CENTER_Z", interactionBoxCenterField.value.z);

            EditorPrefs.SetFloat("ANF_CG_INTERACTION_BOX_SIZE_X", interactionBoxSizeField.value.x);
            EditorPrefs.SetFloat("ANF_CG_INTERACTION_BOX_SIZE_Y", interactionBoxSizeField.value.y);
            EditorPrefs.SetFloat("ANF_CG_INTERACTION_BOX_SIZE_Z", interactionBoxSizeField.value.z);

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                EditorPrefs.SetString("ANF_CG_HEADPATH", hierarchyField.value);
                EditorPrefs.SetString("ANF_CG_MOUTH", AssetDatabase.GetAssetPath(mouthField.value));
                EditorPrefs.SetString("ANF_CG_EYE", AssetDatabase.GetAssetPath(eyesField.value));
                EditorPrefs.SetString("ANF_CG_MATERIAL", AssetDatabase.GetAssetPath(spriteMaterialField.value));

                EditorPrefs.SetFloat("ANF_CG_SPRITE_SCALE_X", spriteScaleField.value.x);
                EditorPrefs.SetFloat("ANF_CG_SPRITE_SCALE_Y", spriteScaleField.value.y);
                EditorPrefs.SetFloat("ANF_CG_SPRITE_SCALE_Z", spriteScaleField.value.z);

                EditorPrefs.SetFloat("ANF_CG_SPRITE_POSITION_X", spritePositionField.value.x);
                EditorPrefs.SetFloat("ANF_CG_SPRITE_POSITION_Y", spritePositionField.value.y);
                EditorPrefs.SetFloat("ANF_CG_SPRITE_POSITION_Z", spritePositionField.value.z);
            }

            Sprite defaultMouthSprite = null;
            Sprite defaultEyeSprite = null;

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(eyesField.value));
                string templateName = eyesField.value.name.Substring(0, eyesField.value.name.Length - 2);

                foreach (Object sprite in sprites)
                {
                    if (sprite is Sprite && sprite.name.EndsWith('0') && sprite.name.Substring(0, sprite.name.Length - 2).Equals(templateName))
                    {
                        defaultEyeSprite = sprite as Sprite;
                        break;
                    }
                }

                templateName = mouthField.value.name.Substring(0, mouthField.value.name.Length - 2);
                sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mouthField.value));
                foreach (Object sprite in sprites)
                {
                    if (sprite is Sprite && sprite.name.EndsWith('0') && sprite.name.Substring(0, sprite.name.Length - 2).Equals(templateName))
                    {
                        defaultMouthSprite = sprite as Sprite;
                        break;
                    }
                }
            }

            // Generate roots
            GameObject characterRoot = new GameObject(characterIdField.value);

            GameObject modelRoot = PrefabUtility.InstantiatePrefab(modelField.value as GameObject) as GameObject;
            modelRoot.name = "Model";
            //PrefabUtility.UnpackPrefabInstance(modelField.value as GameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);
            modelRoot.transform.SetParent(characterRoot.transform);
            modelRoot.transform.localScale = Vector3.one;

            // Generate the interaction
            GameObject interactableRoot = new GameObject("Interaction");
            Scene.InteractableObject interactableObject = interactableRoot.AddComponent<Scene.InteractableObject>();
            interactableRoot.layer = LayerMask.NameToLayer("Interaction");
            BoxCollider interactionCollider = interactableRoot.AddComponent<BoxCollider>();
            interactionCollider.center = interactionBoxCenterField.value;
            interactionCollider.size = interactionBoxSizeField.value;
            interactableRoot.transform.SetParent(modelRoot.transform);
            interactableRoot.transform.localScale = Vector3.one;

            // Generate Animations
            string pathToAnimations = animationsPathField.value;
            if (!pathToAnimations.EndsWith('/'))
                pathToAnimations += '/';
            pathToAnimations += characterIdField.value + "/";

            DirectoryInfo dir = new DirectoryInfo(pathToAnimations);
            if (!dir.Exists)
                dir.Create();

            dir.CreateSubdirectory("Body");
            dir.CreateSubdirectory("Eye");
            dir.CreateSubdirectory("Mouth");

            if (type == CharacterGeneratorType.MouthEyeSprites)
            {
                Transform headRoot = modelRoot.transform;
                string[] split = hierarchyField.value.Split('/');
                foreach (string path in split)
                {
                    headRoot = headRoot.Find(path);
                }

                // Generate mouth animator
                GameObject mouthRoot = new GameObject("Mouth");
                mouthRoot.transform.SetParent(headRoot);
                mouthRoot.transform.localScale = spriteScaleField.value;
                mouthRoot.transform.localEulerAngles = new Vector3(0, 180, 0);
                mouthRoot.transform.localPosition = spritePositionField.value;

                SpriteRenderer spriteRenderer = mouthRoot.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = defaultMouthSprite;
                spriteRenderer.material = spriteMaterialField.value as Material;
                spriteRenderer.sortingOrder = 1;

                // Generate eye animator
                GameObject eyeRoot = new GameObject("Eye");
                eyeRoot.transform.SetParent(headRoot);
                eyeRoot.transform.localScale = spriteScaleField.value;
                eyeRoot.transform.localEulerAngles = new Vector3(0, 180, 0);
                eyeRoot.transform.localPosition = spritePositionField.value + new Vector3(0.0f, 0.05f, 0.0f);

                spriteRenderer = eyeRoot.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = defaultEyeSprite;
                spriteRenderer.material = spriteMaterialField.value as Material;
                spriteRenderer.sortingOrder = 1;
            }

            // Add Character

            interactableObject.EditorInit(characterIdField.value, interactionIconField.value as Texture2D,
                    modelRoot.GetComponentsInChildren<Renderer>(),
                    interactionCollider);

            Scene.Character character = characterRoot.AddComponent<Scene.Character>();
            character.EditorInit(characterIdField.value, characterRoot.AddComponent<Animator>(),
                characterRoot.GetComponentsInChildren<Renderer>(),
                interactableObject);

            Editor.CharacterEditor.CreateDefaultController(character, pathToAnimations);

            Animator animator = character.GetAnimator();
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;

            if (createTemplateAnimationsToggle.value)
            {
                Editor.CharacterEditor.CreateBodyFromTemplate(controller, controller.layers[0].stateMachine,
                    pathToAnimations + "Body/", "Normal");

                if (type == CharacterGeneratorType.MouthEyeSprites)
                {
                    Editor.CharacterEditor.CreateEyeFromTemplate(controller, controller.layers[1].stateMachine,
                        pathToAnimations + "Eye/", "Normal", eyesField.value as Sprite);

                    Editor.CharacterEditor.CreateMouthFromTemplate(controller, controller.layers[2].stateMachine,
                        pathToAnimations + "Mouth/", "Normal", mouthField.value as Sprite);
                }
            }


            // Generate Prefab
            bool success;
            PrefabUtility.SaveAsPrefabAssetAndConnect(characterRoot,
                "Assets/Resources/Characters/" + characterIdField.value + ".prefab",
                InteractionMode.AutomatedAction, out success);

            AssetDatabase.SaveAssets();

            if (success) Debug.LogWarning("Character Generation finished");
            else Debug.LogError("Character Generation failed");

            Close();
        }
    }
}