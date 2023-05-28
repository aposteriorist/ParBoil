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
using ParLibrary.Sllz;
using ParBoil.RGGFormats;

namespace ParBoil;

internal class ProjectManager
{
    public static Node Par;
    public static string Project;
    public static string Name;

    private static ParArchiveReaderParameters readerParams;
    private static ParArchiveWriterParameters writerParams;

    private static Dictionary<string, DataStream> IncludedStreams;
    private static Dictionary<string, ParFile> ReplacedFormats;

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
        if (ReplacedFormats.Count > 0)
        {
            Directory.SetCurrentDirectory(Project);

            using var includeFile = DataStreamFactory.FromFile($"{Name}.include", FileOpenMode.Write);

            var writer = new Yarhl.IO.TextWriter(includeFile);
            foreach (var includePath in ReplacedFormats.Keys)
            {
                writer.WriteLine(includePath);
            }
        }

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


        IncludedStreams = new Dictionary<string, DataStream>();
        ReplacedFormats = new Dictionary<string, ParFile>();

        if (File.Exists($"{Name}.include"))
        {
            var inclusions = File.ReadLines($"{Name}.include");
            foreach (string includePath in inclusions)
            {
                var file = Navigator.SearchNode(Par, includePath);
                var oldFormat = file.GetFormatAs<ParFile>();

                string absoluteIncludePath = $"{Project}{file.Path}/{file.Name}";

                if (!File.Exists(absoluteIncludePath))
                {
                    continue;
                }

                using var fileStream = DataStreamFactory.FromFile(absoluteIncludePath, FileOpenMode.Read);
                var newStream = DataStreamFactory.FromMemory();
                fileStream.WriteTo(newStream);

                IncludedStreams.Add(includePath, newStream);
                ReplacedFormats.Add(includePath, oldFormat);

                var newFormat = new ParFile(newStream)
                {
                    CanBeCompressed = true,
                    IsCompressed = false,
                    WasCompressed = oldFormat.WasCompressed,
                    CompressionVersion = oldFormat.CompressionVersion,
                    DecompressedSize = (uint)newStream.Length,
                    Attributes = oldFormat.Attributes,
                    Timestamp = oldFormat.Timestamp,
                };

                file.ChangeFormat(newFormat, disposePreviousFormat: false);
            }
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

    public static void IncludeFile(Node file)
    {
        if (IncludedStreams.ContainsKey(file.Path))
            IncludedStreams.Remove(file.Path);

        ParFile oldFormat;
        if (ReplacedFormats.ContainsKey(file.Path))
        {
            oldFormat = ReplacedFormats[file.Path];
            ReplacedFormats.Remove(file.Path);
        }
        else
        {
            oldFormat = file.GetFormatAs<ParFile>();
        }

        var newStream = file.GetFormatAs<RGGFormat>().AsBinStream();

        IncludedStreams.Add(file.Path, newStream);
        ReplacedFormats.Add(file.Path, oldFormat);

        var newFormat = new ParFile(newStream)
        {
            CanBeCompressed = true,
            IsCompressed = false,
            WasCompressed = oldFormat.WasCompressed,
            CompressionVersion = oldFormat.CompressionVersion,
            DecompressedSize = (uint)newStream.Length,
            Attributes = oldFormat.Attributes,
            Timestamp = oldFormat.Timestamp,
        };

        file.ChangeFormat(newFormat, disposePreviousFormat: false);

        // Write the modified stream out to disk as a file, to be loaded from if need be.
        newStream.WriteTo($"{Project}{file.Path}/{file.Name}");

        // .include file will be written to on close, not here.
    }
}

