using ANF.Persistent;
using ANF.Scene;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ANF.GUI
{
    /// <summary>
    /// Represents a button in the pause menu
    /// </summary>
    public class PauseMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private Locals.LocalizedText label;
        [SerializeField] private Image icon;
        [SerializeField] private RectTransform iconTransform;

        private PauseMenuUI pauseMenu;
        private ANFManager manager;
        private PauseMenuButtonData data;
        private int id;

        /// <summary>
        /// Initialize the button
        /// </summary>
        /// <param name="id">The button's id</param>
        /// <param name="data">The button's data</param>
        /// <param name="pauseMenu">The pause menu</param>
        /// <param name="manager">The ANF Manager</param>
        public void Initialize(int id, PauseMenuButtonData data, PauseMenuUI pauseMenu, ANFManager manager)
        {
            this.id = id;
            this.data = data;
            this.pauseMenu = pauseMenu;
            this.manager = manager;

            iconTransform.sizeDelta = Vector2.zero;

            if (data != null)
            {
                label.SetNewKey(data.labelKey);
                icon.sprite = data.iconSprite;
            }
        }

        public void OnClick()
        {
            if (data != null)
                data.OnClick(pauseMenu, manager);
        }

        public void OnEnter()
        {
            iconTransform.DOSizeDelta(Vector2.one * 40f, 0.5f).SetEase(Ease.OutQuad);
            buttonRoot.DOAnchorPosX(25, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnExit()
        {
            iconTransform.DOSizeDelta(Vector2.zero, 0.5f).SetEase(Ease.OutQuad);
            buttonRoot.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutQuad);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                pauseMenu.SelectCurrentButton();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            pauseMenu.SetCurrentButton(id);
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }
    }


    /// <summary>
    /// Represents the data linked to a pause menu button.
    /// Extend this to create a new button
    /// </summary>
    [System.Serializable]
    public abstract class PauseMenuButtonData
    {
        public Sprite iconSprite;
        public string labelKey;

        public abstract void OnClick(PauseMenuUI pauseMenu, ANFManager manager);
    }

    /// <summary>
    /// Subclass for the resume button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataResume : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            pauseMenu.SetEnabled(false);
        }
    }

    /// <summary>
    /// Subclass for the save button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataSave : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<SaveMenuUI>(out SaveMenuUI saveMenu))
            {
                saveMenu.SetIsInSaveMode(true);
                pauseMenu.ChangeSubMenu(saveMenu);
            }
        }
    }

    /// <summary>
    /// Subclass for the load button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataLoad : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<SaveMenuUI>(out SaveMenuUI saveMenu))
            {
                saveMenu.SetIsInSaveMode(false);
                pauseMenu.ChangeSubMenu(saveMenu);
            }
        }
    }

    /// <summary>
    /// Subclass for the Quest button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataQuest : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<QuestsMenuUI>(out QuestsMenuUI questsMenu))
            {
                pauseMenu.ChangeSubMenu(questsMenu);
            }
        }
    }

    /// <summary>
    /// Subclass for the New Game button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataNewGame : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<NewGameMenu>(out NewGameMenu newGameMenu))
            {
                pauseMenu.ChangeSubMenu(newGameMenu);
            }
        }
    }

    /// <summary>
    /// Subclass for the Logs button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataLogs : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<LogsMenuUI>(out LogsMenuUI logsMenu))
            {
                pauseMenu.ChangeSubMenu(logsMenu);
            }
        }
    }

    /// <summary>
    /// Subclass for the History button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataHistory : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<HistoryMenuUI>(out HistoryMenuUI historyMenuUI))
            {
                pauseMenu.ChangeSubMenu(historyMenuUI);
            }
        }
    }

    /// <summary>
    /// Subclass for the options button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataOptions : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if (manager.GetGUIManager().GetComponent<SettingsMenuUI>(out SettingsMenuUI settingsMenu))
            {
                pauseMenu.ChangeSubMenu(settingsMenu);
            }
        }
    }

    /// <summary>
    /// Subclass for the Main Menu button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataMainMenu : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            manager.ChangeScene(PersistentDataManager.instance.GetANFSettings().mainMenuScene);
        }
    }

    /// <summary>
    /// Subclass for the Quit Game button
    /// </summary>
    [System.Serializable]
    public class PauseMenuButtonDataQuitGame : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            Application.Quit();
        }
    }
}
