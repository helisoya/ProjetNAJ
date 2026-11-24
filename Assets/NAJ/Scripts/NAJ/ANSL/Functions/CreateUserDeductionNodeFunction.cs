using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using Unity.VisualScripting;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Create Deduction Node can be used to create a user node from ANSL Scripts
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "createUserDeductionNode",
        functionAutoComplete: new string[] {
            "createUserDeductionNode(Id;IsVisible;PosX;PosY;LabelPos;UISizeFactor;LinkedNode;IsActiveLink;IsEndNode;EndScript;Prerequistes)",
        },
        functionDesc: "Creates a new User Deduction Node")]
    public class CreateUserDeductionNodeFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL,
                    FunctionParameterType.INT, FunctionParameterType.INT, FunctionParameterType.STRING,
                    FunctionParameterType.FLOAT, FunctionParameterType.STRING, FunctionParameterType.BOOL,
                    FunctionParameterType.BOOL, FunctionParameterType.STRING, FunctionParameterType.LISTSTRING },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out bool isVisible) &&
                parameters.GetParameter(2, out int posX) &&
                parameters.GetParameter(3, out int posY) &&
                parameters.GetParameter(4, out string labelPosition) &&
                parameters.GetParameter(5, out float uiSizeFactor) &&
                parameters.GetParameter(6, out string linkedNode) &&
                parameters.GetParameter(7, out bool isActiveLink) &&
                parameters.GetParameter(8, out bool isEndNode) &&
                parameters.GetParameter(9, out string endScript) &&
                parameters.GetParameter(10, out string[] prerequistes) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out DeductionsContainer deductionsContainer))
            {
                DeductionNode.LabelPosition parsedLabelPos = DeductionNode.LabelPosition.Left;
                if (labelPosition != null)
                {
                    labelPosition = labelPosition.ToLower();
                    switch (labelPosition)
                    {
                        case "left":
                            parsedLabelPos = DeductionNode.LabelPosition.Left;
                            break;
                        case "right":
                            parsedLabelPos = DeductionNode.LabelPosition.Right;
                            break;
                        case "top":
                            parsedLabelPos = DeductionNode.LabelPosition.Top;
                            break;
                        case "bottom":
                            parsedLabelPos = DeductionNode.LabelPosition.Bottom;
                            break;
                    }
                }

                if (prerequistes != null)
                {
                    bool good = true;
                    foreach (string item in prerequistes)
                    {
                        if (item != null)
                        {
                            good = false;
                            break;
                        }
                    }
                    if (good)
                        prerequistes = null;
                }

                deductionsContainer.CreateUserNode(id, isVisible, new UnityEngine.Vector2Int(posX, posY), parsedLabelPos,
                uiSizeFactor, linkedNode, isActiveLink, isEndNode, endScript, prerequistes);
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

