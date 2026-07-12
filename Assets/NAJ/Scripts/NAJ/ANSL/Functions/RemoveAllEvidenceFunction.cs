using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Remove All Evidence function allow you to remove all evidence from the inventory
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "removeAllEvidence",
        functionAutoComplete: new string[] {
            "removeAllEvidence()"
        },
        functionDesc: "Removes all evidence from the inventory")]
    public class RemoveAllEvidenceFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][]
            {
                new FunctionParameterType[]{},
            };
        }

        protected override void OnStartProcess()
        {

            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer inventoryContainer))
            {
                inventoryContainer.RemoveAllEvidence();
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

