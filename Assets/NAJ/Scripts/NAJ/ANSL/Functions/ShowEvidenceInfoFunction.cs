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
    /// The Show Evidence Info function can be used to show the information of a specific item on screen
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "showEvidenceInfo",
        functionAutoComplete: new string[]
        {
            "showEvidenceInfo(Item;Placement)"
        },
        functionDesc: "Shows the information of a specific item on screen (Evidence/XXX or Profile/XXX)")]
    public class ShowEvidenceInfoFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{ FunctionParameterType.STRING, FunctionParameterType.STRING}
            };
        }


        protected override void OnStartProcess()
        {
            if (parameters.GetParameter(0, out string item) &&
                parameters.GetParameter(1, out string placement) &&
                manager.GetGUIManager().GetComponent(out EvidenceInfoUI evidenceInfoUI))
            {
                string[] split = item.Split('/', 2);
                if (split.Length == 2)
                {
                    bool isEvidence = split[0].ToLower().Equals("evidence");

                    string lowerPlacement = placement.ToLower();
                    EvidenceInfoUI.Status status = EvidenceInfoUI.Status.ShowLeft;
                    if (lowerPlacement.Equals("left"))
                        status = EvidenceInfoUI.Status.ShowLeft;
                    else if (lowerPlacement.Equals("right"))
                        status = EvidenceInfoUI.Status.ShowRight;
                    else if (lowerPlacement.Equals("full"))
                        status = EvidenceInfoUI.Status.ShowFull;

                    evidenceInfoUI.SetInfos(status, split[1], isEvidence);
                }

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

