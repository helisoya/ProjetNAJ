//  JSONKeyNotFoundException


using Leguar.TotalJSON.Internal;
using System.Collections.Generic;

namespace Leguar.TotalJSON
{

    /// <summary>
    /// Exception that is thrown if given parameter key doesn't exist in JSON.
    /// </summary>
    public class JSONKeyNotFoundException : KeyNotFoundException
    {

        internal JSONKeyNotFoundException(string message)
            : base(message)
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
