using ANF.GUI;
using ANF.Scene;
using UnityEngine;

namespace NAJ.GUI
{
    /// <summary>
    /// Handles the click that opens the case selection menu
    /// </summary>
    [System.Serializable]
    public class PauseMenuCasSelectionButtonData : PauseMenuButtonData
    {
        public override void OnClick(PauseMenuUI pauseMenu, ANFManager manager)
        {
            if(manager.GetGUIManager().GetComponent(out CaseSelectionMenu caseSelectionMenu))
            {
                pauseMenu.ChangeSubMenu(caseSelectionMenu);
            }
        }
    }
}

