using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PerkinElmerSP2CSV
{
    /// <summary>
    /// ID (int16)
    /// Len (int32)
    /// TypeCode (int16) -- only DataSetDataMember?
    /// Data (len)
    /// </summary>
    public class TypedMemberBlock : Block
    {
        public TypedMemberBlock(BinaryReader file) : base(file.ReadInt16())
        {
            int len = file.ReadInt32();
            TypeCode = file.ReadInt16();
            Data = file.ReadBytes(len - 2);
        }

        public short TypeCode { get; }
    }
}
