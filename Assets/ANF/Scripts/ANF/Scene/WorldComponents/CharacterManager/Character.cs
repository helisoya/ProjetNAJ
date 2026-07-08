using ANF.Utils;
using Leguar.TotalJSON;
using Unity.VisualScripting;
using UnityEngine;

namespace ANF.Scene
{
    /// <summary>
	/// Represents a character in the scene
	/// </summary>
    public class Character : SceneObject
    {
        [Header("Character")]
        [SerializeField] private string characterName;
        [SerializeField] private Animator animator;
        [SerializeField] private float talkingTransitionDuration = 0.1f;
        private LerpInstanceFloat[] lerps;

        public void EditorInit(string characterName, Animator animator, Renderer[] renderers, InteractableObject interactableObject)
        {
            this.characterName = characterName;
            this.animator = animator;
            this.renderers = renderers;
            this.linkedInteraction = interactableObject;
        }

        public override void OnSkipModeToggle(bool enabled)
        {
            if (skipModeEnabled != enabled)
            {
                skipModeEnabled = enabled;
                if (lerpPosition != null && lerpPosition.lerping)
                    lerpPosition.ChangeDuration(0.1f);
                if (lerpRotation != null && lerpRotation.lerping)
                    lerpRotation.ChangeDuration(0.1f);
                if (lerpAlpha != null && lerpAlpha.lerping)
                    lerpAlpha.ChangeDuration(0.1f);
                foreach (LerpInstanceFloat lerp in lerps)
                    if (lerp != null && lerp.lerping)
                        lerp.ChangeDuration(0.1f);
            }
        }

        /// <summary>
        /// Checks if a parameter is in a transition or not (only available for Float parameters)
        /// </summary>
        /// <param name="parameterName">The parameter</param>
        /// <returns>True if the parameter is in a transition</returns>
        public bool IsParameterLerping(string parameterName)
        {
            int index = FindParameterIndex(parameterName);

            if (index != -1 && lerps[index] != null)
                return lerps[index].lerping;
            return false;
        }

        /// <summary>
        /// Finds the index of a specific parameter
        /// </summary>
        /// <param name="parameterName">The parameter's name</param>
        /// <returns>Its index, or -1 if not found</returns>
        private int FindParameterIndex(string parameterName)
        {
            for (int i = 0; i < animator.parameterCount; i++)
                if (animator.parameters[i].name.Equals(parameterName))
                    return i;
            return -1;
        }

        /// <summary>
        /// Sets a trigger in the animator
        /// </summary>
        /// <param name="triggerName">The trigger's name</param>
        public void SetTrigger(string triggerName)
        {
            animator.SetTrigger(triggerName);
        }

        /// <summary>
        /// Sets an integer value in the animator
        /// </summary>
        /// <param name="parameterName">The parameter's name</param>
        /// <param name="value">The new value</param>
        public void SetInteger(string parameterName, int value)
        {
            animator.SetInteger(parameterName, value);
        }

        /// <summary>
        /// Sets an bool value in the animator
        /// </summary>
        /// <param name="parameterName">The parameter's name</param>
        /// <param name="value">The new value</param>
        public void SetBool(string parameterName, bool value)
        {
            animator.SetBool(parameterName, value);
        }

        /// <summary>
        /// Sets a float value in the animator
        /// </summary>
        /// <param name="parameterName">The parameter's name</param>
        /// <param name="value">The new value</param>
        /// <param name="immediate">True if the change is immediate</param>
        /// <param name="transitionDuration">The transition duration if not immediate</param>
        public void SetFloat(string parameterName, float value, bool immediate = true, float transitionDuration = 0.5f)
        {
            int index = FindParameterIndex(parameterName);
            if (index == -1)
                return;

            if (immediate)
            {
                animator.SetFloat(parameterName, value);

                if (lerps[index].lerping)
                    lerps[index].StopLerp();
            }
            else
            {
                lerps[index].StartLerp(animator.GetFloat(parameterName), value, skipModeEnabled ? 0.1f : transitionDuration);
            }
        }

        /// <summary>
        /// Changes if the character is talking or not
        /// </summary>
        /// <param name="isTalking">True if talking</param>
        public void SetIsTalking(bool isTalking)
        {
            SetFloat("Talking", isTalking ? 1.0f : 0.0f, false, talkingTransitionDuration);
        }

        /// <summary>
        /// Changes the current body animation
        /// </summary>
        /// <param name="stateName">The next animator's state</param>
        /// <param name="immediate">True if the change should be immediate</param>
        /// <param name="transitionTime">The transition time if not immediate</param>
        public void ChangeBodyAnimation(string stateName, bool immediate = false, float transitionTime = 0.25f)
        {
            if (!immediate)
                animator.CrossFade(stateName, transitionTime, 0);
            else
                animator.Play(stateName, 0);
        }

        /// <summary>
        /// Changes the current eye animation
        /// </summary>
        /// <param name="stateName">The next animator's state</param>
        /// <param name="immediate">True if the change should be immediate</param>
        /// <param name="transitionTime">The transition time if not immediate</param>
        public void ChangeEyeAnimation(string stateName, bool immediate = false, float transitionTime = 0.25f)
        {
            if (!immediate)
                animator.CrossFade(stateName, transitionTime, 1);
            else
                animator.Play(stateName, 1);
        }

