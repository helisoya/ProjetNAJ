using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set location function allows you to change the current player location for the save slot info
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setLocation",
        functionAutoComplete: new string[] {
            "setLocation(Location)"
        },
        functionDesc: "Changes the location (visible on the save slot's info)")]
    public class SetLocationFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string location) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerVariableContainer>(out PlayerVariableContainer container))
            {
                container.SetLocation(location);
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

