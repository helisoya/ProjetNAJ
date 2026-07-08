using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The player name function changes the player's name
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "playerName",
        functionAutoComplete: new string[] { "playerName(playerName)" },
        functionDesc: "Changes the player's name")]
    public class PlayerNameFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer container))
            {
                if (parameters.GetParameter(0, out string newName))
                    container.SetPlayerName(newName);
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

