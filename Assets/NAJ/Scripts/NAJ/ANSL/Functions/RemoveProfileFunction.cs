using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Remove Profile function allow you to remove profiles from the inventory
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "removeProfile",
        functionAutoComplete: new string[] {
            "removeProfile(Profile)"
        },
        functionDesc: "Removes a profile from the inventory")]
    public class RemoveProfileFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string profileID) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer inventoryContainer))
            {
                inventoryContainer.RemoveProfile(profileID);
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

