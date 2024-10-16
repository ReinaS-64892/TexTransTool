using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace net.rs64.MultiLayerImage.Parser.PSD.AdditionalLayerInfo
{
    internal static class DescriptorStructureParser
    {
        public static DescriptorStructure ParseDescriptorStructure(ref SubSpanStream stream)
        {
            var structure = new DescriptorStructure();


            var classIDNameByteLength = stream.ReadUInt32() * 2;
            structure.NameFromClassID = stream.ReadSubStream((int)classIDNameByteLength).Span.ParseBigUTF16();

            var classIDLength = stream.ReadUInt32();
            structure.ClassID = stream.ReadSubStream((int)(classIDLength == 0 ? 4 : classIDLength)).Span.ParseASCII();

            structure.DescriptorCount = stream.ReadUInt32();
            structure.Structures = new((int)structure.DescriptorCount);

            for (var i = 0; structure.DescriptorCount > i; i += 1)
            {
                var keyLength = stream.ReadUInt32();
                var keyStr = stream.ReadSubStream((int)(keyLength == 0 ? 4 : keyLength)).Span.ParseASCII();
                var osTypeKey = stream.ReadSubStream(4).Span.ParseASCII();

                object structureValue;
                switch (osTypeKey)
                {
                    case "obj ":
                        { throw new NotImplementedException(); }
                    case "Objc":
                        {
                            structureValue = ParseDescriptorStructure(ref stream);
                            break;
                        }
                    case "VlLs":
                        { throw new NotImplementedException(); }

                    case "doub":
                        {
                            structureValue = stream.ReadDouble();
                            break;
                        }

                    case "UntF":
                    case "TEXT":
                    case "enum":
                    case "long":
                    case "comp":
                    case "bool":
                    case "GlbO":
                    case "type":
                    case "GlbC":
                    case "alis":
                    case "tdta":
                    default:
                        { throw new NotImplementedException(); }
                }

                structure.Structures.Add(keyStr, structureValue);
            }

            return structure;
        }

        internal class DescriptorStructure
        {
            public string NameFromClassID;
            public string ClassID;
            public uint DescriptorCount;
            public Dictionary<string, object> Structures;
        }


        /*
        Descriptor structure の
        OSType Key の 'Objc' はそこにもう一度 Descriptor structure が入っているということらしい
        */
    }
}
