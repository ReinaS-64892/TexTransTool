using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace net.rs64.TexTransTool
{
    // public class AsyncTexture2D
    // {
    //     private string _name;
    //     private RenderTexture _rt;
    //     private AsyncGPUReadbackRequest[] _asyncGPUReadbackRequests;
    //     private bool _mipsFromRT, _rawRead;
    //     private Texture2D _outTex;

    //     private static Queue<Action> _pendingActions = new Queue<Action>();

    //     private TextureFormat _format;
    //     private bool _useMip;

    //     private void CreateTexIfNeeded()
    //     {
    //         if (_outTex == null)
    //         {
    //             Profiler.BeginSample("Construct Tex2D");
    //             _outTex = new Texture2D(_rt.width, _rt.height, _format, _useMip, !_rt.sRGB);
    //             _outTex.name = _name;
    //             Profiler.EndSample();
    //         }
    //     }

    //     public AsyncTexture2D(RenderTexture rt, TextureFormat? overrideFormat = null, bool? overrideUseMip = null)
    //     {
    //         _name = rt.name + "_CopyTex2D"; 
            
    //         _useMip = overrideUseMip ?? rt.useMipMap;
    //         _format = overrideFormat ?? GraphicsFormatUtility.GetTextureFormat(rt.graphicsFormat);
    //         var readMapCount = rt.useMipMap && _useMip ? rt.mipmapCount : 1;

    //         // Texture2Dの作成はかなり重いようなので、リードバックを開始してから行います。
    //         // なお、GetPixelDataのバッファーをそのままAsyncGPUReadback.RequestIntoNativeArrayに渡すこともためしたけど、
    //         // 最初のGetPixelDataで40ms以上かかってて使い物にならなかった・・・
    //         _pendingActions.Enqueue(this.CreateTexIfNeeded);

    //         int mipsRemaining = readMapCount;
    //         _asyncGPUReadbackRequests = new AsyncGPUReadbackRequest[readMapCount];
    //         for (var i = 0; readMapCount > i; i += 1)
    //         {
    //             var mipLevel = i;
                
    //             _asyncGPUReadbackRequests[i] = AsyncGPUReadback.Request(rt, i, _format, req =>
    //             {
    //                 Action _readTex = () =>
    //                 {
    //                     Profiler.BeginSample("Ingest texture");
    //                     var buf = req.GetData<byte>();
    //                     _outTex.SetPixelData(buf, mipLevel);
    //                     buf.Dispose();
    //                     Profiler.EndSample();
    //                 };

    //                 if (_outTex != null)
    //                 {
    //                     // すでにTexture2Dが作成されている場合は、そのままTexture2Dにデータを流し込みます。
    //                     _readTex();
    //                 }
    //                 else
    //                 {
    //                     // WaitAllRequestsの後にデータを流し込みます
    //                     _pendingActions.Enqueue(_readTex);
    //                 }
    //             });

    //         }

    //         _mipsFromRT = _useMip;
    //         _rt = rt;
    //     }
        
    //     public Texture2D GetTexture2D()
    //     {
    //         Profiler.BeginSample("GetTexture2D");

    //         // たまったTexture2Dの作成処理を実行
    //         Profiler.BeginSample("Process pending actions");
    //         while (_pendingActions.Count > 0)
    //         {
    //             _pendingActions.Dequeue()();
    //         }
    //         Profiler.EndSample();
            
    //         Profiler.BeginSample("Await async readback");
    //         AsyncGPUReadback.WaitAllRequests();
    //         Profiler.EndSample();

    //         // Texture2Dの作成が間に合わなかったリードバックに関しては、ここでデータを流し込んでおきます。
    //         Profiler.BeginSample("Process pending actions");
    //         while (_pendingActions.Count > 0)
    //         {
    //             _pendingActions.Dequeue()();
    //         }
    //         Profiler.EndSample();

    //         Profiler.BeginSample("Apply texture");
    //         _outTex.Apply(!_mipsFromRT);
    //         Profiler.EndSample();

    //         TTRt.R(_rt);

    //         Profiler.EndSample();
    //         return _outTex;
    //     }
    // }
}
