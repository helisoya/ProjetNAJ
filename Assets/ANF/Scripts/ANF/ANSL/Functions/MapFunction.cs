using ANF.GUI;
using ANF.Persistent;
using ANF.Utils;
using Leguar.TotalJSON;


namespace ANF.ANSL
{
    /// <summary>
    /// The Map function opens the map UI and launches the selected script
    /// </summary>
    [ANSLFunctionAttribute(
        
        functionBody: "map",
        functionAutoComplete: new string[] {
            "map(Map;Def;CurrentLocation)"
        },
        functionDesc: "Opens a map using a specific definition. Current player location on map can be null")]
    public class MapFunction : ANSLFunction
    {
        private bool waitingForMap = false;
        private MapUI mapUI;

        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.STRING, FunctionParameterType.STRING }
            };
        }

        protected override void OnStartProcess()
        {
            waitingForMap = false;
            if (parameters.GetParameter(0, out string map) &&
                parameters.GetParameter(1, out string mapDef) &&
                parameters.GetParameter(2, out string currentLocation) &&
                manager.GetGUIManager().GetComponent<MapUI>(out mapUI) &&
                PersistentDataManager.instance.GetGlobalData().GetComponent<MapContainer>(out MapContainer mapContainer))
            {
                if (mapContainer.GetMap(map, out ANF.Persistent.MapData foundData) &&
                    mapContainer.GetDef(mapDef, out MapDefs foundDefs))
                {
                    waitingForMap = true;
                    mapUI.SetEnabled(true, foundData, foundDefs, currentLocation);
                }
                else
                {
                    EndProcess();
                }
            }
            else
            {
                EndProcess();
            }

        }

        protected override void OnUpdate()
        {
            if (mapUI == null)
                manager.GetGUIManager().GetComponent<MapUI>(out mapUI);

            if (mapUI != null && mapUI.showingMap)
                return;

            if (mapUI)
            {
                if (mapUI.showingMap)
                    return;

                EndProcess();

                string selectedScript = mapUI.selectedScript;
                string resolvedScript = ANSLUtils.ResolveFilePath(context.GetCurrentFilepath(), selectedScript);

                if (resolvedScript != null)
                    context.LoadScript(resolvedScript);

                mapUI = null;
            }
            else
            {
                EndProcess();
            }
        }

        protected override void OnCleanup()
        {
            
        }

        protected override void OnSave(JSON json)
        {
            json.Add("waitingForMap", waitingForMap);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("waitingForMap"))
                waitingForMap = json.GetBool("waitingForMap");
            else
                waitingForMap = false;
        }
    }
}

