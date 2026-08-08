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

        functionBody: "createUserProfile",
        functionAutoComplete: new string[] {
            "createUserProfile(Id)",
            "createUserProfile(Id;IconSpritesheet)"
        },
        functionDesc: "Creates a new User Profile")]
    public class CreateUserProfileFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING},
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer inventoryContainer))
            {
                string iconSpritesheet;

                if (parameters.GetTemplateId() == 0 ||
                    !parameters.GetParameter(1, out iconSpritesheet))
                    iconSpritesheet = null;

                inventoryContainer.CreateUserProfile(id, iconSpritesheet);
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

