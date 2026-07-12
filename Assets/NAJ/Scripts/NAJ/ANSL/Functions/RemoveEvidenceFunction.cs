using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Remove Evidence function allow you to remove evidence from the inventory
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "removeEvidence",
        functionAutoComplete: new string[] {
            "removeEvidence(Evidence)"
        },
        functionDesc: "Removes evidence from the inventory")]
    public class RemoveEvidenceFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string evidenceID) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer inventoryContainer))
            {
                inventoryContainer.RemoveEvidence(evidenceID);
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

