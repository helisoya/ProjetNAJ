using ANF.GUI;
using ANF.Locals;
using ANF.Persistent;
using DG.Tweening;
using Leguar.TotalJSON;
using NAJ.Persistent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NAJ.GUI
{
    /// <summary>
    /// Handles the rendering of UI Animations (Ex: Guilty/Not Guilty animations)
    /// </summary>
    public class AnimationUI : GUIComponent
    {
        [Header("All")]
        [SerializeField] private CanvasGroup canvasGroup;

        private Dictionary<string, Animator> animations;

        public override void OnInitialize()
        {
            animations = new Dictionary<string, Animator>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1.0f;
        }
        
        /// <summary>
        /// Checks if an animation is playing
        /// </summary>
        /// <param name="animationId">The animation's ID</param>
        /// <returns>True if playing</returns>
        public bool IsAnimationInProgress(string animationId)
        {
            return animations.ContainsKey(animationId);
        }
        
        /// <summary>
        /// Starts playing a new animation (Only one instance of the same animation at any given point)
        /// </summary>
        /// <param name="animationId">The animation's Id</param>
        /// <returns>True if the animation was started, false if not found or already existing</returns>
        public bool PlayAnimation(string animationId)
        {
            if (animations.ContainsKey(animationId))
                return false;


            Animator animator = Resources.Load<Animator>("UIAnimations/" + animationId);
            if (animator == null)
                return false;

            animations.Add(animationId, Instantiate(animator, root.transform));

            return true;
        }

        public override void OnStart()
        {

        }

        public override void OnUpdate()
        {
            List<string> toRemove = new List<string>();

            foreach(string key in animations.Keys)
            {
                if (animations[key].GetCurrentAnimatorStateInfo(0).normalizedTime > 1 &&
                    !animations[key].IsInTransition(0))
                    toRemove.Add(key);
            }

            foreach (string key in toRemove)
            {
                Destroy(animations[key].gameObject);
                animations.Remove(key);
            }
        }

        public override void OnEnabled()
        {
            OnUnPaused();
        }

        public override void OnDisabled()
        {
            OnPaused();
        }

        public override void OnPaused()
        {
            foreach (Animator animation in animations.Values)
                animation.speed = 0.0f;
            canvasGroup.DOFade(0.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnUnPaused()
        {
            foreach (Animator animation in animations.Values)
                animation.speed = 1.0f;
            canvasGroup.DOFade(1.0f, 0.5f).SetEase(Ease.OutQuad);
        }

        public override void OnRegisterInputs()
        {
        }

        public override void OnUnRegisterInputs()
        {
        }

        public override bool OnChangeScene()
        {
            return true;
        }

        public override bool IsCleaningUpForSceneChange()
        {
            return false;
        }

        public override void OnSave(JSON json)
        {
            json.Add("animations", animations.Keys.ToArray());
        }

        public override void OnLoad(JSON json)
        {
            if(json.ContainsKey("animations"))
            {
                string[] animations = json.GetJArray("animations").AsStringArray();
                foreach (string animation in animations)
                    PlayAnimation(animation);
            }
        }
    }
}

