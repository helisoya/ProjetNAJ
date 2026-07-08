using ANF.Locals;
using ANF.Persistent;


namespace ANF.ANSL
{
    /// <summary>
    /// The Set Locals function allows you to change the additional files for the locals system
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "setLocals",
        functionAutoComplete: new string[] {
            "setLocals(Locals)"
        },
        functionDesc: "Shows/Hides the dialog window")]
    public class SetLocalsFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.LISTSTRING}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string[] list) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent<PlayerLocals>(out PlayerLocals locals))
            {
                if (list.Length == 0 && list[0] == null)
                    list = null;
                locals.SetAdditionalFiles(list);
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

