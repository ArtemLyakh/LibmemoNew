using System;
using System.IO;

namespace Libmemo
{
    public interface IFileStreamPicker
    {
        (Stream Stream, string Name) GetFile(string path);
    }
}
