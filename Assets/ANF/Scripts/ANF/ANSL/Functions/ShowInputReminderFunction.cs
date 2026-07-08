using ANF.GUI;
using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Show Input Reminder Function can be used to show/hide an input reminder
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "showInputReminder",
        functionAutoComplete: new string[] {
            "showInputReminder(Id;Enabled)"
        },
        functionDesc: "Shows/Hides an input reminder")]
    public class ShowInputReminderFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL },
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out bool enabled) &&
                manager.GetGUIManager().GetComponent<InputReminderUI>(out InputReminderUI inputReminderUI))
            {
                inputReminderUI.SetReminderEnabled(id, enabled);
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {

        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

