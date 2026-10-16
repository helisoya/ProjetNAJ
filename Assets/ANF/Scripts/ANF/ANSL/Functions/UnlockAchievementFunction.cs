using ANF.Persistent;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Unlock Achievement Function can be used to unlock a steam achievement
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "unlockAchievement",
        functionAutoComplete: new string[] {
            "unlockAchievement(Id)"
        },
        functionDesc: "Unlocks a steam achievement")]
    public class UnlockAchievementFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out ANFSteamworkContainer steamworkContainer))
            {
                steamworkContainer.UnlockAchievement(id);
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

