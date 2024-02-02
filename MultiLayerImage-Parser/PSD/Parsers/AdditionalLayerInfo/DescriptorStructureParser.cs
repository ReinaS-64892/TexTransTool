using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace net.rs64.MultiLayerImage.Parser.PSD
{
    internal static class DescriptorStructureParser
    {
        public static DescriptorStructure ParseDescriptorStructure(ref SubSpanStream stream)
        {
            var structure = new DescriptorStructure();


            var classIDNameByteLength = stream.ReadInt32() * 2;
            structure.NameFromClassID = stream.ReadSubStream(classIDNameByteLength).Span.ParseBigUTF16();

            var classIDLength = stream.ReadInt32();
            structure.NameFromClassID = stream.ReadSubStream(classIDLength == 0 ? 4 : classIDLength).Span.ParseASCII();

            structure.DescriptorCount = stream.ReadInt32();
            structure.Structures = new(structure.DescriptorCount);

            for (var i = 0; structure.DescriptorCount > i; i += 1)
            {
                var keyLength = stream.ReadInt32();
                var keyStr = stream.ReadSubStream(keyLength == 0 ? 4 : keyLength).Span.ParseASCII();
                var osTypeKey = stream.ReadSubStream(4).Span.ParseASCII();

                object structureValue;
                switch (osTypeKey)
                {

                    case "Objc":
                        {
                            structureValue = ParseDescriptorStructure(ref stream);
                            break;
                        }
                    case "doub":
                        {
                            structureValue = stream.ReadDouble();
                            break;
                        }

                    case "obj ":
                    case "VlLs":
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
                        { structureValue = null; break; }//未対応
                }

                structure.Structures.Add(keyStr, structureValue);
            }

            return structure;
        }

        internal class DescriptorStructure
        {
            public string NameFromClassID;
            public string ClassID;
            public int DescriptorCount;
            public Dictionary<string, object> Structures;
        }


        /*
        Descriptor structure の
        OSType Key の 'Objc' はそこにもう一度 Descriptor structure が入っているということらしい
        */
    }
}