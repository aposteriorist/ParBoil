using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Yarhl.FileSystem;
using Yarhl.IO;
using ParLibrary.Converter;
using ParLibrary;
using System.Diagnostics;
using Yarhl.FileFormat;

namespace ParBoil;

internal class ProjectManager
{
    public static Node Par;
    public static string Project;
    public static string Name;
    public static List<string> Include;

    private static ParArchiveReaderParameters readerParams;
    private static ParArchiveWriterParameters writerParams;

    public static void Initialize()
    {
        writerParams = new ParArchiveWriterParameters
        {
            CompressorVersion = 3,
            IncludeDots = false,
        };        
    }

    public static void Close()
    {
        if (Par != null)
            Par.Dispose();
    }

    public static void FromFile(string path)
    {
        if (Par != null)
            Par.Dispose();

        Project = path + ".boil\\";
        Name = Path.GetFileName(path);

        if (!Directory.Exists(Project))
            Directory.CreateDirectory(Project);

        if (!File.Exists($"{Project}{Name}.orig"))
            File.Copy(path, $"{Project}{Name}.orig");

        using var inputStream = DataStreamFactory.FromFile(path, FileOpenMode.Read);
        Par = NodeFactory.FromMemory(Name);
        inputStream.WriteTo(Par.Stream);

        readerParams = new ParArchiveReaderParameters
        {
            Recursive = false,
            Tags = new Dictionary<string, dynamic>(),
        };

        Par.TransformWith<ParArchiveReader, ParArchiveReaderParameters>(readerParams);

        foreach (var tag in readerParams.Tags)
            Par.Tags.Add(tag);        

        Directory.SetCurrentDirectory(Project);


        if (File.Exists($"{Name}.include"))
            Include = new List<string>(File.ReadLines($"{Name}.include"));
        else
        {
            File.Create($"{Name}.include").Close();
            Include = new List<string>();
        }
    }

    public static void FromProject(string path)
    {

    }

    public static void ToFile(string path)
    {
        writerParams.OutputPath = path;

        DateTime startTime = DateTime.Now;
        Debug.WriteLine("Creating PAR...");

        var parArchive = (ParFile)ConvertFormat.With<ParArchiveWriter, ParArchiveWriterParameters>(writerParams, Par.Format);

        using var fileStream = DataStreamFactory.FromFile(path, FileOpenMode.Write);
        parArchive.Stream.WriteTo(fileStream);

        Debug.WriteLine("Done.");
        DateTime endTime = DateTime.Now;
        Debug.WriteLine($"Time elapsed: {endTime - startTime:g}");
    }

    private static void FlushNodesTo(string path)
    {
        foreach (Node n in Navigator.IterateNodes(Par))
            if (!n.IsContainer && !File.Exists(path + n.Path + n.Name))
                n.Stream.WriteTo(path + n.Path + n.Name);
    }
}

