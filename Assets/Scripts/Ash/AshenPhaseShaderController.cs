using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AshenPhaseShaderController : MonoBehaviour
{
    public static AshenPhaseShaderController Instance { get; private set; }

    [Header("Shader Filter")]
    [SerializeField] private bool enableShaderFilter = true;
    [SerializeField] private Shader filterShader;
    [SerializeField] private Color filterColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] private float maxIntensity = 0.28f;
    [SerializeField, Range(0f, 1f)] private float filterOpacityMultiplier = 0.35f;
    [SerializeField] private float darkness = 0.22f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private int overlaySortingOrder = short.MaxValue;

    [Header("Particles")]
    [SerializeField] private bool enableAshParticles = true;
    [SerializeField] private bool createParticlesIfMissing = true;
    [SerializeField] private ParticleSystem ashParticles;
    [SerializeField] private Vector2 particleSizeRange = new Vector2(0.025f, 0.07f);
    [SerializeField] private Vector2 particleSpeedRange = new Vector2(0.08f, 0.35f);
    [SerializeField] private float particleRandomDrift = 0.45f;
    [SerializeField] private float particleNoiseStrength = 0.35f;

    private GameObject overlayObject;
    private Material runtimeMaterial;
    private Coroutine fadeCoroutine;
    private bool filterTargetActive;
    private float currentFilterIntensity;

    private static readonly int FilterColorId = Shader.PropertyToID("_FilterColor");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int DarknessId = Shader.PropertyToID("_Darkness");

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("场景中存在多个 AshenPhaseShaderController，请确认只保留一个。");
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SetupOverlay();
        SetupAshParticles();
        SetFilterIntensity(0f);

        if (ashParticles != null)
        {
            ashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void LateUpdate()
    {
        KeepOverlayInFront();

        if (filterTargetActive && fadeCoroutine == null)
        {
            currentFilterIntensity = maxIntensity;
        }

        ApplyFilterProperties();
    }

    public void SetAshenVisualActive(bool active)
    {
        SetFilterActive(active);
        SetParticlesActive(active);
    }

    private void SetupOverlay()
    {
        if (!enableShaderFilter)
            return;

        if (filterShader == null)
        {
            filterShader = Shader.Find("AshenClocktower/AshenPhaseFilter");
        }

        if (filterShader == null)
        {
            Debug.LogWarning("未找到 AshenClocktower/AshenPhaseFilter Shader，灰烬态 Shader 滤镜不会显示。");
            return;
        }

        runtimeMaterial = new Material(filterShader);
        runtimeMaterial.renderQueue = 5000;

        overlayObject = new GameObject("AshenPhaseShaderOverlay");

        Canvas canvas = overlayObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = overlaySortingOrder;

        CanvasScaler canvasScaler = overlayObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasScaler.matchWidthOrHeight = 0.5f;

        GameObject imageObject = new GameObject("FilterImage");
        imageObject.transform.SetParent(overlayObject.transform, false);

        Image image = imageObject.AddComponent<Image>();
        image.material = runtimeMaterial;
        image.raycastTarget = false;

        RectTransform imageRect = image.rectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        overlayObject.SetActive(false);
        KeepOverlayInFront();
    }

    private void KeepOverlayInFront()
    {
        if (overlayObject == null)
            return;

        Canvas canvas = overlayObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = overlaySortingOrder;
        }

        overlayObject.transform.SetAsLastSibling();
    }

    private void SetFilterActive(bool active)
    {
        if (!enableShaderFilter)
            return;

        if (overlayObject == null || runtimeMaterial == null)
        {
            SetupOverlay();
        }

        if (overlayObject == null || runtimeMaterial == null)
            return;

        overlayObject.SetActive(true);
        filterTargetActive = active;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        float targetIntensity = active ? maxIntensity : 0f;
        fadeCoroutine = StartCoroutine(FadeFilterTo(targetIntensity));
    }

    private IEnumerator FadeFilterTo(float targetIntensity)
    {
        float startIntensity = currentFilterIntensity;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = fadeDuration <= 0 ? 1f : timer / fadeDuration;
            SetFilterIntensity(Mathf.Lerp(startIntensity, targetIntensity, progress));
            yield return null;
        }

        SetFilterIntensity(targetIntensity);

        if (Mathf.Approximately(targetIntensity, 0f) && overlayObject != null)
        {
            overlayObject.SetActive(false);
            filterTargetActive = false;
        }
    }

    private void SetFilterIntensity(float intensity)
    {
        if (runtimeMaterial == null)
            return;

        currentFilterIntensity = intensity;
        ApplyFilterProperties();
    }

    private void ApplyFilterProperties()
    {
        if (runtimeMaterial == null)
            return;

        runtimeMaterial.SetColor(FilterColorId, filterColor);
        runtimeMaterial.SetFloat(IntensityId, currentFilterIntensity * filterOpacityMultiplier);
        runtimeMaterial.SetFloat(DarknessId, darkness);
    }

    private void SetParticlesActive(bool active)
    {
        if (!enableAshParticles)
            return;

        SetupAshParticles();

        if (ashParticles == null)
        {
            Debug.LogWarning("AshenPhaseShaderController 没有绑定 ashParticles，无法播放灰烬粒子。");
            return;
        }

        if (active)
        {
            ashParticles.Play();
        }
        else
        {
            ashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }

    private void SetupAshParticles()
    {
        if (!enableAshParticles)
            return;

        if (ashParticles == null && createParticlesIfMissing)
        {
            Transform parent = transform;
            Camera mainCamera = Camera.main;

            if (mainCamera != null)
            {
                parent = mainCamera.transform;
            }

            GameObject particleObject = new GameObject("AshenPhaseParticles");
            particleObject.transform.SetParent(parent, false);
            particleObject.transform.localPosition = new Vector3(0f, 0f, 1f);
            ashParticles = particleObject.AddComponent<ParticleSystem>();
        }

        if (ashParticles == null)
            return;

        ParticleSystem.MainModule main = ashParticles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.8f, 3.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeedRange.x, particleSpeedRange.y);
        main.startSize = new ParticleSystem.MinMaxCurve(particleSizeRange.x, particleSizeRange.y);
        main.startRotation = new ParticleSystem.MinMaxCurve(-Mathf.PI, Mathf.PI);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = ashParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 40f;

        ParticleSystem.ShapeModule shape = ashParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 10f, 0.1f);

        ParticleSystem.VelocityOverLifetimeModule velocity = ashParticles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-particleRandomDrift, particleRandomDrift);
        velocity.y = new ParticleSystem.MinMaxCurve(-particleRandomDrift * 0.4f, particleRandomDrift);
        velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ParticleSystem.NoiseModule noise = ashParticles.noise;
        noise.enabled = true;
        noise.strength = particleNoiseStrength;
        noise.frequency = 0.55f;
        noise.scrollSpeed = 0.4f;
        noise.damping = true;

        ParticleSystemRenderer particleRenderer = ashParticles.GetComponent<ParticleSystemRenderer>();
        if (particleRenderer != null)
        {
            particleRenderer.sortingOrder = overlaySortingOrder - 1;
        }
    }
}
