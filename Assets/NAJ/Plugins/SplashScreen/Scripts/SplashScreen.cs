using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CaribouProd.General
{
    /// <summary>
    /// Handles the Voxel Caribou Splash Screen
    /// </summary>
    public class SplashScreen : MonoBehaviour
    {
        [Header("Splash Screen")]
        [SerializeField] private InputActionReference skipAction;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform logoRoot;
        [SerializeField] private Transform caribouRoot;
        [SerializeField] private Renderer skyboxRenderer;
        [SerializeField] private Renderer baseCaribouRenderer;
        [SerializeField] private Renderer wireframeCaribouRenderer;
        [SerializeField] private string nextScene;

        [Header("Audio")]
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip caribouSound;
        [SerializeField] private AudioClip printerSound;
        [SerializeField] private AudioClip fallSound;

        private Sequence sequence;

        public enum SoundType
        {
            CARIBOU,
            CAR,
            IMPACT
        }

        void Awake()
        {
            canvasGroup.alpha = 1.0f;
        }

        void Start()
        {
            MaterialPropertyBlock wireFrameBlock = new MaterialPropertyBlock();
            MaterialPropertyBlock baseBlock = new MaterialPropertyBlock();
            MaterialPropertyBlock skyboxBlock = new MaterialPropertyBlock();

            wireFrameBlock.SetFloat("_WireframeScale", 0.5f);
            wireFrameBlock.SetFloat("_MaxHeight", 1.0f);
            wireframeCaribouRenderer.SetPropertyBlock(wireFrameBlock);

            baseBlock.SetFloat("_MaxHeight", 5.0f);
            baseCaribouRenderer.SetPropertyBlock(baseBlock);

            skyboxBlock.SetFloat("_MaxHeight", -15.0f);
            skyboxRenderer.SetPropertyBlock(skyboxBlock);

            caribouRoot.position = new Vector3(0.6f, 9.0f, -0.3f);
            caribouRoot.eulerAngles = new Vector3(0, -140, 0);
            caribouRoot.localScale = new Vector3(0.9f, 0.9f, 0.9f);


            logoRoot.position = new Vector3(-3.99f, -12f, 4.7f);
            logoRoot.eulerAngles = Vector3.zero;


            sequence = DOTween.Sequence();
            sequence.Insert(0.0f, canvasGroup.DOFade(0, 0.5f).SetEase(Ease.OutQuad))
            .Insert(1.0f, caribouRoot.DOMove(new Vector3(0.6f, -2.5f, -0.3f), 1.5f).SetEase(Ease.OutBounce))
            .InsertCallback(1.5f, () => { Play2DSound(fallSound); })
            .InsertCallback(2.5f, () => { Play2DSound(printerSound); })
            .Insert(2.5f, DOVirtual.Float(5.0f, -3.0f, 3.0f, (float value) =>
            {
                baseBlock.SetFloat("_MaxHeight", value);
                baseCaribouRenderer.SetPropertyBlock(baseBlock);
            }).SetEase(Ease.OutQuad))
            .Insert(2.5f, DOVirtual.Float(-15.0f, 15.0f, 3.0f, (float value) =>
            {
                skyboxBlock.SetFloat("_MaxHeight", value);
                skyboxRenderer.SetPropertyBlock(skyboxBlock);
            }).SetEase(Ease.OutQuad))
            .Insert(2.5f, caribouRoot.DORotate(new Vector3(0, 360.0f, 0), 3, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.OutQuad))
            .Insert(2.5f, DOVirtual.Float(1.0f, -1.0f, 3.0f, (float value) =>
            {
                wireFrameBlock.SetFloat("_MaxHeight", value);
                wireframeCaribouRenderer.SetPropertyBlock(wireFrameBlock);
            }).SetEase(Ease.OutQuad))
            .Insert(5.5f, caribouRoot.DOMove(new Vector3(0.6f, -0.55f, 3.0f), 2.0f).SetEase(Ease.OutQuad))
            .Insert(5.5f, logoRoot.DOMove(new Vector3(-3.99f, -5f, 4.7f), 2.0f).SetEase(Ease.OutQuad))
            .Insert(7.5f, caribouRoot.DOPunchScale(new Vector3(-0.05f, -0.05f, -0.1f), 0.5f, 1))
            .Insert(7.5f, logoRoot.DORotate(new Vector3(0, 0, -2.5f), 0.5f).SetEase(Ease.OutBounce))
            .InsertCallback(7.5f, () => { Play2DSound(caribouSound); })
            .Insert(12.0f, canvasGroup.DOFade(1, 0.5f).SetEase(Ease.OutQuad))
            .OnComplete(() => { EndSplashScreen(); });

        }


        /// <summary>
        /// Ends the splash screen
        /// </summary>
        public void EndSplashScreen()
        {

            if (sequence.IsActive())
                sequence.Kill();
            SceneManager.LoadScene(nextScene);
        }

        /// <summary>
        /// Play a 2D Sound
        /// </summary>
        /// <param name="clip">The clip</param>
        public void Play2DSound(AudioClip clip)
        {
            source.PlayOneShot(clip);
        }

        void Update()
        {
            if (skipAction.action.triggered)
                EndSplashScreen();
        }
    }
}