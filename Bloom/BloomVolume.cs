using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("COD_Bloom")]
public class BloomVolume : VolumeComponent,IPostProcessComponent
{
    [Tooltip("Enable Cloud")]
    public BoolParameter enableCloud = new BoolParameter(true);

    public ClampedFloatParameter _luminanceThreshole = new ClampedFloatParameter(0.7f, 0.0f, 1.0f);

    public ClampedIntParameter downSampleBlurSize = new ClampedIntParameter(8, 3, 15);
    public ClampedFloatParameter downSampleBlurSigma = new ClampedFloatParameter(0.2f, 0.01f, 10.0f);

    public ClampedIntParameter _upSampleBlurSize = new ClampedIntParameter(8, 3, 15);
    public ClampedFloatParameter _upSampleBlurSigma = new ClampedFloatParameter(0.2f, 0.01f, 10.0f);

    public ClampedFloatParameter _bloomIntensity = new ClampedFloatParameter(1.0f, 0.001f, 10.0f);

    [Range(0.001f, 10.0f)] public float bloomIntensity = 1.0f;

    public void load(Material material, ref RenderingData renderingData)
    {
        material.SetFloat("_luminanceThreshole", _luminanceThreshole.value);
        material.SetInt("_downSampleBlurSize", downSampleBlurSize.value);
        material.SetFloat("_downSampleBlurSigma", downSampleBlurSigma.value);

        material.SetInt("_upSampleBlurSize", _upSampleBlurSize.value);
        material.SetFloat("_upSampleBlurSigma", _upSampleBlurSigma.value);

        material.SetFloat("_bloomIntensity", _bloomIntensity.value);
    }

    public bool IsActive() => enableCloud == true;
    public bool IsTileCompatible() => false;
}
