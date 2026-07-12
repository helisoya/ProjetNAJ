using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Remove All Profile function allow you to remove all profiles from the inventory
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "removeAllProfiles",
        functionAutoComplete: new string[] {
            "removeAllProfiles()"
        },
        functionDesc: "Removes all profiles from the inventory")]
    public class RemoveAllProfileFunction : ANSLFunction
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
                inventoryContainer.RemoveAllProfiles();
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

