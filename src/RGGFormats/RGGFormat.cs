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
        public RGGFormat() : base() { }
        public RGGFormat(DataStream stream) : base(stream) { }
        public RGGFormat(DataStream stream, long offset, long length) : base(stream, offset, length) { }

        public abstract Control Handle { get; set; }

        public abstract void GenerateControls(Size formSize, Color ForeColor, Color EditableColor, Color BackColor, Font font);

        public abstract void ResizeAll(Size size);
        public abstract void Resize();

        public abstract Type TypeOfHandle();
    }
}
