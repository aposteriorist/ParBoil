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
using System.Xml.Linq;

namespace ParBoil;

internal class ProjectManager
{
    public static Node Par;
    public static string Project;
    public static string Name;

    internal const string Original = "Original";
    internal const string LoadedVersions = "LoadedVersions";
    internal const string IncludedVersion = "IncludedVersion";
    internal const string SelectedVersion = "SelectedVersion";
    internal const string FirstLoadBuffer = "FirstLoadBuffer";

    private static ParArchiveReaderParameters readerParams;
    private static ParArchiveWriterParameters writerParams;

    private static List<Node> includedNodes;

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
        if (includedNodes != null && includedNodes.Count > 0)
        {
            Directory.SetCurrentDirectory(Project);

            using var includeFile = DataStreamFactory.FromFile($"{Name}.include", FileOpenMode.Write);

            var writer = new Yarhl.IO.TextWriter(includeFile);
            foreach (var node in includedNodes)
            {
                writer.WriteLine($"{node.Path}:{node.Tags[IncludedVersion]}");
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

        if (File.Exists($"{Name}.include"))
        {
            var inclusions = File.ReadLines($"{Name}.include");
            foreach (string includePath in inclusions)
            {
                var split = includePath.Split(':');
                var file = Navigator.SearchNode(Par, split[0]);

                string includedFile = $"{Project}{file.Path}/{file.Name}";
                string includedFileJSON = $"{Project}{file.Path}/{split[1]}.json";

                if (!File.Exists(includedFile) || !File.Exists(includedFileJSON))
                {
                    MessageBox.Show($"Failed inclusion of {file.Path}/{file.Name} version {split[1]}.");
                    continue;
                }

                file.Tags[IncludedVersion] = split[1];
                includedNodes.Add(file);

                if (file.GetFormatAs<ParFile>().IsCompressed)
                    file.TransformWith<Decompressor>();

                RGGFormat.TransformToRGGFormat(file);

                var oldFormat = file.GetFormatAs<RGGFormat>();

                using var json = DataStreamFactory.FromFile(includedFileJSON, FileOpenMode.Read);
                oldFormat.LoadFromJSON(json);

                file.Tags[FirstLoadBuffer] = oldFormat;

                var newStream = Program.CopyStreamFromFile(includedFile, FileOpenMode.Read);
                var newFormat = oldFormat.CopyFormat(newStream, true);

                file.Tags[LoadedVersions] = new Dictionary<string, RGGFormat>();
                file.Tags[LoadedVersions][file.Tags[IncludedVersion]] = newFormat;

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

    public static void IncludeFile(Node file, ParFile newFormat, object versionTag)
    {
        if (!file.Tags.ContainsKey("LoadedVersions"))
        {
            file.Tags["LoadedVersions"] = new Dictionary<string, RGGFormat>();
        }

        if (!file.Tags["LoadedVersions"].ContainsKey(Original))
            file.Tags["LoadedVersions"][Original] = file.Format;
        file.Tags[IncludedVersion] = versionTag;

        if (!includedNodes.Contains(file))
            includedNodes.Add(file);

        file.ChangeFormat(newFormat, disposePreviousFormat: false);

        // Write the modified stream out to disk as a file, to be loaded from if need be.
        newFormat.Stream.WriteTo($"{Project}{file.Path}/{file.Name}");

        // .include file will be written to on close, not here.
    }

    public static void ExcludeFile(Node file)
    {
        if (file.Tags.ContainsKey(IncludedVersion) && file.Tags.ContainsKey(FirstLoadBuffer))
        {
            file.ChangeFormat(file.Tags[FirstLoadBuffer], disposePreviousFormat: false);

            includedNodes.Remove(file);

            file.Tags.Remove(IncludedVersion);
        }
        // Hitting else is an error.
    }
}

