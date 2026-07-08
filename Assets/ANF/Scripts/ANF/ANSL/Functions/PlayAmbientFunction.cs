using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Play Ambient function can be used to play a Ambient
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "playAmbient",
        functionAutoComplete: new string[] {
            "playAmbient(Ambient)",
            "playAmbient(Ambient;BaseVolume)"
            },
        functionDesc: "Plays an ambient")]
    public class PlayAmbientFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING},
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.FLOAT}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Persistent.AudioManager>(out Persistent.AudioManager audioManager) &&
                parameters.GetParameter(0, out string name))
            {
                float baseVolume;
                if (parameters.GetTemplateId() == 0 || !parameters.GetParameter(1, out baseVolume))
                    baseVolume = 1.0f;

                audioManager.PlayAmbient(name, baseVolume);
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

