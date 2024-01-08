using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FirecrackerData", order = 1)]
public class FirecrackerData : ScriptableObject
{
    [Header("Prefabs")]
    public GameObject Prefab;
    public GameObject FracturedPrefab;
    public GameObject PlaceholderPrefab;

    [Header("Detonation Settings")]
    [Tooltip("How many Detonations the Firecracker has.")]
    public int DetonationCount;
    [Tooltip("Delays for the Detonations. (e.g: First Element sets the delay for the first detonation, Must be the same as DetonationCount)")]
    public float[] DetonationOffsets;
    [Tooltip("The Detonation Time will be randomly picked inside this range.")]
    public ValueRange<float> DetonationRange;
    [Tooltip("The Explosion Force will be randomly picked inside this range.")]
    public ValueRange<float> ExplosionForceRange;
    [Tooltip("Other Firecrackers inside this radius will be activated at detonation.")]
    public float DetonationRadius;
    [Tooltip("Determines in which layer the detonation range will take effect.")]
    public LayerMask DetonationMask;

    [Header("Audio Settings")]
    [Tooltip("The Sounds for the Detonations. (Must be the same as DetonationCount)")]
    public AudioClip[] DetonationSounds;
    public float[] DetonationVolumes;
    public AudioClip FuseSound;
    public float FuseVolume;

    [Header("Effect Settings")]
    public GameObject[] SparksPrefabs;
    public GameObject[] SmokePrefabs;
    public Vec3Path FusePath;
    [Tooltip("Duration in Percentage of the total detonation time.")]
    public float[] SparksDurations;
    [Tooltip("Duration in Percentage of the total detonation time.")]
    public float[] SmokeDurations;
    public Quaternion[] SparksRotations;
    public Quaternion[] SmokeRotations;
    public float[] SparkOffsets;
    public float[] SmokeOffsets;
}

[System.Serializable]
public class ValueRange<T> where T : struct
{
    public T Min;
    public T Max;
}

[System.Serializable]
public class Vec3Path
{
    public Vector3 From;
    public Vector3 To;
}