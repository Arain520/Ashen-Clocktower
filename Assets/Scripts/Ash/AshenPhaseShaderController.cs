using System.Collections;
using UnityEngine;

public class AshenPhaseShaderController : MonoBehaviour
{
    public static AshenPhaseShaderController Instance { get; private set; }

    [Header("Shader Filter")]
    [SerializeField] private bool enableShaderFilter = true;
    [SerializeField] private Shader filterShader;
    [SerializeField] private Color filterColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] private float maxIntensity = 0.65f;
    [SerializeField] private float darkness = 0.55f;
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private float cameraDistance = 1f;

    [Header("Particles")]
    [SerializeField] private bool enableAshParticles = true;
    [SerializeField] private ParticleSystem ashParticles;

    private Camera targetCamera;
    private GameObject overlayObject;
    private Material runtimeMaterial;
    private Coroutine fadeCoroutine;

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
        SetFilterIntensity(0f);

        if (ashParticles != null)
        {
            ashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void LateUpdate()
    {
        UpdateOverlaySize();
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

        targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogWarning("场景中没有 MainCamera，无法创建灰烬态 Shader 滤镜。");
            return;
        }

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
        overlayObject.transform.SetParent(targetCamera.transform, false);
        overlayObject.transform.localPosition = new Vector3(0f, 0f, targetCamera.nearClipPlane + cameraDistance);
        overlayObject.transform.localRotation = Quaternion.identity;

        MeshFilter meshFilter = overlayObject.AddComponent<MeshFilter>();
        meshFilter.mesh = CreateQuadMesh();

        MeshRenderer meshRenderer = overlayObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = runtimeMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.sortingOrder = 32767;

        overlayObject.SetActive(false);
        UpdateOverlaySize();
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "AshenPhaseOverlayQuad";
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f),
            new Vector3(0.5f, 0.5f, 0f),
            new Vector3(0.5f, -0.5f, 0f)
        };
        mesh.uv = new[]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        return mesh;
    }

    private void UpdateOverlaySize()
    {
        if (targetCamera == null || overlayObject == null)
            return;

        float height = targetCamera.orthographic
            ? targetCamera.orthographicSize * 2f
            : 2f * (targetCamera.nearClipPlane + cameraDistance) * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        float width = height * targetCamera.aspect;
        overlayObject.transform.localScale = new Vector3(width, height, 1f);
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

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        float targetIntensity = active ? maxIntensity : 0f;
        fadeCoroutine = StartCoroutine(FadeFilterTo(targetIntensity));
    }

    private IEnumerator FadeFilterTo(float targetIntensity)
    {
        float startIntensity = runtimeMaterial.GetFloat(IntensityId);
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
        }
    }

    private void SetFilterIntensity(float intensity)
    {
        if (runtimeMaterial == null)
            return;

        runtimeMaterial.SetColor(FilterColorId, filterColor);
        runtimeMaterial.SetFloat(IntensityId, intensity);
        runtimeMaterial.SetFloat(DarknessId, darkness);
    }

    private void SetParticlesActive(bool active)
    {
        if (!enableAshParticles)
            return;

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
}
