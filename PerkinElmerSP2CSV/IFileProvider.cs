using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper;

namespace PerkinElmerSP2CSV
{
    public interface IFileProvider
    {
        public string Extension { get; }
        public IData GetData(BlockFile file);
        
    }
}
