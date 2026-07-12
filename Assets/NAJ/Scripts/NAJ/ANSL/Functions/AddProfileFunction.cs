using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Add Profile function allow you to add new profile to the inventory
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "addProfile",
        functionAutoComplete: new string[] {
            "addProfile(Profile)"
        },
        functionDesc: "Adds a new profile to the inventory")]
    public class AddProfileFunction : ANSLFunction
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
                inventoryContainer.AddProfile(profileID);
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

