using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using Unity.VisualScripting;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Set Deduction Node Visible can be used to change if a node is visible or not
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setDeductionNodeVisible",
        functionAutoComplete: new string[] {
            "setDeductionNodeVisible(Id;Visible)",
        },
        functionDesc: "Changes if a deduction node is visible or not")]
    public class SetDeductionNodeVisibleFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out bool visible) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out DeductionsContainer deductionsContainer))
            {
                DeductionNode node = deductionsContainer.GetNode(id);
                if (node != null)
                    node.SetVisible(visible);
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

