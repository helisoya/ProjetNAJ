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
    /// The Hide Evidence Info function can be used to hide the currently visible evidence infos
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "HideEvidenceInfo",
        functionAutoComplete: new string[]
        {
            "HideEvidenceInfo()"
        },
        functionDesc: "Hides the currently visible evidence infos")]
    public class HideEvidenceInfoFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{}
            };
        }


        protected override void OnStartProcess()
        {
            if (manager.GetGUIManager().GetComponent(out EvidenceInfoUI evidenceInfoUI))
            {
                evidenceInfoUI.HideInfos();
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

