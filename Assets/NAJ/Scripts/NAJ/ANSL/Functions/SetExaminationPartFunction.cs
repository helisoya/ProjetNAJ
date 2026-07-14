using ANF.ANSL;
using Leguar.TotalJSON;
using NAJ.GUI;

using System.Diagnostics;
using UnityEngine;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Set Examination Part function is used to update the Cross-Examination GUI Component
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setExaminationPart",
        functionAutoComplete: new string[]
        {
            "setExaminationPart(Id)"
        },
        functionDesc: "Sets what part of the examination is the current one.")]
    public class SetExaminationPartFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.STRING}
            };
        }


        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string id) &&
                manager.GetGUIManager().GetComponent(out ExaminationUI examinationUI))
            {
                examinationUI.SetId(id);
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

