//  ProtectedException


using Leguar.TotalJSON.Internal;
using System;

namespace Leguar.TotalJSON
{

    /// <summary>
    /// Exception thrown if trying to change anything in protected JSON or JArray objects.
    /// </summary>
    public class ProtectedException : Exception
    {

        internal ProtectedException(string message) : base(message)
        {
        }

        public override string StackTrace
        {
            get
            {
                return InternalTools.getCleanedStackTrace(base.StackTrace);
            }
        }

    }

}
