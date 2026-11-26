using Leguar.TotalJSON;
using ANF.ANSL;
using ANF.Persistent;
using NAJ.Persistent;
using Unity.VisualScripting;

namespace NAJ.ANSL
{
    /// <summary>
    /// The Set Deduction Node Active Link can be used to change if a node is the active link in a switch
    /// </summary>
    [ANSLFunctionAttribute(

        functionBody: "setDeductionNodeActiveLink",
        functionAutoComplete: new string[] {
            "setDeductionNodeActiveLink(Id;ActiveLink)",
        },
        functionDesc: "Changes if a deduction node is the active link in a switch")]
    public class SetDeductionNodeActiveLinkFunction : ANSLFunction
    {
        public override FunctionParameterType[][] GetParametersTemplates()
        {
            return new FunctionParameterType[][] {
                new FunctionParameterType[]{FunctionParameterType.STRING, FunctionParameterType.BOOL },
            };
        }

        protected override void OnStartProcess()
        {

            if (parameters.GetParameter(0, out string id) &&
                parameters.GetParameter(1, out bool activeLink) &&
                PersistentDataManager.instance.GetPlayerData().GetComponent(out DeductionsContainer deductionsContainer))
            {
                DeductionNode node = deductionsContainer.GetNode(id);
                if (node != null && !string.IsNullOrEmpty(node.linkedNode))
                {
                    DeductionNode linkedNode = deductionsContainer.GetNode(node.linkedNode);
                    if (linkedNode != null)
                    {
                        node.SetIsActiveLink(activeLink);
                        linkedNode.SetIsActiveLink(!activeLink);

                    }
                }

            }

            EndProcess();
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnCleanup()
        {
            // Unused
        }

        protected override void OnSave(JSON json)
        {

        }

        protected override void OnLoad(JSON json)
        {

        }
    }
}

