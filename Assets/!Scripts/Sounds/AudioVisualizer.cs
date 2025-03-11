using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class AudioVisualizer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private float bounceScale = 100f; // Sensitivity for audio response
    [SerializeField] private float minScale = 0.5f; // Minimum scale to ensure letters don't disappear
    [SerializeField] private float maxScale = 2f; // Maximum scale for bounce effect
    [SerializeField] private float smoothSpeed = 10f; // How quickly the bounce smooths out
    [SerializeField] private int spectrumSamples = 512; // Number of spectrum samples (power of 2)
    [SerializeField] private float highFrequencyBoost = 2f; // Multiplier for higher frequencies

    // fields for bulk low-frequency objects
    [Header("Bulk Low Frequency Objects")]
    [SerializeField] private string lowFrequencyObjectTag = "LowFrequencyObject"; // Tag to identify low-frequency objects
    [SerializeField] private bool useTagForBulkAssignment = true; // Toggle to enable bulk assignment via tag
    [SerializeField] private float lowFrequencyScale = 100f; // Sensitivity for low-frequency response
    [SerializeField] private float lowFrequencyMinScale = 0.5f; // Minimum scale for low-frequency objects
    [SerializeField] private float lowFrequencyMaxScale = 2f; // Maximum scale for low-frequency objects
    [SerializeField] private float lowFrequencySmoothSpeed = 10f; // Smoothing speed for low-frequency objects
    [SerializeField] private float lowFrequencyRange = 0.1f; // Percentage of spectrum to use for low frequencies (0 to 1)
    [SerializeField] private EffectType bulkEffectType = EffectType.Scale; // Default effect type for bulk objects
    [SerializeField] private float bulkEffectStrength = 1f; // Default effect strength for bulk objects

    [SerializeField] private LowFrequencyObject[] individualLowFrequencyObjects;

    public enum EffectType
    {
        Scale,          // Scale the object
        VerticalMove,   // Move the object up and down
        Rotate          // Rotate the object
    }

    [System.Serializable]
    public class LowFrequencyObject
    {
        public GameObject targetObject;
        public EffectType effectType;
        [Tooltip("For movement/rotation: Maximum displacement in units/degrees")]
        public float effectStrength = 1f; // Strength of the effect (e.g., max movement distance or rotation angle)
    }

    private float[] spectrumData; // Array to hold spectrum data
    private float[] currentScales; // Current scale values for each letter
    private float[] targetScales; // Target scale values for each letter
    private int[] frequencyBands; // Which frequency band each letter reacts to
    private float[] bandMaxima; // Maximum intensity seen in each band for normalization

    // Variables for low-frequency objects
    private List<LowFrequencyObject> allLowFrequencyObjects;
    private float[] lowFrequencyCurrentValues;
    private float[] lowFrequencyTargetValues;
    private Vector3[] lowFrequencyOriginalPositions;
    private Quaternion[] lowFrequencyOriginalRotations;

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not assigned in AudioVisualizer!");
            return;
        }

        if (titleText == null)
        {
            Debug.LogError("Title Text (TMP) not assigned in AudioVisualizer!");
            return;
        }

        spectrumData = new float[spectrumSamples];

        titleText.ForceMeshUpdate();

        InitializeArrays();

        InitializeLowFrequencyObjects();

    }

    private void InitializeArrays()
    {
        var charInfo = titleText.textInfo.characterInfo;
        int numLetters = charInfo.Length;

        if (numLetters == 0)
        {
            Debug.LogWarning("No characters found in TMP text!");
            return;
        }

        currentScales = new float[numLetters];
        targetScales = new float[numLetters];
        frequencyBands = new int[numLetters];
        bandMaxima = new float[numLetters];

        float logMax = Mathf.Log(spectrumSamples, 2);
        for (int i = 0; i < numLetters; i++)
        {
            currentScales[i] = 1f;
            targetScales[i] = 1f;

            float normalizedIndex = (float)i / (numLetters - 1);
            float logIndex = Mathf.Pow(2, normalizedIndex * logMax);
            frequencyBands[i] = Mathf.FloorToInt(logIndex) - 1;

            frequencyBands[i] = Mathf.Clamp(frequencyBands[i], 0, spectrumSamples - 1);

            bandMaxima[i] = 0.001f;
        }
    }

    private void InitializeLowFrequencyObjects()
    {
        allLowFrequencyObjects = new List<LowFrequencyObject>();

        if (useTagForBulkAssignment)
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(lowFrequencyObjectTag);
            foreach (GameObject obj in taggedObjects)
            {
                allLowFrequencyObjects.Add(new LowFrequencyObject
                {
                    targetObject = obj,
                    effectType = bulkEffectType,
                    effectStrength = bulkEffectStrength
                });
            }
        }

        if (individualLowFrequencyObjects != null)
        {
            foreach (var obj in individualLowFrequencyObjects)
            {
                if (obj.targetObject != null)
                {
                    allLowFrequencyObjects.Add(obj);
                }
            }
        }

        if (allLowFrequencyObjects.Count == 0)
        {
            Debug.LogWarning("No low-frequency objects found or assigned!");
            return;
        }

        lowFrequencyCurrentValues = new float[allLowFrequencyObjects.Count];
        lowFrequencyTargetValues = new float[allLowFrequencyObjects.Count];
        lowFrequencyOriginalPositions = new Vector3[allLowFrequencyObjects.Count];
        lowFrequencyOriginalRotations = new Quaternion[allLowFrequencyObjects.Count];

        for (int i = 0; i < allLowFrequencyObjects.Count; i++)
        {
            if (allLowFrequencyObjects[i].targetObject == null)
            {
                Debug.LogWarning($"Low-frequency object {i} has no target object assigned!");
                continue;
            }

            lowFrequencyCurrentValues[i] = 0f;
            lowFrequencyTargetValues[i] = 0f;
            lowFrequencyOriginalPositions[i] = allLowFrequencyObjects[i].targetObject.transform.position;
            lowFrequencyOriginalRotations[i] = allLowFrequencyObjects[i].targetObject.transform.rotation;
        }
    }

    private void Update()
    {
        if (audioSource == null || titleText == null) return;

        if (!audioSource.isPlaying)
        {
            Debug.LogWarning("AudioSource is not playing!");
            return;
        }

        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        titleText.ForceMeshUpdate();
        var charInfo = titleText.textInfo.characterInfo;

        if (charInfo.Length == 0)
        {
            Debug.LogWarning("No characters to process in TMP text!");
            return;
        }

        bool anyLetterMovement = false;
        for (int i = 0; i < charInfo.Length; i++)
        {
            if (!charInfo[i].isVisible) continue; // Skip invisible characters

            float rawIntensity = spectrumData[frequencyBands[i]];

            bandMaxima[i] = Mathf.Max(bandMaxima[i], rawIntensity);

            float normalizedIntensity = rawIntensity / bandMaxima[i];

            float frequencyBoost = Mathf.Lerp(1f, highFrequencyBoost, (float)i / (charInfo.Length - 1));

            float intensity = normalizedIntensity * bounceScale * frequencyBoost;

            targetScales[i] = Mathf.Clamp(1f + intensity, minScale, maxScale);

            currentScales[i] = Mathf.Lerp(currentScales[i], targetScales[i], Time.deltaTime * smoothSpeed);

            ScaleCharacter(i, currentScales[i]);

            if (Mathf.Abs(currentScales[i] - 1f) > 0.01f)
            {
                anyLetterMovement = true;
            }
        }

        if (!anyLetterMovement)
        {
            Debug.LogWarning("No noticeable letter movement detected. Try increasing bounceScale or highFrequencyBoost.");
        }

        titleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        ProcessLowFrequencyObjects();
    }

    private void ProcessLowFrequencyObjects()
    {
        if (allLowFrequencyObjects == null || allLowFrequencyObjects.Count == 0) return;

        int lowFrequencySampleCount = Mathf.FloorToInt(spectrumSamples * lowFrequencyRange);
        float lowFrequencyRawIntensity = 0f;
        float lowFrequencyMaxIntensity = 0.001f;

        for (int i = 0; i < lowFrequencySampleCount; i++)
        {
            lowFrequencyRawIntensity += spectrumData[i];
            lowFrequencyMaxIntensity = Mathf.Max(lowFrequencyMaxIntensity, spectrumData[i]);
        }
        lowFrequencyRawIntensity /= lowFrequencySampleCount;

        float lowFrequencyNormalizedIntensity = lowFrequencyRawIntensity / lowFrequencyMaxIntensity;

        float lowFrequencyIntensity = lowFrequencyNormalizedIntensity * lowFrequencyScale;

        bool anyObjectMovement = false;
        for (int i = 0; i < allLowFrequencyObjects.Count; i++)
        {
            if (allLowFrequencyObjects[i].targetObject == null) continue;

            lowFrequencyTargetValues[i] = Mathf.Clamp(lowFrequencyIntensity, lowFrequencyMinScale, lowFrequencyMaxScale);

            lowFrequencyCurrentValues[i] = Mathf.Lerp(lowFrequencyCurrentValues[i], lowFrequencyTargetValues[i], Time.deltaTime * lowFrequencySmoothSpeed);

            switch (allLowFrequencyObjects[i].effectType)
            {
                case EffectType.Scale:
                    allLowFrequencyObjects[i].targetObject.transform.localScale = Vector3.one * lowFrequencyCurrentValues[i];
                    break;
                case EffectType.VerticalMove:
                    Vector3 movePosition = lowFrequencyOriginalPositions[i] + Vector3.up * (lowFrequencyCurrentValues[i] - 1f) * allLowFrequencyObjects[i].effectStrength;
                    allLowFrequencyObjects[i].targetObject.transform.position = movePosition;
                    break;
                case EffectType.Rotate:
                    Quaternion rotateRotation = lowFrequencyOriginalRotations[i] * Quaternion.Euler(0f, 0f, (lowFrequencyCurrentValues[i] - 1f) * allLowFrequencyObjects[i].effectStrength);
                    allLowFrequencyObjects[i].targetObject.transform.rotation = rotateRotation;
                    break;
            }

            if (Mathf.Abs(lowFrequencyCurrentValues[i] - 1f) > 0.01f) // Check if there's noticeable movement
            {
                anyObjectMovement = true;
            }
        }

        if (!anyObjectMovement)
        {
            Debug.LogWarning("No noticeable low-frequency object movement detected. Try increasing lowFrequencyScale or lowFrequencyRange.");
        }
    }

    private void ScaleCharacter(int charIndex, float scale)
    {
        var textInfo = titleText.textInfo;
        if (charIndex >= textInfo.characterCount || !textInfo.characterInfo[charIndex].isVisible) return;

        var charInfo = textInfo.characterInfo[charIndex];
        int materialIndex = charInfo.materialReferenceIndex;
        int vertexIndex = charInfo.vertexIndex;

        var meshInfo = textInfo.meshInfo[materialIndex];
        var vertices = meshInfo.vertices;

        Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 1] + vertices[vertexIndex + 2] + vertices[vertexIndex + 3]) / 4f;

        for (int j = 0; j < 4; j++)
        {
            Vector3 vertex = vertices[vertexIndex + j];
            Vector3 offset = vertex - center;
            vertices[vertexIndex + j] = center + offset * scale;
        }
    }
    public void SetTitleText(string newTitle)
    {
        if (titleText != null)
        {
            titleText.text = newTitle;
            titleText.ForceMeshUpdate();
            InitializeArrays();
        }
    }
}