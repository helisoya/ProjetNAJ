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
    /// The Show Examination UI function can be used to show/hide the cross examination UI
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "showExaminationUI",
        functionAutoComplete: new string[]
        {
            "showExaminationUI(Shown)"
        },
        functionDesc: "Shows/Hides the cross examination UI")]
    public class ShowExaminationUIFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.BOOL}
            };
        }


        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out bool enabled) &&
                manager.GetGUIManager().GetComponent(out ExaminationUI examinationUI))
            {
                examinationUI.SetEnabled(enabled);
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

