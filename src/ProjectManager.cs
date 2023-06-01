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

    private static List<Node> includedNodes;
    private static Dictionary<string, DataStream> includedStreams;
    private static Dictionary<string, ParFile> replacedFormats;

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
        if (replacedFormats != null && replacedFormats.Count > 0)
        {
            Directory.SetCurrentDirectory(Project);

            using var includeFile = DataStreamFactory.FromFile($"{Name}.include", FileOpenMode.Write);

            var writer = new Yarhl.IO.TextWriter(includeFile);
            foreach (var includePath in replacedFormats.Keys)
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


        includedNodes = new List<Node>();
        includedStreams = new Dictionary<string, DataStream>();
        replacedFormats = new Dictionary<string, ParFile>();

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

                var newStream = Program.CopyStreamFromFile(absoluteIncludePath, FileOpenMode.Read);

                includedNodes.Add(file);
                includedStreams.Add(includePath, newStream);
                replacedFormats.Add(includePath, oldFormat);

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
        if (includedStreams.ContainsKey(file.Path))
            includedStreams.Remove(file.Path);

        ParFile oldFormat;
        if (replacedFormats.ContainsKey(file.Path))
        {
            oldFormat = replacedFormats[file.Path];
            replacedFormats.Remove(file.Path);
        }
        else
        {
            oldFormat = file.GetFormatAs<ParFile>();
        }

        var newStream = file.GetFormatAs<RGGFormat>().AsBinStream();

        includedNodes.Add(file);
        includedStreams.Add(file.Path, newStream);
        replacedFormats.Add(file.Path, oldFormat);

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

    public static void ExcludeFile(Node file)
    {
        var oldFormat = replacedFormats[file.Path];

        file.ChangeFormat(oldFormat, disposePreviousFormat: true);

        includedNodes.Remove(file);
        includedStreams.Remove(file.Path);
        replacedFormats.Remove(file.Path);
    }
}

