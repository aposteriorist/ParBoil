// -------------------------------------------------------
// © Kaplas. Licensed under MIT. See LICENSE for details.
// -------------------------------------------------------
using Yarhl.FileSystem;

namespace ParLibrary.Converter
{
    /// <summary>
    /// Parameters for ParArchiveReader.
    /// </summary>
    public class ParArchiveReaderParameters
    {
        /// <summary>
        /// Gets or sets a value indicating whether the reading is recursive.
        /// </summary>
        public bool Recursive { get; set; }

        /// <summary>
        /// Gets or sets a Dictionary equivalent to the Tags field from the Node class.
        /// </summary>
        public IDictionary<string, dynamic>? Tags { get; set; }
    }
}
