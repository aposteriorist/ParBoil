using ParLibrary;
using System.Composition;
using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Yarhl.FileFormat;
using Yarhl.IO;

namespace ParBoil.RGGFormats
{
    public class MSGFormat : RGGFormat, IConverter<ParFile, MSGFormat>
    {
        public MSGFormat() : base() { }
        public MSGFormat(DataStream stream) : base(stream) { }
        public MSGFormat(DataStream stream, long offset, long length) : base(stream, offset, length) { }

        public Section[] sections { get; set; }
        public MiscEntry[] misc { get; set; }
        public Event[] events { get; set; }
        internal override Control Handle { get; set; }
        public override uint EditCount { get; set; }

        public struct MiscEntry
        {
            public uint Pointer;
            public string Export;
            internal string Import;
        }

        public struct Section
        {
            [JsonPropertyName("Section Pointer")]
            public uint Pointer;
            [JsonPropertyName("Header Count")]
            public uint HeaderCount;
            public Header[] Headers;
        }
        public struct Header
        {
            [JsonPropertyName("Header Pointer")]
            public uint Pointer;
            [JsonPropertyName("Message Count")]
            public uint MessageCount;
            public Message[] Messages;
        }
        public struct Message
        {
            public uint Bytecount;
            [JsonPropertyName("Function Count")]
            public byte FunctionCount;
            [JsonPropertyName("Message Pointer")]
            public uint Pointer;
            [JsonPropertyName("Function Table")]
            public uint FunctionTable;
            internal Function[] _functions;
            public string[] Functions;
            internal int SpeakerIndex;
            public string Export;
            internal string Import;
        }
        public struct Function
        {
            public byte Type;
            public byte Subtype;
            public short[] Args;
        }

        public struct Event
        {
            string Type;
            int Index;
            string TagType;
            uint TagLength;
            uint[] RGBA;
            string SignID;
        }

        public MSGFormat Convert(ParFile source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new MSGFormat(source.Stream, 0, source.Stream.Length);
        }

        public override void Load()
        { 
            var reader = new DataReader(Stream) {
                DefaultEncoding = Encoding.GetEncoding(932),
                Endianness = EndiannessMode.BigEndian,
            };

            reader.Stream.Seek(0x10);
            int table_ptr = reader.ReadInt32();
            reader.Stream.Seek(0x3c);
            int misc_ptr = reader.ReadInt32();
            short miscount = reader.ReadInt16();

            misc = new MiscEntry[miscount];
            reader.Stream.Seek(misc_ptr);
            for (int e = 0; e < miscount; e++)
            {
                var entry = new MiscEntry();
                entry.Pointer = reader.ReadUInt32();
                reader.Stream.PushToPosition(entry.Pointer);
                entry.Export = reader.ReadString();
                entry.Import = entry.Export;
                reader.Stream.PopPosition();

                misc[e] = entry;
            }

            reader.Stream.Seek(table_ptr);
            uint table = reader.ReadUInt32();
            ushort chairs = reader.ReadUInt16();

            sections = new Section[chairs];
            reader.Stream.Seek(table + 6);
            for (int s = 0; s < chairs; s++)
            {
                var section = new Section();
                section.HeaderCount = reader.ReadUInt16();
                section.Pointer = reader.ReadUInt32();
                section.Headers = new Header[section.HeaderCount];

                reader.Stream.PushToPosition(section.Pointer + 4);

                for (int h = 0; h < section.HeaderCount; h++)
                {
                    var header = new Header();
                    header.Pointer = reader.ReadUInt32();
                    header.MessageCount = reader.ReadUInt16();
                    header.Messages = new Message[header.MessageCount];

                    reader.Stream.PushToPosition(header.Pointer);

                    for (int m = 0; m < header.MessageCount; m++)
                    {
                        var message = new Message();
                        message.Bytecount = reader.ReadUInt16();
                        message.FunctionCount = reader.ReadByte();
                        reader.Stream.Position++;
                        message.Pointer = reader.ReadUInt32();
                        message.FunctionTable = reader.ReadUInt32();
                        message._functions = new Function[message.FunctionCount];
                        message.Functions = new string[message.FunctionCount];

                        reader.Stream.PushToPosition(message.FunctionTable);

                        for (int f = 0; f < message.FunctionCount; f++)
                        {
                            var function = new Function();
                            function.Type = reader.ReadByte();
                            function.Subtype = reader.ReadByte();
                            function.Args = new short[9];

                            function.Args[0] = reader.ReadInt16();
                            function.Args[1] = reader.ReadInt16();
                            function.Args[2] = reader.ReadInt16();
                            function.Args[3] = reader.ReadInt16();
                            function.Args[4] = reader.ReadInt16();
                            function.Args[5] = reader.ReadByte();
                            function.Args[6] = reader.ReadByte();
                            function.Args[7] = reader.ReadByte();
                            function.Args[8] = reader.ReadByte();

                            message.Functions[f] = String.Format("{0:X2} {1:X2} ({2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})",
                                function.Type, function.Subtype, function.Args[0], function.Args[1],
                                function.Args[2], function.Args[3], function.Args[4], function.Args[5],
                                function.Args[6], function.Args[7], function.Args[8]);

                            message._functions[f] = function;
                        }

                        reader.Stream.Seek(message.Pointer);
                        message.Export = reader.ReadString();
                        message.Import = message.Export;
                        message.SpeakerIndex = message._functions[0].Args[3];
                        if (message.SpeakerIndex >= misc.Length)
                            message.SpeakerIndex = -1;

                        header.Messages[m] = message;

                        reader.Stream.PopPosition();
                    }

                    section.Headers[h] = header;

                    reader.Stream.PopPosition();
                    reader.Stream.Position += 10;
                }

                sections[s] = section;

                reader.Stream.PopPosition();
                reader.Stream.Position += 6;
            }
        }

