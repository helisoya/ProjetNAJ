using ANF.Persistent;
using Leguar.TotalJSON;

namespace ANF.Locals
{
    /// <summary>
    /// A player locals represents the part of the locals system that should be saved in player datas (Ex: Additional files)
    /// </summary>
    [System.Serializable]
    public class PlayerLocals : DataContainer
    {
        private string[] currentAdditionalFiles = null;

        public DataContainer CloneContainer()
        {
            return new PlayerLocals();
        }

        public void Initialize(ANFSettings settings)
        {
        }

        public void Reset()
        {
            currentAdditionalFiles = null;
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Locals>(out Locals locals))
            {
                locals.ChangeAdditionalFiles(currentAdditionalFiles);
            }
        }

        /// <summary>
        /// Changes the local's additional files
        /// </summary>
        /// <param name="newAdditionalFiles">The new additional files</param>
        public void SetAdditionalFiles(string[] newAdditionalFiles)
        {
            currentAdditionalFiles = newAdditionalFiles;
            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Locals>(out Locals locals))
            {
                locals.ChangeAdditionalFiles(currentAdditionalFiles);
            }
        }

        public void Save(JSON json)
        {
            if (currentAdditionalFiles != null)
                json.Add("currentAdditionalFiles", currentAdditionalFiles);
        }

        public void Load(JSON json)
        {
            if (json.ContainsKey("currentAdditionalFiles"))
                currentAdditionalFiles = json.GetJArray("currentAdditionalFiles").AsStringArray();

            if (PersistentDataManager.instance.GetGlobalData().GetComponent<Locals>(out Locals locals))
            {
                locals.ChangeAdditionalFiles(currentAdditionalFiles);
            }
        }
    }
}

