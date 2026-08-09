using ANF.GUI;
using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Create User Quest function can be used to create a new local quest
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "deleteUserQuest",
        functionAutoComplete: new string[] {
            "deleteUserQuest(Variable;Category)"
            },
        functionDesc: "Deletes a User Quest")]
    public class DeleteUserQuestFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out QuestInfosContainer container) &&
                parameters.GetParameter(0, out string variableId) &&
                parameters.GetParameter(1, out string categoryId))
            {
                container.RemoveUserQuest(variableId, categoryId);
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {
            // Unused
        }

        protected override void OnCleanup()
        {
            // Unused
        }
    }
}

