using ParLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil.RGGFormats
{
    public class MSGFormat : ParFile, IConverter<ParFile, MSGFormat>
    {
        public MSGFormat() : base() { }
        public MSGFormat(DataStream stream) : base(stream) { }
        public MSGFormat(DataStream stream, long offset, long length) : base(stream, offset, length) { }

        public Section[] sections { get; set; }
        public MiscEntry[] misc { get; set; }

        public struct MiscEntry
        {
            public uint pointer;
            public string export;
            public string import;
        }

        public struct Section
        {
            public uint pointer;
            public uint headerCount;
            public Header[] headers;
        }
        public struct Header
        {
            public uint pointer;
            public uint messageCount;
            public Message[] messages;
        }
        public struct Message
        {
            public uint bytecount;
            public byte functionCount;
            public uint pointer;
            public uint functionTable;
            public Function[] functions;
            public string export;
            public string import;
            public string speakerExport;
            public string speakerImport;
        }
        public struct Function
        {
            public byte type;
            public byte subtype;
            public ushort[] args;
        }

        public MSGFormat Convert (ParFile source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var msg = new MSGFormat(source.Stream, 0, source.Stream.Length);

            var reader = new DataReader(source.Stream) {
                DefaultEncoding = Encoding.GetEncoding(932),
                Endianness = EndiannessMode.BigEndian,
            };

            reader.Stream.Seek(0x10);
            int table_ptr = reader.ReadInt32();
            reader.Stream.Seek(0x3c);
            int misc_ptr = reader.ReadInt32();
            short miscount = reader.ReadInt16();

            msg.misc = new MiscEntry[miscount];
            reader.Stream.Seek(misc_ptr);
            for (int e = 0; e < miscount; e++)
            {
                var misc = new MiscEntry();
                misc.pointer = reader.ReadUInt32();
                reader.Stream.PushToPosition(misc.pointer);
                misc.export = reader.ReadString();
                reader.Stream.PopPosition();

                msg.misc[e] = misc;
            }

            reader.Stream.Seek(table_ptr);
            uint table = reader.ReadUInt32();
            ushort chairs = reader.ReadUInt16();

            msg.sections = new Section[chairs];
            reader.Stream.Seek(table + 6);
            for (int s = 0; s < chairs; s++)
            {
                var section = new Section();
                section.headerCount = reader.ReadUInt16();
                section.pointer = reader.ReadUInt32();
                section.headers = new Header[section.headerCount];

                reader.Stream.PushToPosition(section.pointer + 4);

                for (int h = 0; h < section.headerCount; h++)
                {
                    var header = new Header();
                    header.pointer = reader.ReadUInt32();
                    header.messageCount = reader.ReadUInt16();
                    header.messages = new Message[header.messageCount];

                    reader.Stream.PushToPosition(header.pointer);

                    for (int m = 0; m < header.messageCount; m++)
                    {
                        var message = new Message();
                        message.bytecount = reader.ReadUInt16();
                        message.functionCount = reader.ReadByte();
                        reader.Stream.Position++;
                        message.pointer = reader.ReadUInt32();
                        message.functionTable = reader.ReadUInt32();
                        message.functions = new Function[message.functionCount];

                        reader.Stream.PushToPosition(message.functionTable);

                        for (int f = 0; f < message.functionCount; f++)
                        {
                            var function = new Function();
                            function.type = reader.ReadByte();
                            function.subtype = reader.ReadByte();
                            function.args = new ushort[9];

                            function.args[0] = reader.ReadUInt16();
                            function.args[1] = reader.ReadUInt16();
                            function.args[2] = reader.ReadUInt16();
                            function.args[3] = reader.ReadUInt16();
                            function.args[4] = reader.ReadUInt16();
                            function.args[5] = reader.ReadByte();
                            function.args[6] = reader.ReadByte();
                            function.args[7] = reader.ReadByte();
                            function.args[8] = reader.ReadByte();

                            message.functions[f] = function;
                        }

                        reader.Stream.Seek(message.pointer);
                        message.export = reader.ReadString();
                        if (message.functions[0].args[3] < msg.misc.Length)
                            message.speakerExport = msg.misc[message.functions[0].args[3]].export;

                        header.messages[m] = message;

                        reader.Stream.PopPosition();
                    }

                    section.headers[h] = header;

                    reader.Stream.PopPosition();
                    reader.Stream.Position += 10;
                }

                msg.sections[s] = section;

                reader.Stream.PopPosition();
                reader.Stream.Position += 6;
            }

            return msg;
        }

        public void gimme()
        {
            foreach (var s in sections)
                foreach (var h in s.headers)
                    foreach (var m in h.messages)
                        Debug.WriteLine(m.export);
        }
    }
}
