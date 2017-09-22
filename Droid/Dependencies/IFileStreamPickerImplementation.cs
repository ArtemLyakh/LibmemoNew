using System;
using System.IO;
using Libmemo;
using Libmemo.Droid.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(IFileStreamPickerImplementation))]
namespace Libmemo.Droid.Dependencies
{
    public class IFileStreamPickerImplementation : IFileStreamPicker
    {
        (Stream Stream, string Name) IFileStreamPicker.GetFile(string path)
        {
			var file = File.OpenRead(path);
			return (file as Stream, file.Name);
        }
    }
}
