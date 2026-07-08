using ANF.Scene;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Add Static Function can be used to add a new static object to the scene
    /// </summary>
    [ANSLFunctionAttribute(
        functionBody: "addStatic",
        functionAutoComplete: new string[] {
            "addStatic(Name)",
            "addStatic(Name;Hidden)",
            "addStatic(Name;Hidden;StartingMarker)"
        },
        functionDesc: "Add a new static object (one per type)")]
    public class AddStaticFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL },
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL, FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string name) &&
                manager.GetWorld().GetComponent<StaticObjectManager>(out StaticObjectManager staticObjectManager))
            {
                if (staticObjectManager.AddSceneObject(name, out StaticObject obj))
                {
                    bool hidden = false;
                    if (parameters.GetTemplateId() >= 1)
                        if (!parameters.GetParameter(1, out hidden))
                            hidden = false;

                    string marker = null;
                    if (parameters.GetTemplateId() == 2)
                        if (!parameters.GetParameter(2, out marker))
                            marker = null;

                    if (hidden)
                        obj.SetAlpha(0, true);

                    if (marker != null && manager.GetWorld().GetComponent<ANF.Scene.BackgroundManager>(out ANF.Scene.BackgroundManager backgroundManager))
                    {
                        ANF.Scene.Background currentBackground = backgroundManager.GetBackground();

                        if (currentBackground != null)
                        {
                            obj.SetPosition(currentBackground.GetMarkerPosition(marker), true);
                            obj.SetRotation(currentBackground.GetMarkerRotation(marker), true);
                        }
                    }
                }
            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

