using ParLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        internal abstract Control Handle { get; set; }
        internal abstract List<Control> EditedControls { get; set; }

        public abstract void LoadFromBin();
        public abstract void LoadFromJSON(DataStream json);
        public abstract void GenerateControls(Size formSize, Color ForeColor, Color EditableColor, Color BackColor, Font font);

        public abstract void ResizeAll(Size size);
        public abstract void Resize();

        public abstract Type TypeOfHandle();

        public abstract DataStream ToJSONStream();
        public abstract string ToJSONString();
        public abstract void FormClosing();
    }
}
