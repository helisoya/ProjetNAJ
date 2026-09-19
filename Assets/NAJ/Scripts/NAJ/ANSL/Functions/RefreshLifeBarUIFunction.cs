using ANF.ANSL;
using ANF.Persistent;
using ANF.Utils;
using JetBrains.Annotations;
using Leguar.TotalJSON;
using NAJ.GUI;
using NAJ.Persistent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Refresh Life Bar UI function can be used to refresh the life bar UI
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "refreshLifeBarUI",
        functionAutoComplete: new string[]
        {
            "refreshLifeBarUI(Immediate;RefreshValues)"
        },
        functionDesc: "Refreshes the life bar UI")]
    public class RefreshLifeBarUIFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.BOOL, FunctionParameterType.BOOL}
            };
        }


        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out bool immediate) &&
                parameters.GetParameter(1, out bool refreshValues) &&
                manager.GetGUIManager().GetComponent(out LifePointsUI lifePointsUI))
            {
                lifePointsUI.Refresh(immediate, refreshValues);
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

