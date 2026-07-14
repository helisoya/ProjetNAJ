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
    /// The Check Examination Pressed function is used to check if the required function were pressed
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "",
        functionAutoComplete: null,
        functionDesc: "Checks if the specified parts were pressed. INTERNAL FUNCTION.")]
    public class CheckExaminationPressedFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.UINT},
                new FunctionParameterType[]{ FunctionParameterType.UINT, FunctionParameterType.LISTSTRING}
            };
        }


        protected override void OnStartProcess()
        {
            EndProcess();
            if (parameters.GetTemplateId() == 1 &&
                parameters.GetParameter(0, out uint endLine) &&
                parameters.GetParameter(1, out string[] toCheck) &&
                manager.GetGUIManager().GetComponent(out ExaminationUI examinationUI))
            {
                List<string> pressed = examinationUI.GetPressedIds();
                bool good = true;

                foreach (string check in toCheck)
                {
                    if (!pressed.Contains(check))
                    {
                        good = false;
                        break;
                    }
                }

                if (good)
                    context.SetLineCounter(endLine);
            }
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

