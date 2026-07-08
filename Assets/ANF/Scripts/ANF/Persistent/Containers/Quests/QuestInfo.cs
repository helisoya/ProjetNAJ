namespace ANF.Persistent
{
    public struct QuestInfo
    {
        public string variableID;
        public string categoryID;
        public int maxQuestState;

        /// <summary>
        /// Gets a quest's name key
        /// </summary>
        /// <returns>Its name's locals key</returns>
        public string GetNameKey()
        {
            return $"Quest_{variableID}_name";
        }

        /// <summary>
        /// Gets a quest's description key
        /// </summary>
        /// <returns>Its description's locals key</returns>
        public string GetDescKey()
        {
            return $"Quest_{variableID}_desc";
        }

        /// <summary>
        /// Gets a quest's done key
        /// </summary>
        /// <returns>Its done's locals key</returns>
        public string GetDoneKey()
        {
            return $"Quest_{variableID}_done";
        }

        /// <summary>
        /// Gets a quest's canceled key
        /// </summary>
        /// <returns>The canceled's locals key</returns>
        public string GetCanceledKey()
        {
            return $"Quest_{variableID}_canceled";
        }

        /// <summary>
        /// Gets a quest state's key
        /// </summary>
        /// <param name="state">The quest's state</param>
        /// <returns>The state's locals key</returns>
        public string GetStateKey(int state)
        {
            return $"Quest_{variableID}_{state}";
        }
    }

}
