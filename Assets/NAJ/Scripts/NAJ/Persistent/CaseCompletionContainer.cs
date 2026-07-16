using ANF.Persistent;
using Leguar.TotalJSON;
using UnityEngine;

namespace NAJ.Persistent
{
    /// <summary>
    /// Contains the global case progression
    /// </summary>
    [System.Serializable]
    public class CaseCompletionContainer : DataContainer
    {
        private uint currentProgression;

        public DataContainer CloneContainer()
        {
            return new CaseCompletionContainer()
            {
            };
        }

        public void Initialize(ANFSettings settings)
        {
            Reset();
        }

        public void Reset()
        {
            currentProgression = 0;
        }

        /// <summary>
        /// Gets the current global progression
        /// </summary>
        /// <returns>The current global progression</returns>
        public uint GetCurrentProgression()
        {
            return currentProgression;
        }

        /// <summary>
        /// Try setting the progression to a specific value. The stored value can only go up.
        /// </summary>
        /// <param name="newProgression">The new progression value</param>
        public void TrySetProgression(uint newProgression)
        {
            if (currentProgression < newProgression)
                currentProgression = newProgression;
        }

        public void Save(JSON json)
        {
            json.Add("currentProgression", currentProgression);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("currentProgression"))
                currentProgression = json.GetJNumber("currentProgression").AsUInt();
        }
    }
}

