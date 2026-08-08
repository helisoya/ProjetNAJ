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

        functionBody: "createUserEvidence",
        functionAutoComplete: new string[] {
            "createUserEvidence(Id;NumberOfCheckImages)",
            "createUserEvidence(Id;NumberOfCheckImages;IconSpritesheet)",
            "createUserEvidence(Id;NumberOfCheckImages;IconSpritesheet;CheckImagesSpritesheet)",
        },
        functionDesc: "Creates a new User Evidence")]
    public class CreateUserEvidenceFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT, FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.UINT, FunctionParameterType.STRING, FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out uint checkImages) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out NAJCaseInventoryContainer inventoryContainer))
            {
                string iconSpritesheet;
                string checkImagesSpriteSheet;

                if (parameters.GetTemplateId() == 0 ||
                    !parameters.GetParameter(2, out iconSpritesheet))
                    iconSpritesheet = null;

                if (parameters.GetTemplateId() <= 1 ||
                    !parameters.GetParameter(3, out checkImagesSpriteSheet))
                    checkImagesSpriteSheet = null;

                inventoryContainer.CreateUserEvidence(id, checkImages, iconSpritesheet, checkImagesSpriteSheet);
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

