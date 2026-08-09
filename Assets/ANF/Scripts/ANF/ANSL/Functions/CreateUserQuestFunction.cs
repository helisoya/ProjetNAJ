using ANF.GUI;
using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Create User Quest function can be used to create a new local quest
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "createUserQuest",
        functionAutoComplete: new string[] {
            "createUserQuest(Variable;Category;MaxState)"
            },
        functionDesc: "Creates a User Quest")]
    public class CreateUserQuestFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.INT }
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out QuestInfosContainer container) &&
                parameters.GetParameter(0, out string variableId) &&
                parameters.GetParameter(1, out string categoryId) &&
                parameters.GetParameter(2, out int maxState))
            {
                container.CreateUserQuest(variableId, categoryId, maxState);
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

