using UnityEngine;


namespace ANF.GUI
{
    /// <summary>
    /// Represents a Generic In/Out Transition for a UI component.
    /// Implement this class to create new transition effects.
    /// In/Out Transitions can be affected to Enabled/Paused
    /// </summary>
    public abstract class GUIInOutTransition
    {
        [Header("Base")]
        [SerializeField] protected GameObject root;
        [SerializeField] protected bool hideWhenOut = true;
        [SerializeField] protected float transitionDuration = 0.5f;

        protected const int TWEENING_ID = -4343;


        /// <summary>
        /// On start callback. Use it to setup the transition
        /// </summary>
        public abstract void OnStart();

        /// <summary>
        /// Transitions the UI inside the visible camera space
        /// </summary>
        /// <param name="immediate">True if the transition must be immediate</param>
        public abstract void TransitionIn(bool immediate = false);

        /// <summary>
        /// Transitions the UI outside of the visible camera space
        /// </summary>
        /// <param name="immediate">True if the transition must be immediate</param>
        public abstract void TransitionOut(bool immediate = false);
    }
}