        public override string ToJSONString()
        {
            var opts = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            return JsonSerializer.Serialize(new object[2] { misc, sections }, opts);
        }

        public override byte[] ToJSON()
        {
            var opts = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            return JsonSerializer.SerializeToUtf8Bytes(new object[2] { misc, sections }, opts);
        }

        public void gimme()
        {
            foreach (var s in sections)
                foreach (var h in s.Headers)
                    foreach (var m in h.Messages)
                        Debug.WriteLine(m.Export);
        }

        private MiscEntry[] buffer_misc;
        public override void GenerateControls(Size formSize, Color formForeColor, Color formEditableColor, Color formBackColor, Font formFont)
        {
            //binToJSON();
            if (buffer_misc == null)
            {
                buffer_misc = new MiscEntry[misc.Length];
                misc.CopyTo(buffer_misc, 0);
            }

            // Tabs are ugly. Could I do this custom with menu strips or buttons?
            // I already have one panel per tab, all I should need to do is attach that panel to a generated button,
            // which puts its associated panel centre-stage when it's pressed.

            TabControl topTabs = new TabControl()
            {
                Dock = DockStyle.Fill,
                //Appearance = TabAppearance.Buttons,
            };

            // Populate the misc. entries.
            TabPage miscTab = new TabPage()
            {
                Text = "Misc.",
                Size = formSize,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0),
            };

            var miscPanel = new FlowLayoutPanel()
            {
                Height = miscTab.Height,
                Width = miscTab.Width,
                AutoScroll = true,
                BackColor = formBackColor,
                Margin = new Padding(0),
            };

            for (int m = 0; m < misc.Length; m++)
            {
                var panel = new FlowLayoutPanel()
                {
                    Width = miscPanel.Width - 100,
                    Height = 50,
                    //BackColor = Color.FromArgb(65, 65, 65),
                    BackColor = formBackColor,
                    AutoScroll = true,
                    Margin = new Padding(0, 20, 0, 0),
                    Tag = false,
                };

                var export = new TextBox()
                {
                    Width = 200,
                    Height = 30,
                    BackColor = panel.BackColor,
                    ForeColor = formForeColor,
                    Font = formFont,
                    Text = misc[m].Export,
                    ReadOnly = true,
                    Margin = new Padding(30,0,15,0),
                    Tag = m,
                };

                var import = new TextBox()
                {
                    Width = 200,
                    Height = 30,
                    BackColor = formEditableColor,
                    ForeColor = formForeColor,
                    Font = formFont,
                    Text = misc[m].Import,
                    Margin = new Padding(15,0,3,0),
                    Tag = m,             
                };
                import.LostFocus += delegate
                {
                    panel.Tag = misc[(int)import.Tag].Export == import.Text;
                    UpdateSpeaker((int)import.Tag, import.Text);
                };

                panel.Controls.Add(export);
                panel.Controls.Add(import);
                miscPanel.Controls.Add(panel);
            }

            miscTab.Controls.Add(miscPanel);
            topTabs.TabPages.Add(miscTab);


