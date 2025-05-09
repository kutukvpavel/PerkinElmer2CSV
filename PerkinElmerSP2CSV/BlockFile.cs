using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PerkinElmerSP2CSV
{
    /// <summary>
    /// Overall structure:
    /// Header = PEPE magic header + 40-byte content description string
    /// Contents = an array of blocks with id, size and some binary content
    /// </summary>
    /// <seealso cref="Block"/>
    public class BlockFile
    {
        const string FileSignature = "PEPE";
        const int DescriptionRecordLength = 40;

        public static BlockFile Load(string path)
        {
            BlockFile file;
            using (FileStream s = new FileStream(path, FileMode.Open))
            {
                file = new BlockFile(s);
            }
            return file;
        }

        public BlockFile(FileStream file)
        {
            //Parse header
            byte[] sig = new byte[FileSignature.Length];
            file.Read(sig);
            if (Encoding.ASCII.GetString(sig) != FileSignature)
                throw new NotSupportedException("This is not a Perkin-Elmer block file.");
            sig = new byte[DescriptionRecordLength];
            file.Read(sig);
            Description = Encoding.ASCII.GetString(sig);
            //Read contents
            List<Block> blocks = new List<Block>(); //Todo: some capacity heuristics based on file length?
            try
            {
                using BinaryReader r = new BinaryReader(file);
                while (r.BaseStream.Position < r.BaseStream.Length)
                {
                    blocks.Add(new Block(r));
                }
            }
            catch (EndOfStreamException)
            { }
            Contents = blocks.ToArray();
        }

        public string Description { get; }
        public Block[] Contents { get; }
    }
}
