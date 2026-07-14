using ANF.ANSL;
using ANF.GUI;
using ANF.Persistent;
using Leguar.TotalJSON;
using NAJ.GUI;

using UnityEngine;
using UnityEngine.InputSystem;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Set Examination Part function is used to update the Cross-Examination GUI Component
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "",
        functionAutoComplete: null,
        functionDesc: "Waits for user input during a cross examination. INTERNAL FUNCTION.")]
    public class ExaminationWaiterFunction : ANSLFunction
    {
        private bool inProgress;
        private string id;
        private bool shouldPress;
        private uint pressLine;
        private uint failLine;
        private uint endLine;
        private uint previousLine;
        private uint nextLine;
        private string[] evidenceToShow;

        private bool detectedGoingLeft = false;
        private bool detectedGoingRight = false;
        private bool detectedChangePart = false;
        private uint detectedChangePartValue;
        private bool detectedPress = false;
        private bool rebindEvents = false;

        private ExaminationUI examinationUI;
        private InventoryUI inventoryUI;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                                new FunctionParameterType[]{ FunctionParameterType.STRING, FunctionParameterType.BOOL, FunctionParameterType.UINT,
                    FunctionParameterType.UINT, FunctionParameterType.UINT,
                    FunctionParameterType.UINT, FunctionParameterType.UINT},
                new FunctionParameterType[]{ FunctionParameterType.STRING, FunctionParameterType.BOOL, FunctionParameterType.UINT,
                    FunctionParameterType.UINT, FunctionParameterType.UINT,
                    FunctionParameterType.UINT, FunctionParameterType.UINT, FunctionParameterType.LISTSTRING}
            };
        }


        protected override void OnStartProcess()
        {
            inProgress = true;

            if (parameters.GetParameter(0, out id) &&
                parameters.GetParameter(1, out shouldPress) &&
                parameters.GetParameter(2, out pressLine) &&
                parameters.GetParameter(3, out failLine) &&
                parameters.GetParameter(4, out endLine) &&
                parameters.GetParameter(5, out previousLine) &&
                parameters.GetParameter(6, out nextLine) &&
                manager.GetGUIManager().GetComponent(out examinationUI) &&
                manager.GetGUIManager().GetComponent(out inventoryUI))
            {
                if (parameters.GetTemplateId() == 1 &&
                    !parameters.GetParameter(7, out evidenceToShow))
                    evidenceToShow = null;

                inventoryUI.ClearSelectedItem();
                inventoryUI.SetCurrentMode(InventoryUI.InventoryMode.CanPresent);
                examinationUI.SetCanBeInteractedWith(true);

                examinationUI.onGoLeft.AddListener(OnGoLeft);
                examinationUI.onGoRight.AddListener(OnGoRight);
                examinationUI.onChangePart.AddListener(OnChangePart);
                examinationUI.onPressPart.AddListener(OnPress);

                PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed += OnPress;
                PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;

                if (manager.GetGUIManager().GetComponent(out InputReminderUI inputReminderUI))
                    inputReminderUI.SetReminderEnabled("press", true);
            }
            else
            {
                inProgress = false;
            }

            if (!inProgress)
                EndProcess();
        }

        private void OnGoLeft()
        {
            if (!detectedGoingRight)
                detectedGoingLeft = true;
        }

        private void OnGoRight()
        {
            if (!detectedGoingLeft)
                detectedGoingRight = true;
        }

        private void OnChangePart(uint targetLine)
        {
            detectedChangePart = true;
            detectedChangePartValue = targetLine;
        }

        private void OnPress()
        {
            detectedPress = true;
        }

        private void OnPress(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton() && inProgress && !inventoryUI.isEnabled && inventoryUI.selectedItem == null
                && !(manager.GetGUIManager().GetComponent(out PauseMenuUI pauseMenu) && pauseMenu.isEnabled))
                OnPress();
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            if (inProgress && !inventoryUI.isEnabled
                && !(manager.GetGUIManager().GetComponent(out PauseMenuUI pauseMenu) && pauseMenu.isEnabled))
            {
                float x = context.ReadValue<Vector2>().x;
                if (x < 0.5f)
                    OnGoLeft();
                else if (x > 0.5f)
                    OnGoRight();
            }
        }

        protected override void OnUpdate()
        {
            if (!examinationUI && !manager.GetGUIManager().GetComponent(out examinationUI))
            {
                EndProcess();
                return;
            }

            if (!inventoryUI && !manager.GetGUIManager().GetComponent(out inventoryUI))
            {
                EndProcess();
                return;
            }

            if (rebindEvents)
            {
                examinationUI.onGoLeft.AddListener(OnGoLeft);
                examinationUI.onGoRight.AddListener(OnGoRight);
                examinationUI.onChangePart.AddListener(OnChangePart);
                examinationUI.onPressPart.AddListener(OnPress);

                PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed += OnPress;
                PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed += OnMove;
            }

            if (detectedPress)
            {
                examinationUI.RegisterIdAsPressed(id);
                ResetUIToDefault();

                EndProcess();
                context.SetLineCounter(pressLine);
                return;
            }

            if (detectedChangePart)
            {
                ResetUIToDefault();

                EndProcess();
                context.SetLineCounter(detectedChangePartValue);
                return;
            }

            if (detectedGoingLeft)
            {
                ResetUIToDefault();

                EndProcess();
                context.SetLineCounter(previousLine);
                return;
            }

            if (detectedGoingRight)
            {
                ResetUIToDefault();

                EndProcess();
                context.SetLineCounter(nextLine);
                return;
            }

            if (!inventoryUI.isEnabled && inventoryUI.selectedItem != null)
            {
                string selectedItem = inventoryUI.selectedItem;

                // item was selected

                bool good = false;

                if (evidenceToShow != null)
                {
                    foreach (string evidence in evidenceToShow)
                    {
                        if (evidence.Equals(selectedItem))
                        {
                            good = true;
                            break;
                        }
                    }
                }

                uint correctLine = good ? endLine : failLine;

                ResetUIToDefault();

                EndProcess();
                context.SetLineCounter(correctLine);
                return;
            }

        }

        /// <summary>
		/// Resets the UI to default values
		/// </summary>
        private void ResetUIToDefault()
        {
            examinationUI.SetCanBeInteractedWith(false);
            examinationUI.SetEnabled(false);
            inventoryUI.SetCurrentMode(InventoryUI.InventoryMode.ViewOnly);

            if (manager.GetGUIManager().GetComponent(out InputReminderUI inputReminderUI))
                inputReminderUI.SetReminderEnabled("press", false);
        }

        protected override void OnCleanup()
        {
            if (examinationUI)
            {
                examinationUI.onGoLeft.RemoveListener(OnGoLeft);
                examinationUI.onGoRight.RemoveListener(OnGoRight);
                examinationUI.onChangePart.RemoveListener(OnChangePart);
                examinationUI.onPressPart.RemoveListener(OnPress);
            }

            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Present").performed -= OnPress;
            PersistentDataManager.instance.GetANFInput().GetInput().actions.FindAction("Move").performed -= OnMove;

            examinationUI = null;
            inventoryUI = null;

            detectedGoingLeft = false;
            detectedGoingRight = false;
            detectedPress = false;
            detectedChangePart = false;
            inProgress = false;
            rebindEvents = false;
        }

        protected override void OnSave(JSON json)
        {
            json.Add("inProgress", inProgress);
            json.Add("shouldPress", shouldPress);
            json.Add("id", id);
            json.Add("pressLine", pressLine);
            json.Add("failLine", failLine);
            json.Add("endLine", endLine);
            json.Add("previousLine", previousLine);
            json.Add("nextLine", nextLine);

            if (evidenceToShow != null)
                json.Add("evidenceToShow", evidenceToShow);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("inProgress"))
                inProgress = json.GetBool("inProgress");

            if (json.ContainsKey("shouldPress"))
                shouldPress = json.GetBool("shouldPress");

            if (json.ContainsKey("id"))
                id = json.GetString("id");

            if (json.ContainsKey("pressLine"))
                pressLine = json.GetJNumber("pressLine").AsUInt();

            if (json.ContainsKey("failLine"))
                failLine = json.GetJNumber("failLine").AsUInt();

            if (json.ContainsKey("endLine"))
                endLine = json.GetJNumber("endLine").AsUInt();

            if (json.ContainsKey("previousLine"))
                previousLine = json.GetJNumber("previousLine").AsUInt();

            if (json.ContainsKey("nextLine"))
                nextLine = json.GetJNumber("nextLine").AsUInt();

            if (json.ContainsKey("evidenceToShow"))
                evidenceToShow = json.GetJArray("evidenceToShow").AsStringArray();

            rebindEvents = true;

        }
    }
}