            // Populate sections and headers
            uint s = 0;
            foreach (var section in sections)
            {
                TabPage sectionTab = new TabPage()
                {
                    Text = $"Section {s + 1}",
                    Size = formSize,
                    BorderStyle = BorderStyle.None,
                    Margin = new Padding(0),
                };

                TabControl tabs = new TabControl()
                {
                    Dock = DockStyle.Fill,
                    //Appearance = TabAppearance.Buttons,
                };

                for (int h = 0; h < section.Headers.Length; h++)
                {
                    var header = section.Headers[h];

                    TabPage headerTab = new TabPage()
                    {
                        Text = $"Header {h + 1}",
                        Height = sectionTab.Height - 96,
                        Width = sectionTab.Width - 32,
                        BackColor = formBackColor,
                        BorderStyle = BorderStyle.None,
                        Margin = new Padding(0),
                    };
                    if (header.MessageCount > 1) headerTab.Text += $" ({header.MessageCount})";

                    var mp = new FlowLayoutPanel();
                    mp.Height = headerTab.Height;
                    mp.Width = headerTab.Width;
                    mp.AutoScroll = true;
                    mp.BackColor = formBackColor;
                    mp.Margin = new Padding(0);
                    //mp.Dock = DockStyle.Fill;

                    for (int i = 0; i < header.MessageCount; i++)
                    {
                        var panel = new FlowLayoutPanel()
                        {
                            Width = mp.Width - 60,
                            Height = 260,
                            //BackColor = Color.FromArgb(65, 65, 65),
                            BackColor = formBackColor,
                            //AutoScroll = true,
                        };

                        var label = new Label()
                        {
                            Text = $"Message {i + 1}:    ",
                            Margin = new Padding(0, 0, panel.Width, 3),
                        };
                        panel.Controls.Add(label);

                        var exportPanel = new FlowLayoutPanel()
                        {
                            Width = panel.Width / 2 - 30,
                            Height = panel.Height - 2,
                            Margin = new Padding(30, 0, 24, 0),
                        };

                        var importPanel = new FlowLayoutPanel
                        {
                            Width = panel.Width / 2 - 30,
                            Height = panel.Height - 2,
                            Margin = new Padding(0, 0, 0, 0),
                        };

                        if (header.Messages[i].SpeakerIndex >= 0)
                        {
                            var speakerExport = new TextBox()
                            {
                                Width = 120,
                                Height = 20,
                                BackColor = panel.BackColor,
                                ForeColor = formForeColor,
                                Font = formFont,
                                BorderStyle = BorderStyle.None,
                                Text = misc[header.Messages[i].SpeakerIndex].Export,
                                ReadOnly = true,
                                // Margin = new Padding(0, 0, exportPanel.Width, 3);
                                Tag = header.Messages[i].SpeakerIndex,
                            };
                            exportPanel.Controls.Add(speakerExport);

                            var speakerImport = new TextBox()
                            {
                                Width = 120,
                                Height = 20,
                                BackColor = panel.BackColor,
                                ForeColor = formForeColor,
                                Font = formFont,
                                BorderStyle = BorderStyle.None,
                                Text = misc[header.Messages[i].SpeakerIndex].Import,
                                ReadOnly = true,
                                // Margin = new Padding(0, 0, importPanel.Width, 3),
                                Tag = header.Messages[i].SpeakerIndex,
                            };
                            importPanel.Controls.Add(speakerImport);
                        }
                        else
                        {
                            panel.Height -= 40;
                            exportPanel.Height -= 40;
                            importPanel.Height -= 40;
                        }

                        var textExport = new RichTextBox()
                        {
                            Height = 140,
                            Width = exportPanel.Width - 2,
                            BackColor = panel.BackColor,
                            ForeColor = formForeColor,
                            Font = formFont,
                            ReadOnly = true,
                        };
                        InitializeText(textExport, header.Messages[i], false);
                        exportPanel.Controls.Add(textExport);

                        var textImport = new RichTextBox()
                        {
                            Height = 140,
                            Width = exportPanel.Width - 2,
                            BackColor = formEditableColor,
                            ForeColor = formForeColor,
                            Font = formFont,
                        };
                        InitializeText(textImport, header.Messages[i], true);
                        if (textImport.Text == "") textImport.ReadOnly = true;
                        importPanel.Controls.Add(textImport);

                        panel.Controls.Add(exportPanel);
                        panel.Controls.Add(importPanel);

                        mp.Controls.Add(panel);
                        headerTab.Controls.Add(mp);
                    }

                    tabs.TabPages.Add(headerTab);

                }

                tabs.SelectedIndexChanged += delegate { Resize(); };
                sectionTab.Controls.Add(tabs);
                topTabs.TabPages.Add(sectionTab);
                s++;
            }

            topTabs.SelectedIndexChanged += delegate { Resize(); };
            Handle = topTabs;
        }

