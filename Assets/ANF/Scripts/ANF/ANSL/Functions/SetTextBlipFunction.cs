using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Text Blip Function can be used to change the current text blip sound
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setTextBlip",
        functionAutoComplete: new string[] {
            "setTextBlip(Name)",
            "setTextBlip(default)",
            },
        functionDesc: "Changes the current text blip")]
    public class SetTextBlipFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Persistent.AudioManager>(out Persistent.AudioManager audioManager) &&
                parameters.GetParameter(0, out string name))
            {
                if (name.Equals("default"))
                    audioManager.SetTextBlipToDefault();
                else
                    audioManager.SetTextBlip(name);
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

