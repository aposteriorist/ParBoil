using ParLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil.RGGFormats
{
    public abstract class RGGFormat : ParFile
    {
        public RGGFormat() : base()
        {
            //EditedControls = new List<Control>();
        }
        public RGGFormat(DataStream stream) : base(stream)
        {
            //EditedControls = new List<Control>();
        }
        public RGGFormat(DataStream stream, long offset, long length) : base(stream, offset, length)
        {
            //EditedControls = new List<Control>();
        }

        internal Control Handle { get; set; }
        internal List<Control> EditedControls { get; set; }

        internal bool TrackEdits { get; set; }
        internal bool Enabled { get; set; }

        public abstract void LoadFromBin();
        public abstract void LoadFromJSON(DataStream json);
        public abstract RGGFormat CopyFormat(DataStream? newStream = null, bool duplicateStream = false);

        public abstract void GenerateControls(Size formSize, Color ForeColor, Color EditableColor, Color BackColor, Font font);
        public abstract void UpdateControls();
        public abstract void EnableControls(bool enabled, Color newBackColor);
        public abstract void ResizeAll(Size size);
        public abstract void Resize();

        public abstract void ApplyEdits();
        public abstract void RevertEdits();
        public abstract DataStream UpdateStream(bool overwrite = false);
        public abstract DataStream ToJSONStream();
        public abstract string ToJSONString();
        public abstract void FormClosing();

        public static void TransformToRGGFormat(Node file)
        {
            if (file.Format is RGGFormat)
            {
                return;
            }

            switch (file.Name[^4..])
            {
                case ".msg":
                    if (file.Format is not MSGFormat)
                        file.TransformWith<MSGFormat>();
                    break;
            }
        }
    }
}
