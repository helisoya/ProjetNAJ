using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using Unity.VisualScripting;
using NAJ.GUI;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Show Deduction Node Popup can be used to show the deduction node popup
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "showDeductionNodePopup",
        functionAutoComplete: new string[] {
            "showDeductionNodePopup()",
        },
        functionDesc: "Shows the deduction node popup")]
    public class ShowDeductionNodePopupFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][]
            {
                new FunctionParameterType[]{}
            };
        }

        protected override void OnStartProcess()
        {
            if (manager.GetGUIManager().GetComponent(out DeductionsUI deductionsUI))
            {
                deductionsUI.ShowANSLPopup();
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

