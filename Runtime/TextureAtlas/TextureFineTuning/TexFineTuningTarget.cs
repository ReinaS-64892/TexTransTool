using System;
using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransTool.Utils;
using UnityEngine;

namespace net.rs64.TexTransTool.TextureAtlas.FineTuning
{
    public class TexFineTuningHolder
    {
        public Texture2D Texture2D;
        Dictionary<Type, ITuningData> _tuningDataDict;

        public Texture2D OriginTexture2D;



        internal TexFineTuningHolder(Texture2D texture2D)
        {
            Texture2D = texture2D;
            OriginTexture2D = texture2D;
            _tuningDataDict = new();
        }

        internal TuningData Find<TuningData>() where TuningData : class, ITuningData, new()
        {
            if (_tuningDataDict.TryGetValue(typeof(TuningData), out ITuningData t)) { return t as TuningData; }
            else { return null; }
        }
        internal TuningData Get<TuningData>() where TuningData : class, ITuningData, new()
        {
            if (_tuningDataDict.ContainsKey(typeof(TuningData))) { return _tuningDataDict[typeof(TuningData)] as TuningData; }
            else
            {
                var d = _tuningDataDict[typeof(TuningData)] = new TuningData();
                return d as TuningData;
            }
        }
        internal void Set<TuningData>(TuningData tuningData) where TuningData : class, ITuningData, new()
        {
            _tuningDataDict[typeof(TuningData)] = tuningData;
        }

    }

    internal class TexFineTuningUtility
    {

        public static Dictionary<string, TexFineTuningHolder> InitTexFineTuning(Dictionary<string, Texture2D> textures)
        {
            var texFineTuningTargets = textures.ToDictionary(i => i.Key, i => new TexFineTuningHolder(i.Value));
            foreach (var texKv in texFineTuningTargets)
            {
                texKv.Value.Set(new MipMapData());
                texKv.Value.Set(new TextureCompressionData());
            }
            return texFineTuningTargets;
        }
        public static void FinalizeTexFineTuning(Dictionary<string, TexFineTuningHolder> texFineTuningTargets, IDeferTextureCompress compress)
        {
            var applicantList = InterfaceUtility.GetInterfaceInstance<ITuningApplicant>().ToList();
            applicantList.Sort((L, R) => L.Order - R.Order);
            foreach (var applicant in applicantList)
            {
                applicant.ApplyTuning(texFineTuningTargets, compress);
            }
        }


        public static Dictionary<string, TexFineTuningHolder> ConvertForTargets(Dictionary<string, Texture2D> propAndTexture2Ds)
        {
            var targets = new Dictionary<string, TexFineTuningHolder>(propAndTexture2Ds.Count);
            foreach (var pat in propAndTexture2Ds)
            {
                targets.Add(pat.Key, new TexFineTuningHolder(pat.Value));
            }
            return targets;
        }

    }

}