        /// <summary>
        /// Changes the current mouth animation
        /// </summary>
        /// <param name="stateName">The next animator's state</param>
        /// <param name="immediate">True if the change should be immediate</param>
        /// <param name="transitionTime">The transition time if not immediate</param>
        public void ChangeMouthAnimation(string stateName, bool immediate = false, float transitionTime = 0.25f)
        {
            if (!immediate)
                animator.CrossFade(stateName, transitionTime, 2);
            else
                animator.Play(stateName, 2);
        }

        /// <summary>
        /// Gets the character's animator
        /// </summary>
        /// <returns>The character's animator</returns>
        public Animator GetAnimator()
        {
            return animator;
        }

        /// <summary>
        /// Gets the character's name
        /// </summary>
        /// <returns>The character's name</returns>
        public string GetCharacterName()
        {
            return characterName;
        }

        protected override void OnCreate(ANFManager manager)
        {
            lerps = new LerpInstanceFloat[animator.parameterCount];
            for (int i = 0; i < animator.parameterCount; i++)
            {
                if (animator.parameters[i].type == AnimatorControllerParameterType.Float)
                    lerps[i] = new LerpInstanceFloat();
            }
        }

        protected override void OnRemove(ANFManager manager)
        {
        }

        protected override void OnUpdate(ANFManager manager)
        {
            for (int i = 0; i < lerps.Length; i++)
            {
                if (lerps[i] != null && lerps[i].lerping)
                {
                    animator.SetFloat(animator.parameters[i].nameHash, lerps[i].Update());
                }
            }
        }


        protected override void OnSave(JSON json)
        {
            JArray lerpsJSON = new JArray();
            for (int i = 0; i < lerps.Length; i++)
            {
                if (lerps[i] != null)
                {
                    JSON jsonLerp = new JSON();
                    lerps[i].Save(jsonLerp);
                    lerpsJSON.Add(jsonLerp);
                }
                else
                {
                    lerpsJSON.Add(false); // To preserve length
                }
            }
            json.Add("lerps", lerpsJSON);

            if (animator.IsInTransition(0))
                json.Add("currentBody", animator.GetNextAnimatorStateInfo(0).shortNameHash);
            else
                json.Add("currentBody", animator.GetCurrentAnimatorStateInfo(0).shortNameHash);

            if (animator.IsInTransition(1))
                json.Add("currentEye", animator.GetNextAnimatorStateInfo(1).shortNameHash);
            else
                json.Add("currentEye", animator.GetCurrentAnimatorStateInfo(1).shortNameHash);

            if (animator.IsInTransition(2))
                json.Add("currentMouth", animator.GetNextAnimatorStateInfo(2).shortNameHash);
            else
                json.Add("currentMouth", animator.GetCurrentAnimatorStateInfo(2).shortNameHash);

            JArray parametersArray = new JArray();
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Float)
                    parametersArray.Add(animator.GetFloat(parameter.nameHash));
                else if (parameter.type == AnimatorControllerParameterType.Int)
                    parametersArray.Add(animator.GetInteger(parameter.nameHash));
                else if (parameter.type == AnimatorControllerParameterType.Bool)
                    parametersArray.Add(animator.GetBool(parameter.nameHash));
                else if (parameter.type == AnimatorControllerParameterType.Trigger)
                    parametersArray.Add(false); // To preserve the array's length
            }
            json.Add("parameters", parametersArray);
        }

        protected override void OnLoad(JSON json)
        {
            if (json.ContainsKey("parameters"))
            {
                JArray parametersArray = json.GetJArray("parameters");

                for (int i = 0; i < animator.parameterCount; i++)
                {
                    AnimatorControllerParameter parameter = animator.parameters[i];
                    if (parameter.type == AnimatorControllerParameterType.Float)
                        animator.SetFloat(parameter.nameHash, parametersArray.GetFloat(i));
                    else if (parameter.type == AnimatorControllerParameterType.Int)
                        animator.SetInteger(parameter.nameHash, parametersArray.GetInt(i));
                    else if (parameter.type == AnimatorControllerParameterType.Bool)
                        animator.SetBool(parameter.nameHash, parametersArray.GetBool(i));
                }
            }

            if (json.ContainsKey("lerps"))
            {
                JArray lerpsArray = json.GetJArray("lerps");

                for (int i = 0; i < lerps.Length; i++)
                {
                    if (lerpsArray.Length <= i)
                        break;

                    if (lerps[i] == null)
                    {
                        if (animator.parameters[i].type == AnimatorControllerParameterType.Float)
                            lerps[i] = new LerpInstanceFloat();
                    }

                    if (lerps[i] != null)
                        lerps[i].Load(lerpsArray.GetJSON(i));
                }
            }

            if (json.ContainsKey("currentBody"))
                animator.Play(json.GetInt("currentBody"), 0);
            if (json.ContainsKey("currentEye"))
                animator.Play(json.GetInt("currentEye"), 1);
            if (json.ContainsKey("currentMouth"))
                animator.Play(json.GetInt("currentMouth"), 2);
        }
    }
}

