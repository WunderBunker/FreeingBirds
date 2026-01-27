using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class GrabAllScreenRP : ScriptableRendererFeature
{
    public string TextureName = "_BlitScreenTexture";
    public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRendering;
    public LayerMask LayerMask = ~0;
    public float ResolutionScale = 0.5f;

    GrabScreenPass _grabScreenPass;

    public override void Create()
    {
        _grabScreenPass = new GrabScreenPass(TextureName, RenderPassEvent);
        _grabScreenPass.LayerMask = LayerMask;
        _grabScreenPass.ResolutionScale = ResolutionScale;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_grabScreenPass != null)
            renderer.EnqueuePass(_grabScreenPass);
    }
}

class GrabScreenPass : ScriptableRenderPass
{
    public LayerMask LayerMask;
    public float ResolutionScale = 0.5f;
    readonly string _textureName;

    public GrabScreenPass(string pTextureName, RenderPassEvent pEventTiming)
    {
        _textureName = pTextureName;
        renderPassEvent = pEventTiming;
    }

    public override void RecordRenderGraph(RenderGraph pRenderGraph, ContextContainer pFrameData)
    {
        UniversalResourceData vFrameData = pFrameData.Get<UniversalResourceData>();
        TextureDesc vDescription = vFrameData.cameraDepthTexture.GetDescriptor(pRenderGraph);
        vDescription.depthBufferBits = 0;
        vDescription.width = (int)(vDescription.width * ResolutionScale);
        vDescription.height = (int)(vDescription.height * ResolutionScale);
        vDescription.depthBufferBits = 0;
        vDescription.msaaSamples = MSAASamples.None;
        vDescription.format = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm;

        TextureHandle vOutputTex = pRenderGraph.CreateTexture(vDescription);

        // ============ BLIT SCREEN → OUTPUT TEX ============
        using (var vBuilder = pRenderGraph.AddRasterRenderPass<BlitPassData>("GrabScreen-Write", out var oPassData))
        {
            oPassData.Source = vFrameData.activeColorTexture;
            oPassData.Dest = vOutputTex;

            vBuilder.UseTexture(oPassData.Source);
            vBuilder.SetRenderAttachment(oPassData.Dest, 0);

            vBuilder.AllowPassCulling(false);

            vBuilder.SetRenderFunc((BlitPassData data, RasterGraphContext ctx) =>
            {
                Blitter.BlitTexture(ctx.cmd, data.Source, new Vector4(1, 1, 0, 0), 0, false);
            });
        }

        using (var vBuilder = pRenderGraph.AddRasterRenderPass<BlitPassData>("GrabScreen-SetGlobal", out var oPassData))
        {
            oPassData.Source = vOutputTex;
            oPassData.GlobalName = _textureName;

            vBuilder.UseTexture(oPassData.Source, AccessFlags.Read);

            vBuilder.AllowPassCulling(false);
            vBuilder.AllowGlobalStateModification(true);

            vBuilder.SetRenderFunc((BlitPassData data, RasterGraphContext ctx) =>
            {
                ctx.cmd.SetGlobalTexture(data.GlobalName, data.Source);
                ctx.cmd.SetGlobalVector("_GrabTexSize", new Vector4(
                    1f / vDescription.width,
                    1f / vDescription.height,
                    vDescription.width,
                    vDescription.height
                ));
            });
        }
    }

    class BlitPassData
    {
        public TextureHandle Source;
        public TextureHandle Dest;
        public string GlobalName;
    }
}
