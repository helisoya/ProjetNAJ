//  JArgumentNullException


using Leguar.TotalJSON.Internal;
using System;

namespace Leguar.TotalJSON
{

    /// <summary>
    /// Exception that is thrown if some JSON, JArray or JString parameter is null when it is not allowed to be null.
    /// </summary>
    public class JArgumentNullException : ArgumentNullException
    {

        internal JArgumentNullException(string paramName, string message)
            : base(paramName, message)
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
