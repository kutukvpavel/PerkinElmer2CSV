using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace PerkinElmerSP2CSV
{
    /// <summary>
    /// Based on the code for Matlab(R) by Stephen Westlake and Seer Green from Perkin Elmer (2007)
    /// Adapted for OOP and C#.NET by Kutukov Pavel, 2022
    /// </summary>
    public class SpFileProvider : IFileProvider
    {
        private static readonly SpFileProvider _instance = new SpFileProvider();
        private SpFileProvider() { }
        public static SpFileProvider Instance { get => _instance; }
        public string Extension { get; } = ".sp";

        enum Blocks : short
        {
            DSet2DC1DI = 120,
            HistoryRecord = 121,
            InstrHdrHistoryRecord = 122,
            InstrumentHeader = 123,
            IRInstrumentHeader = 124,
            UVInstrumentHeader = 125,
            FLInstrumentHeader = 126
        }
        enum Members : short
        {
            DataSetDataType = -29839,
            DataSetAbscissaRange = -29838,
            DataSetOrdinateRange = -29837,
            DataSetInterval = -29836,
            DataSetNumPoints = -29835,
            DataSetSamplingMethod = -29834,
            DataSetXAxisLabel = -29833,
            DataSetYAxisLabel = -29832,
            DataSetXAxisUnitType = -29831,
            DataSetYAxisUnitType = -29830,
            DataSetFileType = -29829,
            DataSetData = -29828,
            DataSetName = -29827,
            DataSetChecksum = -29826,
            DataSetHistoryRecord = -29825,
            DataSetInvalidRegion = -29824,
            DataSetAlias = -29823,
            DataSetVXIRAccyHdr = -29822,
            DataSetVXIRQualHdr = -29821,
            DataSetEventMarkers = -29820
        }
        enum TypeCodes : short
        {
            Short = 29999,
            UShort = 29998,
            Int = 29997,
            UInt = 29996,
            Long = 29995,
            Bool = 29988,
            Char = 29987,
            CvCoOrdPoint = 29986,
            StdFont = 29985,
            CvCoOrdDimension = 29984,
            CvCoOrdRectangle = 29983,
            RGBColor = 29982,
            CvCoOrdRange = 29981,
            Double = 29980,
            CvCoOrd = 29979,
            ULong = 29978,
            Peak = 29977,
            CoOrd = 29976,
            Range = 29975,
            CvCoOrdArray = 29974,
            Enum = 29973,
            LogFont = 29972
        }
        const Blocks MainBlock = Blocks.DSet2DC1DI;
        const int DataMemberDataOffset = 4;
        const int SizeofDouble = 8;

        static string ReadString(byte[] data)
        {
            try
            {
                int len = BitConverter.ToInt16(data, 0);
                return Encoding.ASCII.GetString(data, 2, len);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Warning: couldn't read a string field due to a bad length value.");
                return null;
            }
        }
        static void GetSpectrumWrapper(TypedMemberBlock mb, Spectrum2d sp)
        {
            switch ((Members)mb.Id)
            {
                case Members.DataSetAbscissaRange:
                    if (mb.TypeCode != (short)TypeCodes.CvCoOrdRange)
                        throw new NotSupportedException("Not supported data type for X axis range.");
                    sp.StartX = BitConverter.ToDouble(mb.Data, 0);
                    sp.EndX = BitConverter.ToDouble(mb.Data, SizeofDouble);
                    break;
                case Members.DataSetInterval:
                    sp.ResolutionX = BitConverter.ToDouble(mb.Data, 0);
                    break;
                case Members.DataSetNumPoints:
                    sp.PointsY = new double[BitConverter.ToInt32(mb.Data, 0)];
                    break;
                case Members.DataSetXAxisLabel:
                    sp.LabelX = ReadString(mb.Data);
                    break;
                case Members.DataSetYAxisLabel:
                    sp.LabelY = ReadString(mb.Data);
                    break;
                case Members.DataSetData:
                    if (mb.TypeCode != (short)TypeCodes.CvCoOrdArray)
                        throw new NotSupportedException("Not supported data type for Y data array.");
                    if (sp.PointsY == null) 
                        sp.PointsY = new double[BitConverter.ToInt32(mb.Data, 0) / SizeofDouble];
                    try
                    {
                        for (int i = 0; i < sp.PointsY.Length; i++)
                        {
                            sp.PointsY[i] = BitConverter.ToDouble(mb.Data, DataMemberDataOffset + i * SizeofDouble);
                        }
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                    }
                    break;
                case Members.DataSetName:
                    sp.Name = ReadString(mb.Data);
                    break;
                case Members.DataSetAlias:
                    sp.Alias = ReadString(mb.Data);
                    break;
                default:
                    Console.WriteLine($"Info: ignoring unknown block id {mb.Id}.");
                    break;
            }
        }

        static IEnumerable<TypedMemberBlock> ParseMembers(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            using BinaryReader r = new BinaryReader(ms);
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                TypedMemberBlock b = null;
                try
                {
                    b = new TypedMemberBlock(r);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                yield return b;
            }
        }

        public IData GetData(string path)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = BlockFile.Load(path).Contents.FirstOrDefault(x => x.Id == (short)MainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(Blocks), MainBlock)} block.");
            var spec = new Spectrum2d();
            foreach (var item in ParseMembers(main.Data))
            {
                GetSpectrumWrapper(item, spec);
            }
            return spec;
        }
    }
}