        public override void ResizeAll(Size formSize)
        {
            var topTabs = (TabControl)Handle;

            uint s = 0;
            foreach (TabPage sectionTab in topTabs.TabPages)
            {
                if (s == 0)
                {
                    var mp = (FlowLayoutPanel)sectionTab.Controls[0];
                    mp.Height = sectionTab.Height;
                    mp.Width = sectionTab.Width;
                }
                else
                {
                    var tabs = (TabControl)sectionTab.Controls[0];
                    foreach (TabPage headerTab in tabs.TabPages)
                    {
                        // Size the same as in GenerateControls
                        var mp = (FlowLayoutPanel)headerTab.Controls[0];
                        mp.Height = headerTab.Height;
                        mp.Width = headerTab.Width;

                        //foreach (FlowLayoutPanel panel in mp.Controls)
                        //{

                        //}
                    }
                }

                s++;
            }

            topTabs.Refresh();
        }

        public override void Resize()
        {
            var topTabs = (TabControl)Handle;

            var s = topTabs.SelectedIndex;
            var sectionTab = topTabs.SelectedTab;

            if (s == 0)
            {
                var mp = (FlowLayoutPanel)sectionTab.Controls[0];
                mp.Height = sectionTab.Height;
                mp.Width = sectionTab.Width;
            }
            else
            {
                var tabs = (TabControl)sectionTab.Controls[0];
                var headerTab = tabs.SelectedTab;

                // Size the same as in GenerateControls
                var mp = (FlowLayoutPanel)headerTab.Controls[0];
                mp.Height = headerTab.Height;
                mp.Width = headerTab.Width;

                //foreach (FlowLayoutPanel panel in mp.Controls)
                //{

                //}

            }

            sectionTab.Refresh();
        }

        private void UpdateSpeaker (int entry, string import)
        {
            if (buffer_misc[entry].Import == import)
                return;

            var topTabs = (TabControl)Handle;

            for (int s = 1; s < topTabs.TabCount; s++)
            {
                var sectionTab = topTabs.TabPages[s];

                var tabs = (TabControl)sectionTab.Controls[0];
                foreach (TabPage headerTab in tabs.TabPages)
                {
                    var mp = (FlowLayoutPanel)headerTab.Controls[0];
                    foreach (FlowLayoutPanel panel in mp.Controls)
                    {
                        var importPanel = (FlowLayoutPanel)panel.Controls[2];
                        if (importPanel.Controls.Count > 1)
                        {
                            var speakerImport = (TextBox)importPanel.Controls[0];

                            if ((int)speakerImport.Tag == entry)
                                speakerImport.Text = import;
                        }
                    }
                }
            }

            buffer_misc[entry].Import = import;
        }

        // TO-DO: private void UpdateText
        //  When the text changes in the textbox, update the function table as a separate function.

        private void InitializeText(RichTextBox box, Message msg, bool import)
        {
            string text = import ? msg.Import : msg.Export;

            if (text == null || text.Length == 0) return;

            int ignored = 0;
            int crlf = 0;
            int disc = 0;

            foreach (var func in msg._functions)
            {
                if (func.Type == 2)
                {
                    switch (func.Subtype)
                    {
                        case 7:
                            disc = text[(box.TextLength + ignored + crlf)..(func.Args[2] + ignored + crlf)].Count(f => f == '\r');
                            box.AppendText(text[(box.TextLength + ignored + crlf)..(func.Args[2] + ignored + crlf + disc)]);
                            ignored += func.Args[4];
                            crlf += disc;
                            box.SelectionColor = Color.FromArgb(func.Args[5], func.Args[6], func.Args[7], func.Args[8]);
                            box.ScrollToCaret();
                            break;
                        case 8:
                            // Assumption: CRLF will never be encountered as coloured text.
                            //disc = text[(box.TextLength + ignored + crlf)..(func.args[2] + ignored + crlf)].Count(f => f == '\r');
                            box.AppendText(text[(box.TextLength + ignored + crlf)..(func.Args[2] + ignored + crlf)]);
                            box.SelectionColor = box.ForeColor;
                            box.ScrollToCaret();
                            //crlf += disc;
                            ignored += func.Args[4];
                            break;
                    }
                }
            }

            if (box.TextLength + ignored + crlf < msg.Export.Length)
                box.AppendText(text[(box.TextLength + ignored + crlf)..]);
        }

        public void topTabs_GotFocus(object sender, EventArgs e)
        {
            Resize();
        }

        public override Type TypeOfHandle()
        {
            return typeof(TabControl);
        }
    }
}
