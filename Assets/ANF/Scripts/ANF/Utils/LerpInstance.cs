using Leguar.TotalJSON;
using UnityEngine;

namespace ANF.Utils
{
    /// <summary>
	/// Represents an instance of a lerp
	/// </summary>
    public abstract class LerpInstance<Type>
    {
        protected Type start;
        protected float t;
        protected Type target;
        protected float transitionDuration;

        public bool lerping { get; protected set; }

        /// <summary>
		/// Starts a new lerp
		/// </summary>
		/// <param name="startValue">The start value</param>
		/// <param name="targetValue">The target value</param>
		/// <param name="duration">The transition duration</param>
        public void StartLerp(Type startValue, Type targetValue, float duration)
        {
            lerping = true;
            t = 0;
            start = startValue;
            target = targetValue;
            transitionDuration = duration;
        }

        /// <summary>
		/// Changes the duration of the lerp (not recommanded)
		/// </summary>
		/// <param name="duration">The new duration</param>
        public void ChangeDuration(float duration)
        {
            transitionDuration = duration;
        }

        /// <summary>
        /// Stops the lerp
        /// </summary>
        public void StopLerp()
        {
            lerping = false;
        }

        /// <summary>
        /// Gets the current value
        /// </summary>
        /// <returns>The current value</returns>
        public abstract Type Get();

        /// <summary>
        /// Updates the lerp
        /// </summary>
        /// <returns>The updated value</returns>
        public abstract Type Update();

        public void Save(JSON json)
        {
            json.Add("lerping", lerping);

            json.Add("start", start);
            json.Add("t", t);
            json.Add("target", target);
            json.Add("transitionDuration", transitionDuration);
        }

        public abstract void Load(JSON json);
    }

    /// <summary>
	/// Represents a lerp instance for a float
	/// </summary>
    public class LerpInstanceFloat : LerpInstance<float>
    {
        public override void Load(JSON json)
        {
            if (json.ContainsKey("lerping"))
                lerping = json.GetBool("lerping");
            if (json.ContainsKey("target"))
                target = json.GetFloat("target");
            if (json.ContainsKey("start"))
                start = json.GetFloat("start");
            if (json.ContainsKey("t"))
                t = json.GetFloat("t");
            if (json.ContainsKey("transitionDuration"))
                transitionDuration = json.GetFloat("transitionDuration");
        }

        public override float Get()
        {
            return Mathf.Lerp(start, target, t);
        }

        public override float Update()
        {
            float result = 1.0f;
            if (lerping)
            {
                t += Time.deltaTime / transitionDuration;
                result = Get();

                if (t >= 1.0f)
                    lerping = false;
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a lerp instance for a vector2
    /// </summary>
    public class LerpInstanceVector2 : LerpInstance<Vector2>
    {
        public override void Load(JSON json)
        {
            if (json.ContainsKey("lerping"))
                lerping = json.GetBool("lerping");
            if (json.ContainsKey("target"))
                target = json.GetJArray("target").AsVector2();
            if (json.ContainsKey("start"))
                start = json.GetJArray("start").AsVector2();
            if (json.ContainsKey("t"))
                t = json.GetFloat("t");
            if (json.ContainsKey("transitionDuration"))
                transitionDuration = json.GetFloat("transitionDuration");
        }

        public override Vector2 Get()
        {
            return Vector2.Lerp(start, target, t);
        }

        public override Vector2 Update()
        {
            Vector2 result = Vector2.one;
            if (lerping)
            {
                t += Time.deltaTime / transitionDuration;
                result = Get();

                if (t >= 1.0f)
                    lerping = false;
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a lerp instance for a vector3
    /// </summary>
    public class LerpInstanceVector3 : LerpInstance<Vector3>
    {
        public override void Load(JSON json)
        {
            if (json.ContainsKey("lerping"))
                lerping = json.GetBool("lerping");
            if (json.ContainsKey("target"))
                target = json.GetJArray("target").AsVector3();
            if (json.ContainsKey("start"))
                start = json.GetJArray("start").AsVector3();
            if (json.ContainsKey("t"))
                t = json.GetFloat("t");
            if (json.ContainsKey("transitionDuration"))
                transitionDuration = json.GetFloat("transitionDuration");
        }

        public override Vector3 Get()
        {
            return Vector3.Lerp(start, target, t);
        }

        public override Vector3 Update()
        {
            Vector3 result = Vector3.one;
            if (lerping)
            {
                t += Time.deltaTime / transitionDuration;
                result = Get();

                if (t >= 1.0f)
                    lerping = false;
            }

            return result;
        }
    }

    /// <summary>
    /// Represents a lerp instance for a color
    /// </summary>
    public class LerpInstanceColor : LerpInstance<Color>
    {
        public override void Load(JSON json)
        {
            if (json.ContainsKey("lerping"))
                lerping = json.GetBool("lerping");
            if (json.ContainsKey("target"))
                target = json.GetJArray("target").AsColor();
            if (json.ContainsKey("start"))
                start = json.GetJArray("start").AsColor();
            if (json.ContainsKey("t"))
                t = json.GetFloat("t");
            if (json.ContainsKey("transitionDuration"))
                transitionDuration = json.GetFloat("transitionDuration");
        }

        public override Color Get()
        {
            return Color.Lerp(start, target, t);
        }

        public override Color Update()
        {
            Color result = Color.white;
            if (lerping)
            {
                t += Time.deltaTime / transitionDuration;
                result = Get();

                if (t >= 1.0f)
                    lerping = false;
            }

            return result;
        }
    }
}

