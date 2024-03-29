﻿// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
namespace ParLibrary
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Yarhl.FileFormat;
    using Yarhl.IO;

    /// <summary>
    /// Represents a file stored in a PAR archive.
    /// </summary>
    public class ParFile : BinaryFormat, IConverter<BinaryFormat, ParFile>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParFile"/> class.
        /// </summary>
        public ParFile()
        {
            this.CanBeCompressed = true;
            this.IsCompressed = false;
            this.WasCompressed = false;
            this.CompressionVersion = 0;
            this.DecompressedSize = 0;
            this.Attributes = 0x00000020;
            this.FileDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParFile"/> class.
        /// </summary>
        /// <param name="stream">The data stream.</param>
        public ParFile(DataStream stream)
            : base(stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            this.CanBeCompressed = true;
            this.IsCompressed = false;
            this.WasCompressed = false;
            this.CompressionVersion = 0;
            this.DecompressedSize = (uint)stream.Length;
            this.Attributes = 0x00000020;
            this.FileDate = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParFile"/> class.
        /// </summary>
        /// <param name="stream">The base stream.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="length">Data length.</param>
        public ParFile(DataStream stream, long offset, long length)
            : base(stream, offset, length)
        {
            this.CanBeCompressed = true;
            this.IsCompressed = false;
            this.WasCompressed = false;
            this.CompressionVersion = 0;
            this.DecompressedSize = (uint)length;
            this.Attributes = 0x00000020;
            this.FileDate = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the file can be compressed.
        /// </summary>
        public bool CanBeCompressed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file is compressed.
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the file was originally compressed.
        /// </summary>
        public bool WasCompressed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what version the file was originally compressed with.
        /// </summary>
        public byte CompressionVersion { get; set; }

        /// <summary>
        /// Gets or sets the file size (decompressed).
        /// </summary>
        public uint DecompressedSize { get; set; }

        /// <summary>
        /// Gets or sets the file attributes.
        /// </summary>
        public int Attributes { get; set; }

        /// <summary>
        /// Gets or sets the file date (as ulong).
        /// </summary>
        public ulong Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the file date (as DateTime).
        /// </summary>
        public DateTime FileDate
        {
            get
            {
                var baseDate = new DateTime(1970, 1, 1);
                return baseDate.AddSeconds(this.Timestamp);
            }

            set
            {
                var baseDate = new DateTime(1970, 1, 1);
                this.Timestamp = (ulong)(value - baseDate).TotalSeconds;
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownserhip dispose transferred")]
        public ParFile Convert(BinaryFormat source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new ParFile(source.Stream, 0, source.Stream.Length);
        }
    }
}