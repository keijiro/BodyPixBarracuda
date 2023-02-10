using Unity.Barracuda;
using UnityEngine;

namespace BodyPix {

#region Object construction/destruction helpers

static class BufferUtil
{
    public static RenderTexture NewArgbUav(int w, int h)
    {
        var rt = new RenderTexture(w, h, 0, RenderTextureFormat.Default,
                                            RenderTextureReadWrite.Linear);
        rt.enableRandomWrite = true;
        rt.Create();
        return rt;
    }

    public static GraphicsBuffer NewFloat4(int length)
      => new GraphicsBuffer(GraphicsBuffer.Target.Structured, length, sizeof(float) * 4);

    public static void Destroy(Object o)
    {
        if (o == null) return;
        if (Application.isPlaying)
            Object.Destroy(o);
        else
            Object.DestroyImmediate(o);
    }
}

static class ColorUtil
{
    public static bool IsLinear
      => QualitySettings.activeColorSpace == ColorSpace.Linear;
}

#endregion

#region Extension methods

static class ComputeShaderExtensions
{
    public static void DispatchThreads
      (this ComputeShader compute, int kernel, int x, int y, int z)
    {
        uint xc, yc, zc;
        compute.GetKernelThreadGroupSizes(kernel, out xc, out yc, out zc);
        x = (x + (int)xc - 1) / (int)xc;
        y = (y + (int)yc - 1) / (int)yc;
        z = (z + (int)zc - 1) / (int)zc;
        compute.Dispatch(kernel, x, y, z);
    }
}

static class IWorkerExtensions
{
    public static ComputeBuffer PeekOutputBuffer
      (this IWorker worker, string tensorName)
      => ((ComputeTensorData)worker.PeekOutput(tensorName).tensorOnDevice).buffer;
}

#endregion

#region GPU to CPU readback helpers

sealed class KeypointCache
{
    public KeypointCache(GraphicsBuffer source) => _source = source;
    public Keypoint[] Cached => Read();
    public void Invalidate() => _isCached = false;

    GraphicsBuffer _source;
    Keypoint[] _array = new Keypoint[Body.KeypointCount];
    bool _isCached;

    Keypoint[] Read()
    {
        if (_isCached) return _array;
        _source.GetData(_array, 0, 0, Body.KeypointCount);
        _isCached = true;
        return _array;
    }
}

#endregion

} // namespace BodyPix
