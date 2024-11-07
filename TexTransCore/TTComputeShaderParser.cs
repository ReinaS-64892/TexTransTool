#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace net.rs64.TexTransCore
{
    public static class TTComputeShaderUtility
    {

        public const string TTCOMP_HEADER_BEGIN = "BEGIN__TT_COMPUTE_SHADER_HEADER";
        public const string TTCOMP_HEADER_END = "END__TT_COMPUTE_SHADER_HEADER";

        public static TTComputeShaderHeader? Parse(string ttcompString)
        {
            if ((ttcompString.Contains(TTCOMP_HEADER_BEGIN) && ttcompString.Contains(TTCOMP_HEADER_END)) is false) { return null; }

            var headerBegin = ttcompString.IndexOf(TTCOMP_HEADER_BEGIN) + TTCOMP_HEADER_BEGIN.Length;
            var headerEnd = ttcompString.IndexOf(TTCOMP_HEADER_END);

            var headerString = ttcompString.Substring(headerBegin, headerEnd - headerBegin);
            var headerLines = headerString.Split("\n").Select(s => s.Replace("\n", "").Replace("\r", "")).Where(str => string.IsNullOrWhiteSpace(str) is false).ToArray();

            var descriptions = new List<KeyValuePair<string, string>>();
            foreach (var kvStr in headerLines)
            {
                if (kvStr is null) { continue; }
                if (kvStr.StartsWith("//")) { continue; }

                var (keyString, valueString) = GetKeyValueString(kvStr);

                descriptions.Add(new(keyString, valueString));
            }

            return new TTComputeShaderHeader(descriptions);
        }

        public static (string keyString, string valueString) GetKeyValueString(string kvStr)
        {
            var keyStrLength = kvStr.IndexOf(" ");

            var keyString = kvStr.Substring(0, keyStrLength);
            var valueString = kvStr.Substring(keyStrLength + 1, kvStr.Length - (keyStrLength + 1));

            return (keyString, valueString);
        }

        public const string BlendingShaderTemplate =
@"
RWTexture2D<float4> AddTex;
RWTexture2D<float4> DistTex;

[numthreads(32, 32, 1)] void CSMain(uint3 id : SV_DispatchThreadID)
{
    DistTex[id.xy] = ColorBlend( DistTex[id.xy] , AddTex[id.xy] );
}
";


    }

    public class TTComputeShaderHeader
    {
        public readonly string ShaderLanguage;
        public readonly string ShaderLanguageVersion;
        public readonly TTComputeType ComputeType;
        private List<KeyValuePair<string, string>> Descriptions;

        internal TTComputeShaderHeader(List<KeyValuePair<string, string>> descriptions)
        {
            ShaderLanguage = descriptions.Find(kv => kv.Key == "Language").Value;
            ShaderLanguageVersion = descriptions.Find(kv => kv.Key == "LanguageVersion").Value;
            ComputeType = Enum.Parse<TTComputeType>(descriptions.Find(kv => kv.Key == "TTComputeType").Value);
            Descriptions = descriptions;
        }

        public string this[string key]
        {
            get { return Descriptions.Find(kv => kv.Key == key).Value; }
        }

        public IEnumerable<string> FindAll(string key)
        {
            return Descriptions.FindAll(kv => kv.Key == key).Select(kv => kv.Value);
        }
    }

    public enum TTComputeType
    {
        General,
        GrabBlend,
        Blending,
    }
}
