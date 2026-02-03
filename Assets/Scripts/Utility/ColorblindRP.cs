using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class ColorblindRendererFeature : ScriptableRendererFeature
{
    public static ColorblindRendererFeature Instance;

    public Material ColorBlindMaterial;
    public Material BlitMaterial;
    [Range(0, 3)] public int Type;
    public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRendering;

    private ColorblindRenderPass _pass;

    public override void Create()
    {
        if (Instance == null) Instance = this;
        else return;

        if (ColorBlindMaterial == null || BlitMaterial == null) return;
        _pass = new ColorblindRenderPass(ColorBlindMaterial, BlitMaterial, RenderPassEvent);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _pass.Setup(Type);
        renderer.EnqueuePass(_pass);
    }
}

public class ColorblindRenderPass : ScriptableRenderPass
{
    private static readonly int TypeID = Shader.PropertyToID("_Type");

    private Material _colorBlindMaterial;
    private Material _blitMaterial;
    private int _type;

    private class PassData
    {
        public Material BlitMaterial;
        public TextureHandle SourceTexture;
        public int Type;
    }

    public ColorblindRenderPass(Material pColorBlindMaterial, Material pBlitMaterial, RenderPassEvent pEventTiming)
    {
        _colorBlindMaterial = pColorBlindMaterial;
        _blitMaterial = pBlitMaterial;
        renderPassEvent = pEventTiming;
    }

    public void Setup(int type)
    {
        _type = type;
    }

    public override void RecordRenderGraph(RenderGraph pRenderGraph, ContextContainer pFrameData)
    {
        if (_colorBlindMaterial == null) return;

        UniversalResourceData vResourceData = pFrameData.Get<UniversalResourceData>();
        UniversalCameraData vCameraData = pFrameData.Get<UniversalCameraData>();

        // Texture couleur actuelle de la caméra
        TextureHandle vCameraTexture = vResourceData.activeColorTexture;

        //Texture temporaire pour recevoir le blit
        RenderTextureDescriptor vDescriptor = vCameraData.cameraTargetDescriptor;
        vDescriptor.depthBufferBits = 0;
        TextureHandle vTempTexture = UniversalRenderer.CreateRenderGraphTexture(pRenderGraph, vDescriptor, "_ColorBlindTempRT", true);


        // ===== PASS : CameraColor -> Temp =====
        using (var vBuilder = pRenderGraph.AddRasterRenderPass<PassData>("Colorblind Get Color", out var oPassData))
        {
            oPassData.BlitMaterial = _colorBlindMaterial;
            oPassData.SourceTexture = vCameraTexture;
            oPassData.Type = _type;

            vBuilder.UseTexture(vCameraTexture, AccessFlags.Read);
            vBuilder.SetRenderAttachment(vTempTexture, 0, AccessFlags.Write);

            vBuilder.AllowPassCulling(false);

            vBuilder.SetRenderFunc((PassData pData, RasterGraphContext pCtx) =>
            {
                pData.BlitMaterial.SetInt(TypeID, pData.Type);
                Blitter.BlitTexture(pCtx.cmd, pData.SourceTexture, new Vector4(1, 1, 0, 0), pData.BlitMaterial, 0);
            });
        }
        // ---------- PASS 2 : Temp -> CameraColor ----------
        using (var vBuilder = pRenderGraph.AddRasterRenderPass<PassData>("Colorblind Copy Back", out var passData))
        {
            passData.SourceTexture = vTempTexture;
            passData.BlitMaterial = _blitMaterial;

            vBuilder.UseTexture(vTempTexture, AccessFlags.Read);
            vBuilder.SetRenderAttachment(vCameraTexture, 0, AccessFlags.Write);

            vBuilder.AllowPassCulling(false);

            vBuilder.SetRenderFunc((PassData pData, RasterGraphContext pCtx) =>
            {
                Blitter.BlitTexture(pCtx.cmd, pData.SourceTexture, new Vector4(1, 1, 0, 0), pData.BlitMaterial, 0);
            });
        }
    }
}