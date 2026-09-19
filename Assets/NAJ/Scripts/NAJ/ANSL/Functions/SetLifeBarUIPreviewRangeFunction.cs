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
    /// The Set Life Bar UI Preview Range function can be used to set the life bar UI's Preview range (damage / regen)
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setLifeBarUIPreviewRange",
        functionAutoComplete: new string[]
        {
            "setLifeBarUIPreviewRange(Range;Immediate;Refresh)"
        },
        functionDesc: "Changes the life bar UI's preview range (This will chip away at the health bar)")]
    public class SetLifeBarUIPreviewRangeFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.INT , FunctionParameterType.BOOL, FunctionParameterType.BOOL }
            };
        }


        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out int range) &&
                parameters.GetParameter(1, out bool immediate) &&
                parameters.GetParameter(2, out bool refresh) &&
                manager.GetGUIManager().GetComponent(out LifePointsUI lifePointsUI))
            {
                lifePointsUI.SetPreviewRange(range, immediate, refresh);
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

