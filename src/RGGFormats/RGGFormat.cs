using ParLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yarhl.FileFormat;
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

        public abstract void LoadFromBin();
        public abstract void LoadFromJSON(DataStream json);
        public abstract RGGFormat CopyFormat();

        public abstract void GenerateControls(Size formSize, Color ForeColor, Color EditableColor, Color BackColor, Font font);
        public abstract void UpdateControls();
        public abstract void ResizeAll(Size size);
        public abstract void Resize();

        public abstract void ProcessEdits();
        public abstract void RevertEdits();
        public abstract DataStream AsBinStream(bool overwrite = false);
        public abstract DataStream ToJSONStream();
        public abstract string ToJSONString();
        public abstract void FormClosing();
    }
}
