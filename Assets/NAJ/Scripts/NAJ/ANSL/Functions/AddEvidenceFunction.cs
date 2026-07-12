using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Add Evidence function allow you to add new evidence to the inventory
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "addEvidence",
        functionAutoComplete: new string[] {
            "addEvidence(Evidence)"
        },
        functionDesc: "Adds a new evidence to the inventory")]
    public class AddEvidenceFunction : ANSLFunction
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
                inventoryContainer.AddEvidence(evidenceID);
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

