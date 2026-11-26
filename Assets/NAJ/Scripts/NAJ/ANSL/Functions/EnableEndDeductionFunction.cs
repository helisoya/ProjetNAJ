using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using Unity.VisualScripting;
using NAJ.GUI;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Set Deduction Node Visible can be used to change if a node is visible or not
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "enableEndDeduction",
        functionAutoComplete: new string[] {
            "enableEndDeduction(Enabled)",
        },
        functionDesc: "Sets if an end node can be selected in the deductions UI")]
    public class EnableEndDeductionFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.BOOL },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out bool isEnabled) &&
                manager.GetGUIManager().GetComponent(out DeductionsUI deductionsUI))
            {
                deductionsUI.SetCanSelectEndNode(isEnabled);
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

