using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

[ExecuteInEditMode]
public class BadassAssetPipe : RenderPipelineAsset
{
    public bool showOnlyColor = true; 

#if UNITY_EDITOR
    [UnityEditor.MenuItem("RenderPipe/Badass")]
    static void CreateBadassAssetPipeline()
    {
        var instance = CreateInstance<BadassAssetPipe>();
        UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/Create BadassPipe.asset");
    }
#endif

    protected override IRenderPipeline InternalCreatePipeline()
    {
        return new BadassPipeInstance(showOnlyColor);
    }
}

public class BadassPipeInstance : RenderPipeline
{
    private bool m_OnlyColor = false;

    public BadassPipeInstance(bool onlyColor)
    {
        m_OnlyColor = onlyColor;
    }

    private void OnlyColorRender(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        cmd.ClearRenderTarget(true, true, new Color(0.0f, 1.0f, 1.0f));
        renderContext.ExecuteCommandBuffer(cmd);
        cmd.Release();
        renderContext.Submit();
    }

    private void OpaqueRender(ScriptableRenderContext renderContext, Camera[] cameras, CommandBuffer cmd)
    {
        ScriptableCullingParameters cullingParameters;
        if(!CullResults.GetCullingParameters(cameras[0], out cullingParameters))
        {
            //If camera doesn't have culilng?
        }

        // Clear Depth
        CullResults cull = CullResults.Cull(ref cullingParameters, renderContext);
        renderContext.SetupCameraProperties(cameras[0]);
        cmd.ClearRenderTarget(true, false, cameras[0].backgroundColor);
        cmd.Release();

        // Draw object in the BasicPass shader pass
        DrawRendererSettings settings = new DrawRendererSettings(cameras[0], new ShaderPassName("BasicPass"));
        settings.sorting.flags = SortFlags.CommonOpaque;

        FilterRenderersSettings filterSettings = new FilterRenderersSettings(true) {renderQueueRange = RenderQueueRange.opaque };
        renderContext.DrawRenderers(cull.visibleRenderers, ref settings, filterSettings);

        renderContext.Submit();

    }

    public override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        base.Render(renderContext, cameras);
        CommandBuffer cmd = new CommandBuffer();
        if (m_OnlyColor)
        {
            OnlyColorRender(renderContext, cmd);
        }
        else
        {
            OpaqueRender(renderContext, cameras, cmd);            
        }       
        
    }
}
