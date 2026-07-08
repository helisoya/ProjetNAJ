using Leguar.TotalJSON;
using UnityEngine;


namespace ANF.ANSL
{
    /// <summary>
    /// The wait function can be used to wait for a certain number of seconds
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "wait",
        functionAutoComplete: new string[] { "wait(Seconds)" },
        functionDesc: "Waits for a certain number of seconds")]
    public class WaitFunction : ANSLFunction
    {
        private float timeToWait;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.FLOAT}
            };
        }

        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out float time))
                timeToWait = time;
            else
                EndProcess();
        }

        public void OnSkipModeToggle(bool enabled)
        {
            if (enabled && timeToWait > 0.1f)
                timeToWait = 0.1f;
        }

        protected override void OnUpdate()
        {
            timeToWait -= Time.deltaTime;
            if (timeToWait <= 0)
                EndProcess();
        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {
            json.Add("timeToWait", timeToWait);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("timeToWait"))
                timeToWait = json.GetFloat("timeToWait");
        }
    }
}

