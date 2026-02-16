using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class ASCloudRenderPass : ScriptableRenderPass
{
    private RTHandle m_CloudColorHandle;
    private Material m_Material;
    private Material m_CpyMaterial;
    private float m_CloudRenderScale;

    public ASCloudRenderPass(string passName)
    {
        profilingSampler = new ProfilingSampler(passName);
    }

    public void SetUp(Material mat, Material cpyMat, float cloudRenderScale)
    {
        m_Material = mat;
        m_CpyMaterial = cpyMat;
        float crs = cloudRenderScale;
        if(crs < 0.1f) { crs = 0.1f; }
        if(crs > 2f) { crs = 2f; }
        m_CloudRenderScale = crs;
    }

    public void Dispose()
    {
        RTHandles.Release(m_CloudColorHandle);
    }

    class PassData
    {
        public TextureHandle sceneColor;
        public TextureHandle depthBuffer;
        public TextureHandle cloudColorIn;
        public TextureHandle cloudColorOut;
        public Material material;
        public Material cpyMaterial;
        public float scale;
    }

    class CopyPassData
    {
        public TextureHandle sceneColor;
        public TextureHandle SorceColor;
        public Material cpyMaterial;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (m_Material == null || m_CpyMaterial == null) return;

        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        TextureHandle sceneColorHandle = resourceData.activeColorTexture;
        TextureHandle sceneDepthHandle = resourceData.activeDepthTexture;
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        RenderTextureDescriptor CloudTexDescriptor = cameraData.cameraTargetDescriptor;
        CloudTexDescriptor.height = Mathf.RoundToInt(CloudTexDescriptor.height * m_CloudRenderScale);
        CloudTexDescriptor.width = Mathf.RoundToInt(CloudTexDescriptor.width * m_CloudRenderScale);
        CloudTexDescriptor.colorFormat = RenderTextureFormat.ARGB32;
        CloudTexDescriptor.depthBufferBits = 0;
        RenderingUtils.ReAllocateHandleIfNeeded(ref m_CloudColorHandle, CloudTexDescriptor, name: "_CloudColorIN");

        TextureHandle CloudColorInHandle = renderGraph.ImportTexture(m_CloudColorHandle);

        TextureHandle CloudColorOutHandle = UniversalRenderer.CreateRenderGraphTexture(renderGraph, CloudTexDescriptor, "_CloudColorOut", true);

        
/*        using (var builder = renderGraph.AddRasterRenderPass<PassData>("ASCloudRenderPass", out var passData))
        {
            passData.sceneColor = sceneColorHandle;
            passData.depthBuffer = sceneDepthHandle;
            passData.cloudColorIn = CloudColorInHandle;
            passData.cloudColorOut = CloudColorOutHandle;
            passData.material = m_Material;
            passData.scale = m_CloudRenderScale;

            builder.SetRenderAttachment(passData.cloudColorOut, 0, AccessFlags.Write);
            //builder.UseTexture(sceneColorHandle, AccessFlags.Read);
            //builder.SetRenderAttachment(passData.sceneColor, 0, AccessFlags.Write);//<
            builder.SetRenderAttachmentDepth(passData.depthBuffer, AccessFlags.Read);
            builder.UseTexture(passData.cloudColorIn, AccessFlags.Read);

            //builder.AllowPassCulling(false);//Debug

            builder.SetRenderFunc(static (PassData data, RasterGraphContext context) => 
            {
                data.material.SetTexture("_CloudColorIN", data.cloudColorIn);
                //Blitter.BlitTexture(context.cmd, data.sceneColor ,data.cloudColorOut, new Vector4(1,1,0,0), data.material, 0);
                Blitter.BlitTexture(context.cmd, data.sceneColor, Vector2.one, data.material, 0);//< alphaはblitの時のみか?
            });

        }*/
        using (var builder = renderGraph.AddUnsafePass<PassData>("ASCloudRenderPass", out var passData))
        {
            passData.sceneColor = sceneColorHandle;
            passData.depthBuffer = sceneDepthHandle;
            passData.cloudColorIn = CloudColorInHandle;
            passData.cloudColorOut = CloudColorOutHandle;
            passData.material = m_Material;
            passData.cpyMaterial = m_CpyMaterial;
            passData.scale = m_CloudRenderScale;

            //builder.SetRenderAttachment(passData.cloudColorOut, 0, AccessFlags.Write);
            builder.UseTexture(passData.cloudColorOut, AccessFlags.ReadWrite);
            //builder.SetRenderAttachment(passData.sceneColor, 0, AccessFlags.Write);//<

            //builder.SetRenderAttachmentDepth(passData.depthBuffer, AccessFlags.Read);
            builder.UseTexture(passData.depthBuffer, AccessFlags.Read);

            builder.UseTexture(passData.cloudColorIn, AccessFlags.Read);

            builder.AllowPassCulling(false);//Debug

            builder.SetRenderFunc(static (PassData data, UnsafeGraphContext context) => 
            {
                data.material.SetTexture("_CloudColorIN", data.cloudColorIn);
                var commandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
                Blitter.BlitCameraTexture(commandBuffer, data.sceneColor, data.cloudColorOut, data.material, 0);

                //Vector4(縮小倍率,縮小倍率,offset,offset)
                //Blitter.BlitCameraTexture(commandBuffer, data.cloudColorOut, data.sceneColor, new Vector4(2,2,-0.5f,-0.5f));
                
                CoreUtils.SetRenderTarget(commandBuffer, data.sceneColor);
                Blitter.BlitTexture(commandBuffer, data.cloudColorOut, new Vector4(1f,1f,0f,0f), data.cpyMaterial, 0);
                //Blitter.BlitTexture(commandBuffer, data.cloudColorOut, new Vector4(2,2,-0.5f,-0.5f), data.cpyMaterial, 0);
            });

        }
        //renderGraph.AddBlitPass(sceneColorHandle, CloudColorInHandle, Vector2.one, Vector2.zero, passName: "BlitASCloudColor");//コピー?

        /*using (var builder = renderGraph.AddRasterRenderPass<CopyPassData>("ASCloudOverlapRenderPass", out var passData))
        {
            passData.sceneColor = sceneColorHandle;
            passData.SorceColor = CloudColorOutHandle;
            passData.cpyMaterial = m_CpyMaterial;

            builder.SetRenderAttachment(passData.sceneColor, 0, AccessFlags.Write);
            builder.UseTexture(passData.SorceColor, AccessFlags.Read);

            //builder.AllowPassCulling(false);//Debug

            builder.SetRenderFunc(static (CopyPassData data, RasterGraphContext context) => 
            {
                data.cpyMaterial.SetTexture("_SorceTexture", data.SorceColor);
                Blitter.BlitTexture(context.cmd, data.sceneColor, Vector2.one, data.cpyMaterial, 0);//< alphaはblitの時のみか?
            });

        }*/
    }
}