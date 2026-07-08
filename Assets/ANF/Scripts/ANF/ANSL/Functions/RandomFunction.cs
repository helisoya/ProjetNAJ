using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The random function generates a random value and stores it inside the "random" variable
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "random",
        functionAutoComplete: new string[] { "random(Min;Max)" },
        functionDesc: "Generates a random variable between [Min,Max[")]
    public class RandomFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.INT, FunctionParameterType.INT}
            };
        }

        protected override void OnStartProcess()
        {
            if (PersistentDataManager.instance.GetPlayerData().GetComponent(out PlayerVariableContainer container))
            {
                if (parameters.GetParameter(0, out int min) && parameters.GetParameter(1, out int max))
                    container.GenerateRandom(min, max);
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

