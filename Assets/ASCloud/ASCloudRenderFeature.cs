using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class ASCloudRenderFeature : ScriptableRendererFeature
{
    [SerializeField] RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingOpaques;
    [SerializeField] Material material;
    [SerializeField] Material cpyMaterial;
    [SerializeField] float cloudRenderScale = 0.5f;
    ASCloudRenderPass m_asCloudPass;


    public override void Create()
    {
        m_asCloudPass = new ASCloudRenderPass(name);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null) { return; }

        m_asCloudPass.renderPassEvent = injectionPoint;
        m_asCloudPass.SetUp(material,cpyMaterial,cloudRenderScale);

        renderer.EnqueuePass(m_asCloudPass);
    }

    protected override void Dispose(bool disposing)
    {
        m_asCloudPass.Dispose();
    }



}