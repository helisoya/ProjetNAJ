using ANF.Persistent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
    [SerializeField] private Sprite defaultButtonSprite = default;
    [SerializeField] private Sprite defaultBackgroundSprite = default;
    [SerializeField] private MapsData maps = default;

    private List<ANF.Persistent.MapButton> currentButtons;
    private List<Image> currentButtonsOnMap;
    private Image currentVisual;
    private ANF.Persistent.MapButton currentButton;

    private TextField mapIdTextField;
    private Button saveButton;
    private Button newMapButton;
    private Button newButtonButton;
    private Button removeMapButton;

    private VisualElement buttonInfoRoot;
    private TextField buttonIdTextField;
    private Button buttonRenameButton;
    private Slider buttonRotationSlider;
    private Slider buttonPosXSlider;
    private Slider buttonPosYSlider;
    private ObjectField buttonSpriteField;

    private Image backgroundImage;

    private ScrollView mapScrollView;
    private ScrollView buttonsScrollView;

    private const float buttonSize = 5.0f;


    [MenuItem("ANF/Map Editor")]
    public static void ShowExample()
    {
        MapEditorWindow wnd = GetWindow<MapEditorWindow>();
        wnd.titleContent = new GUIContent("Map Editor");
    }

    public void CreateGUI()
    {
        currentButtons = new List<ANF.Persistent.MapButton>();
        currentButtonsOnMap = new List<Image>();

        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        ///root.Add(label);

        // Instantiate UXML
        VisualElement uxmlData = m_VisualTreeAsset.Instantiate();
        root.Add(uxmlData);

        mapIdTextField = uxmlData.Q<TextField>("MapIDTextField");
        saveButton = uxmlData.Q<Button>("SaveButton");
        newMapButton = uxmlData.Q<Button>("NewMap");
        newButtonButton = uxmlData.Q<Button>("NewButton");
        removeMapButton = uxmlData.Q<Button>("RemoveButton");

        buttonInfoRoot = uxmlData.Q<VisualElement>("ButtonInfoRoot");
        buttonIdTextField = uxmlData.Q<TextField>("ButtonIDTextField");
        buttonRenameButton = uxmlData.Q<Button>("RenameButton");
        buttonPosXSlider = uxmlData.Q<Slider>("XSlider");
        buttonPosYSlider = uxmlData.Q<Slider>("YSlider");
        buttonRotationSlider = uxmlData.Q<Slider>("RotationSlider");
        buttonSpriteField = uxmlData.Q<ObjectField>("ButtonSpriteField");

        backgroundImage = uxmlData.Q<Image>("BackgroundImg");

        mapScrollView = uxmlData.Q<ScrollView>("ScrollViewMaps");
        buttonsScrollView = uxmlData.Q<ScrollView>("ScrollViewButtons");

        backgroundImage.RegisterCallback<DragUpdatedEvent>(OnBackgroundDrag);
        backgroundImage.RegisterCallback<DragPerformEvent>(OnBackgroundDrop);

        newMapButton.clicked += NewMap;
        removeMapButton.clicked += RemoveMap;
        saveButton.clicked += SaveMap;
        newButtonButton.clicked += NewButton;

        buttonRenameButton.clicked += RenameButton;
        buttonPosXSlider.RegisterCallback<ChangeEvent<float>>(OnButtonSlider);
        buttonPosYSlider.RegisterCallback<ChangeEvent<float>>(OnButtonSlider);
        buttonRotationSlider.RegisterCallback<ChangeEvent<float>>(OnButtonSlider);
        buttonSpriteField.RegisterCallback<ChangeEvent<Object>>(OnButtonSpriteChange);

        buttonSpriteField.objectType = typeof(Sprite);

        backgroundImage.sprite = defaultBackgroundSprite;

        RefreshMapScrollView();

        NewMap();
    }

    void Update()
    {
        RefreshAllVisualButtonsPosition();
    }

    /// <summary>
    /// Creates a new map
    /// </summary>
    private void NewMap()
    {
        currentButtons.Clear();
        backgroundImage.Clear();
        currentButtonsOnMap.Clear();
        buttonInfoRoot.SetEnabled(false);
        currentVisual = null;
        currentButton = null;

        RefreshButtonScrollView();
    }

    /// <summary>
    /// Removes the map from the pool
    /// </summary>
    private void RemoveMap()
    {
        if (!string.IsNullOrEmpty(mapIdTextField.text) && !string.IsNullOrWhiteSpace(mapIdTextField.text))
        {
            for (int i = 0; i < maps.maps.Count; i++)
            {
                if (maps.maps[i].id.Equals(mapIdTextField.text))
                {
                    maps.maps.RemoveAt(i);
                    EditorUtility.SetDirty(maps);
                }
            }
        }
        NewMap();
        RefreshMapScrollView();
    }

    /// <summary>
    /// Saves the map
    /// </summary>
    private void SaveMap()
    {
        if (!string.IsNullOrEmpty(mapIdTextField.text) && !string.IsNullOrWhiteSpace(mapIdTextField.text))
        {
            ANF.Persistent.MapData map = maps.GetMap(mapIdTextField.text);

            if (map == null)
            {
                map = new ANF.Persistent.MapData();
                map.id = mapIdTextField.text;
                maps.maps.Add(map);
            }

            map.backgroundSprite = backgroundImage.sprite;
            map.buttons.Clear();

            foreach (ANF.Persistent.MapButton button in currentButtons)
            {
                map.buttons.Add(new ANF.Persistent.MapButton()
                {
                    id = button.id,
                    sprite = button.sprite,
                    rotation = button.rotation,
                    normalizedPosition = button.normalizedPosition
                });
            }

            EditorUtility.SetDirty(maps);
        }

        RefreshMapScrollView();
    }

    /// <summary>
    /// Creates a new button on screen
    /// </summary>
    private void NewButton()
    {
        int id = 0;
        while (ButtonNameExists(id.ToString()))
            id++;

        CreateButton(id.ToString(), 0.0f, defaultButtonSprite, new Vector2(0.5f, 0.5f));

        RefreshButtonScrollView();
    }

    /// <summary>
	/// Tries to rename the currentButton
	/// </summary>
    private void RenameButton()
    {
        if (currentButton != null && !string.IsNullOrEmpty(buttonIdTextField.text)
        && !string.IsNullOrWhiteSpace(buttonIdTextField.text) && !ButtonNameExists(buttonIdTextField.text))
        {
            currentButton.id = buttonIdTextField.text;
            RefreshButtonScrollView();
        }
    }

    /// <summary>
    /// Refreshs the button scroll view
    /// </summary>
    private void RefreshMapScrollView()
    {
        mapScrollView.Clear();

        using (var it = Enumerable.Range(0, maps.maps.Count).GetEnumerator())
        {
            while (it.MoveNext())
            {
                int value = it.Current;
                Button instance = new Button();
                instance.text = maps.maps[value].id;
                instance.clicked += () => { OnMapClick(value); };
                mapScrollView.Add(instance);
            }
        }
    }

    /// <summary>
    /// Callback when clicking on a map
    /// </summary>
    /// <param name="index">The map's index</param>
    private void OnMapClick(int index)
    {
        currentButtons.Clear();
        currentButtonsOnMap.Clear();
        backgroundImage.Clear();
        currentVisual = null;
        currentButton = null;

        buttonInfoRoot.SetEnabled(false);
        ANF.Persistent.MapData newMap = maps.maps[index];

        mapIdTextField.SetValueWithoutNotify(newMap.id);
        backgroundImage.sprite = newMap.backgroundSprite;

        foreach (ANF.Persistent.MapButton button in newMap.buttons)
            CreateButton(button.id, button.rotation, button.sprite, button.normalizedPosition);

        RefreshButtonScrollView();
    }

    /// <summary>
    /// Refreshs the button scroll view
    /// </summary>
    private void RefreshButtonScrollView()
    {
        buttonsScrollView.Clear();

        using (var it = Enumerable.Range(0, currentButtons.Count).GetEnumerator())
        {
            while (it.MoveNext())
            {
                int indexValue = it.Current;
                string id = currentButtons[indexValue].id;
                Button instance = new Button();
                instance.text = id;
                instance.clicked += () => { OnButtonClick(id); };
                buttonsScrollView.Add(instance);
            }
        }
    }

    /// <summary>
    /// Callback when clicking on a button
    /// </summary>
    /// <param name="id">The button's id</param>
    private void OnButtonClick(string id)
    {
        for (int i = 0; i < currentButtons.Count; i++)
        {
            ANF.Persistent.MapButton button = currentButtons[i];
            if (button.id.Equals(id))
            {
                buttonInfoRoot.SetEnabled(true);
                buttonIdTextField.SetValueWithoutNotify(button.id);
                buttonPosXSlider.SetValueWithoutNotify(button.normalizedPosition.x);
                buttonPosYSlider.SetValueWithoutNotify(button.normalizedPosition.y);
                buttonRotationSlider.SetValueWithoutNotify(button.rotation);

                buttonSpriteField.SetValueWithoutNotify(button.sprite);

                if (currentVisual != null)
                    currentVisual.tintColor = Color.white;

                currentVisual = currentButtonsOnMap[i];
                currentButton = button;

                currentVisual.tintColor = Color.red;
            }
        }
    }


    /// <summary>
	/// Callback when moving a button slider
	/// </summary>
	/// <param name="value">The new value (unused)</param>
    private void OnButtonSlider(ChangeEvent<float> value)
    {
        if (currentVisual != null)
        {
            currentButton.normalizedPosition.x = buttonPosXSlider.value;
            currentButton.normalizedPosition.y = buttonPosYSlider.value;
            currentButton.rotation = buttonRotationSlider.value;
        }
    }

    /// <summary>
    /// Checks if a button id is already in use
    /// </summary>
    /// <param name="name">The button's id</param>
    /// <returns>True if it is already in use</returns>
    private bool ButtonNameExists(string id)
    {
        foreach (ANF.Persistent.MapButton button in currentButtons)
            if (button.id.Equals(id))
                return true;
        return false;
    }

    /// <summary>
    /// Creates a new button and its visual
    /// </summary>
    /// <param name="id">The button's id</param>
    /// <param name="sprite">The button's sprite</param>
    /// <param name="rotation">The button's rotation</param>
    /// <param name="normalizedPosition"> The button's normalized position</param>
    private void CreateButton(string id, float rotation, Sprite sprite, Vector2 normalizedPosition)
    {
        currentButtons.Add(new ANF.Persistent.MapButton()
        {
            id = id,
            sprite = sprite,
            rotation = rotation,
            normalizedPosition = normalizedPosition
        });

        Image visualButton = new Image();
        visualButton.sprite = sprite;
        backgroundImage.Add(visualButton);
        visualButton.style.position = Position.Absolute;

        visualButton.style.width = buttonSize;
        visualButton.style.height = buttonSize;

        currentButtonsOnMap.Add(visualButton);
        visualButton.RegisterCallback<ClickEvent>((ClickEvent e) =>
        {
            OnButtonClick(id);
        });
    }

    /// <summary>
    /// Refreshs all visual button's position
    /// </summary>
    private void RefreshAllVisualButtonsPosition()
    {
        if (backgroundImage == null || backgroundImage.sprite == null)
            return;

        float spriteHeight, spriteWidth;
        if (backgroundImage.resolvedStyle.width > backgroundImage.resolvedStyle.height)
        {
            var mul = (backgroundImage.resolvedStyle.height / backgroundImage.sprite.rect.height);
            spriteWidth = backgroundImage.sprite.rect.width * mul;
            spriteHeight = backgroundImage.resolvedStyle.height;
        }
        else
        {
            var mul = (backgroundImage.resolvedStyle.width / backgroundImage.sprite.rect.width);
            spriteWidth = backgroundImage.resolvedStyle.width;
            spriteHeight = backgroundImage.sprite.rect.height * mul;
        }


        float sizeDiffX = backgroundImage.resolvedStyle.width - spriteWidth;
        float sizeDiffY = backgroundImage.resolvedStyle.height - spriteHeight;

        for (int i = 0; i < currentButtons.Count; i++)
        {

            currentButtonsOnMap[i].style.width = spriteWidth * (buttonSize / 100.0f);
            currentButtonsOnMap[i].style.height = spriteWidth * (buttonSize / 100.0f);
            currentButtonsOnMap[i].style.top = sizeDiffY / 2.0f + currentButtons[i].normalizedPosition.y * spriteHeight - currentButtonsOnMap[i].style.height.value.value / 2.0f;
            currentButtonsOnMap[i].style.left = sizeDiffX / 2.0f + currentButtons[i].normalizedPosition.x * spriteWidth - currentButtonsOnMap[i].style.width.value.value / 2.0f;
            currentButtonsOnMap[i].style.rotate = new(Angle.Degrees(currentButtons[i].rotation));
        }
    }

    void OnBackgroundDrag(DragUpdatedEvent _)
    {
        Object droppedObject = DragAndDrop.objectReferences[0];

        if (droppedObject && droppedObject is Sprite)
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
        else
            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
    }

    void OnBackgroundDrop(DragPerformEvent _)
    {
        Object droppedObject = DragAndDrop.objectReferences[0];

        if (droppedObject && droppedObject is Sprite)
        {
            backgroundImage.sprite = (Sprite)droppedObject;
        }
    }

    /// <summary>
	/// Callback on changed sprite
	/// </summary>
	/// <param name="value">Unused</param>
    void OnButtonSpriteChange(ChangeEvent<Object> value)
    {
        if (currentButton != null)
        {
            currentButton.sprite = buttonSpriteField.value ? buttonSpriteField.value as Sprite : null;
            currentVisual.sprite = currentButton.sprite;
        }
    }
}
