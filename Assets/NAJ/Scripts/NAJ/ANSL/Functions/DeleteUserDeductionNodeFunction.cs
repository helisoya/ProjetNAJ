using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using Unity.VisualScripting;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Delete Deduction Node can be used to delete a user node from ANSL Scripts
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "deleteUserDeductionNode",
        functionAutoComplete: new string[] {
            "deleteUserDeductionNode(Id)",
        },
        functionDesc: "Deletes a user Deduction Node")]
    public class DeleteUserDeductionNodeFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out DeductionsContainer deductionsContainer))
            {
                deductionsContainer.DeleteUserNode(id);
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

