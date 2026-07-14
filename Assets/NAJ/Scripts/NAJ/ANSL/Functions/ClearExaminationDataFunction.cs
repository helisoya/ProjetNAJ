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
    /// The Clear Examination Data is used to clear the cached cross examination data and truly end it
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "clearExaminationData",
        functionAutoComplete: new string[]
        {
            "clearExaminationData()"
        },
        functionDesc: "Clears the cross-examination data, ending it.")]
    public class ClearExaminationDataFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{}
            };
        }


        protected override void OnStartProcess()
        {
            if (manager.GetGUIManager().GetComponent(out ExaminationUI examinationUI))
            {
                examinationUI.ClearData();
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

