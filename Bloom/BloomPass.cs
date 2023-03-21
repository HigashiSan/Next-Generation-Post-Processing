using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BloomPass : ScriptableRenderPass
{
    static readonly string renderPassTag = "Bloom";

    private BloomVolume bloomVolume;
    Material bloomMaterial;

    private int testRTID;
    int sampleTimes;

    public BloomPass(RenderPassEvent evt, Shader bloomshader, int sampleTimes)
    {
        renderPassEvent = evt;
        var shader = bloomshader;
        if (shader == null)
        {
            Debug.LogError("no shader");
            return;
        }
        bloomMaterial = CoreUtils.CreateEngineMaterial(bloomshader);
        this.sampleTimes = sampleTimes;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (bloomMaterial == null)
        {
            Debug.LogError("≤ƒ÷ ≥ı ºªØ ß∞‹");
            return;
        }

        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        VolumeStack stack = VolumeManager.instance.stack;
        bloomVolume = stack.GetComponent<BloomVolume>();

        var cmd = CommandBufferPool.Get(renderPassTag);   
        Render(cmd, ref renderingData);                
        context.ExecuteCommandBuffer(cmd);            
        
        CommandBufferPool.Release(cmd); 

    }

    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (bloomVolume.IsActive() == false) return;
        bloomVolume.load(bloomMaterial, ref renderingData);

        RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTarget;   
        
        RenderTextureDescriptor thresholdRT = renderingData.cameraData.cameraTargetDescriptor;
        thresholdRT.depthBufferBits = 0;                                                                          
        int thresholdRTID = Shader.PropertyToID("thresholdRT");

        //Get highlight area
        cmd.GetTemporaryRT(thresholdRTID, thresholdRT.width, thresholdRT.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGBFloat);
        cmd.Blit(source, thresholdRTID, bloomMaterial, 0);

        int downSize = 2;
        RenderTextureDescriptor downSampleRTDescriptor = renderingData.cameraData.cameraTargetDescriptor;

        RenderTexture[] downSampleRTs = new RenderTexture[sampleTimes];
        RenderTexture[] upSampleRTs = new RenderTexture[sampleTimes];

        for (int i = 0; i < sampleTimes; i++)
        {
            downSampleRTs[i] = RenderTexture.GetTemporary(downSampleRTDescriptor.width / downSize, downSampleRTDescriptor.height / downSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            downSampleRTs[i].filterMode = FilterMode.Bilinear;
            if (i != sampleTimes - 1)
            {
                upSampleRTs[sampleTimes - i - 2] = RenderTexture.GetTemporary(downSampleRTDescriptor.width / downSize, downSampleRTDescriptor.height / downSize, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                upSampleRTs[sampleTimes - i - 2].filterMode = FilterMode.Bilinear;
            }
            
            downSize *= 2;
        }

        //Down Sample
        for (int i = 0; i < sampleTimes; i++)
        {
            if (i == 0)
            {
                cmd.Blit(thresholdRTID, downSampleRTs[0], bloomMaterial, 1);
            }
            else
            {
                cmd.Blit(downSampleRTs[i - 1], downSampleRTs[i], bloomMaterial, 1);
            }
        }
        //Up Sample
        bloomMaterial.SetTexture("_PrevMip", downSampleRTs[sampleTimes - 1]);
        cmd.Blit(downSampleRTs[sampleTimes - 2], upSampleRTs[0], bloomMaterial, 2);
        for (int i = 1; i < sampleTimes - 1; i++)
        {
            bloomMaterial.SetTexture("_PrevMip", upSampleRTs[i - 1]);
            cmd.Blit(downSampleRTs[sampleTimes - i - 2], upSampleRTs[i], bloomMaterial, 2);
        }

        //Up sample to screen
        int tempID = Shader.PropertyToID("TempRT");
        cmd.GetTemporaryRT(tempID, renderingData.cameraData.cameraTargetDescriptor);

        bloomMaterial.SetTexture("_BloomTex", upSampleRTs[sampleTimes - 2]);
        cmd.Blit(source, tempID);
        cmd.Blit(tempID, source, bloomMaterial, 3);


        //Release
        cmd.ReleaseTemporaryRT(thresholdRTID);
        cmd.ReleaseTemporaryRT(tempID);
        //cmd.ReleaseTemporaryRT(testRTID);
        for (int i = 0; i < sampleTimes; i++)
        {
            RenderTexture.ReleaseTemporary(downSampleRTs[i]);
            RenderTexture.ReleaseTemporary(upSampleRTs[i]);
        }
    }

}
