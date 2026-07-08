using ANF.GUI;


namespace ANF.ANSL
{
    /// <summary>
    /// The Show dialog window function can be used to show/hide the dialog window
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "showDialogWindow",
        functionAutoComplete: new string[] {
            "showDialogWindow(Shown)"
        },
        functionDesc: "Shows/Hides the dialog window")]
    public class ShowDialogWindowFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.BOOL}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out bool shown) &&
                manager.GetGUIManager().GetComponent<DialogUI>(out DialogUI dialogUI))
            {
                dialogUI.SetEnabled(shown);
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

