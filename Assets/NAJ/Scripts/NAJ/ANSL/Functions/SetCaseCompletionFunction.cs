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
    /// The Set Case Completion Function can be used to set the current max case completed (for main menu)
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setCaseCompletion",
        functionAutoComplete: new string[]
        {
            "setCaseCompletion(Number)"
        },
        functionDesc: "Tries to set the max case completed (int)")]
    public class SetCaseCompletionFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.UINT}
            };
        }


        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out uint number) &&
                PersistentDataManager.instance.GetGlobalData().GetComponent(out CaseCompletionContainer completionContainer))
            {
                completionContainer.TrySetProgression(number);
                SaveUtils.SaveGlobalData(
                    PersistentDataManager.instance.GetGlobalData(),
                    PersistentDataManager.instance.GetANFInput(),
                    FileManager.savPath + PersistentDataManager.instance.GetANFSettings().saveFolder + "global.json");
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

