// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary.Converter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Yarhl.FileFormat;
    using Yarhl.FileSystem;
    using Yarhl.IO;

    /// <summary>
    /// Converter from BinaryFormat to ParArchive.
    /// </summary>
    public class ParArchiveReader : IInitializer<ParArchiveReaderParameters>, IConverter<BinaryFormat, NodeContainerFormat>
    {
        private ParArchiveReaderParameters parameters = new ParArchiveReaderParameters
        {
            Recursive = false,
        };

        /// <summary>
        /// Initializes reader parameters.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        public void Initialize(ParArchiveReaderParameters parameters)
        {
            this.parameters = parameters;
        }

        /// <inheritdoc/>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownserhip dispose transferred")]
        public NodeContainerFormat Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            source.Stream.Position = 0;

            var result = new NodeContainerFormat();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var reader = new DataReader(source.Stream)
            {
                DefaultEncoding = Encoding.GetEncoding(1252),
                Endianness = EndiannessMode.BigEndian,
            };

            string magicId = reader.ReadString(4);

            if (magicId == "SLLZ")
            {
                var subStream = new DataStream(source.Stream, 0, source.Stream.Length);
                var compressed = new ParFile(subStream);
                source = (ParFile)ConvertFormat.With<Sllz.Decompressor>(compressed);
                source.Stream.Position = 0;

                reader = new DataReader(source.Stream)
                {
                    DefaultEncoding = Encoding.GetEncoding(1252),
                    Endianness = EndiannessMode.BigEndian,
                };

                magicId = reader.ReadString(4);
            }

            if (magicId != "PARC")
            {
                throw new FormatException("PARC: Bad magic Id.");
            }

            if (parameters.Tags == null)
            {
                parameters.Tags = new Dictionary<string, dynamic>();
            }

            parameters.Tags["PlatformId"] = reader.ReadByte();
            byte endianness = reader.ReadByte();
            parameters.Tags["Endianness"] = endianness;
            parameters.Tags["SizeExtended"] = reader.ReadByte();
            parameters.Tags["Relocated"] = reader.ReadByte();

            if (endianness == 0x00)
            {
                reader.Endianness = EndiannessMode.LittleEndian;
            }

            parameters.Tags["Version"] = reader.ReadInt32();
            parameters.Tags["DataSize"] = reader.ReadInt32();

            int totalFolderCount = reader.ReadInt32();
            int folderInfoOffset = reader.ReadInt32();
            int totalFileCount = reader.ReadInt32();
            int fileInfoOffset = reader.ReadInt32();

            var folderNames = new string[totalFolderCount];
            for (var i = 0; i < totalFolderCount; i++)
            {
                folderNames[i] = reader.ReadString(0x40).TrimEnd('\0');
                if (folderNames[i].Length < 1)
                {
                    folderNames[i] = ".";
                }
            }

            var fileNames = new string[totalFileCount];
            for (var i = 0; i < totalFileCount; i++)
            {
                fileNames[i] = reader.ReadString(0x40).TrimEnd('\0');
            }

            reader.Stream.Seek(folderInfoOffset);
            var folders = new Node[totalFolderCount];

            for (var i = 0; i < totalFolderCount; i++)
            {
                folders[i] = new Node(folderNames[i], new NodeContainerFormat())
                {
                    Tags =
                    {
                        ["FolderCount"] = reader.ReadInt32(),
                        ["FirstFolderIndex"] = reader.ReadInt32(),
                        ["FileCount"] = reader.ReadInt32(),
                        ["FirstFileIndex"] = reader.ReadInt32(),
                        ["Attributes"] = reader.ReadInt32(),
                        ["Unused1"] = reader.ReadInt32(),
                        ["Unused2"] = reader.ReadInt32(),
                        ["Unused3"] = reader.ReadInt32(),
                    },
                };
            }

            reader.Stream.Seek(fileInfoOffset);
            var files = new Node[totalFileCount];

            for (var i = 0; i < totalFileCount; i++)
            {
                uint compressionFlag = reader.ReadUInt32();
                uint size = reader.ReadUInt32();
                uint compressedSize = reader.ReadUInt32();
                uint baseOffset = reader.ReadUInt32();
                int attributes = reader.ReadInt32();
                uint extendedOffset = reader.ReadUInt32();
                ulong timestamp = reader.ReadUInt64();

                long offset = ((long)extendedOffset << 32) | baseOffset;
                var file = new ParFile(source.Stream, offset, compressedSize)
                {
                    CanBeCompressed = false, // Don't try to compress if the original was not compressed.
                    IsCompressed = compressionFlag == 0x80000000,
                    WasCompressed = compressionFlag == 0x80000000,
                    CompressionVersion = 0,
                    DecompressedSize = size,
                    Attributes = attributes,
                    Timestamp = timestamp,
                };

                if (file.IsCompressed)
                {
                    reader.Stream.PushToPosition(offset + 5);
                    file.CompressionVersion = reader.ReadByte();
                    reader.Stream.PopPosition();
                }

                files[i] = new Node(fileNames[i], file)
                {
                    Tags = { ["Timestamp"] = timestamp, },
                };
            }

            BuildTree(folders[0], folders, files, this.parameters);

            result.Root.Add(folders[0]);

            return result;
        }

        private static void BuildTree(Node node, IReadOnlyList<Node> folders, IReadOnlyList<Node> files, ParArchiveReaderParameters parameters)
        {
            int firstFolderIndex = node.Tags["FirstFolderIndex"];
            int folderCount = node.Tags["FolderCount"];
            for (int i = firstFolderIndex; i < firstFolderIndex + folderCount; i++)
            {
                node.Add(folders[i]);
                BuildTree(folders[i], folders, files, parameters);
            }

            int firstFileIndex = node.Tags["FirstFileIndex"];
            int fileCount = node.Tags["FileCount"];
            for (int i = firstFileIndex; i < firstFileIndex + fileCount; i++)
            {
                if (parameters.Recursive && files[i].Name.EndsWith(".par", StringComparison.InvariantCultureIgnoreCase))
                {
                    var recursiveParameters = new ParArchiveReaderParameters()
                    {
                        Recursive = true,
                        Tags = new Dictionary<string, dynamic>(),
                    };

                    files[i].TransformWith<ParArchiveReader, ParArchiveReaderParameters>(recursiveParameters);

                    foreach (var tag in recursiveParameters.Tags)
                        files[i].Tags.Add(tag);
                }

                node.Add(files[i]);
            }
        }
    }
}
