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
            EditCount = 0;
        }
        public RGGFormat(DataStream stream) : base(stream)
        {
            EditCount = 0;
        }
        public RGGFormat(DataStream stream, long offset, long length) : base(stream, offset, length)
        {
            EditCount = 0;
        }

        internal abstract Control Handle { get; set; }
        public abstract uint EditCount { get; set; }

        public abstract void Load();
        public abstract void GenerateControls(Size formSize, Color ForeColor, Color EditableColor, Color BackColor, Font font);

        public abstract void ResizeAll(Size size);
        public abstract void Resize();

        public abstract Type TypeOfHandle();

        public abstract byte[] ToJSON();
        public abstract string ToJSONString();
    }
}
