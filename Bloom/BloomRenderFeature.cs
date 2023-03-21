using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
        [Range(2, 6)] public int sampleTimes;
    }
    public Settings settings = new Settings();

    BloomPass m_bloomPass;

    public override void Create()
    {
        this.name = "BloomPass";
        if (settings.shader == null)
        {
            Debug.Log("no shader");
            return;
        }
        m_bloomPass = new BloomPass(RenderPassEvent.BeforeRenderingPostProcessing, settings.shader, settings.sampleTimes);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_bloomPass); 
    }
}
