using ANF.GUI;
using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Wait For Context End Function can be used to wait for the end of a context
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "waitForContextEnd",
        functionAutoComplete: new string[] {
            "waitForContextEnd(contextId)"
        },
        functionDesc: "Waits for the end of a context")]
    public class waitForContextEndFunction : ANSLFunction
    {
        private bool waitingForEnd;
        private uint linkedContextId;
        private ANSLManager anslManager;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.UINT },
            };
        }

        protected override void OnStartProcess()
        {
            waitingForEnd = false;
            if (parameters.GetParameter(0, out linkedContextId) &&
                manager.GetWorld().GetComponent<ANSLManager>(out anslManager))
            {
                waitingForEnd = anslManager.GetContext(linkedContextId).isRunning;
            }

            if (!waitingForEnd)
                EndProcess();
        }

        protected override void OnUpdate()
        {
            if (anslManager == null)
                manager.GetWorld().GetComponent<ANSLManager>(out anslManager);

            if (anslManager != null && !anslManager.GetContext(linkedContextId).isRunning)
                EndProcess();
        }

        protected override void OnCleanup()
        {

        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForEnd", waitingForEnd);
            json.Add("linkedContextId", linkedContextId);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForEnd"))
                waitingForEnd = json.GetBool("waitingForEnd");
            else
                waitingForEnd = false;

            if (json.ContainsKey("linkedContextId"))
                linkedContextId = json.GetJNumber("linkedContextId").AsUInt();
        }
    }
}

