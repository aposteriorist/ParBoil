using ParLibrary;
using System;
using System.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Yarhl.FileFormat;
using Yarhl.FileSystem;
using Yarhl.IO;

namespace ParBoil.RGGFormats
{
    public class MSGFormat : RGGFormat, IConverter<ParFile, MSGFormat>
    {
        public MSGFormat() : base() { }
        public MSGFormat(DataStream stream) : base(stream) { }
        public MSGFormat(DataStream stream, long offset, long length) : base(stream, offset, length) { }

        internal override Control Handle { get; set; }
        internal override List<Control> EditedControls { get; set; }

        public Section[] Sections { get; set; }
        public MiscEntry[] Misc { get; set; }

        public struct MiscEntry
        {
            public uint Pointer;
            public string Export;
            public string Import;
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
            public ushort Bytecount;
            [JsonPropertyName("Function Count")]
            public byte FunctionCount;
            [JsonPropertyName("Message Pointer")]
            public uint Pointer;
            [JsonPropertyName("Function Table")]
            public uint FunctionTable;
            internal Function[] _functions;
            public string[] Functions;
            public int SpeakerIndex;
            public string Export;
            public string Import;
            internal long relativeOffset;
        }
        public struct Function
        {
            public byte Type;
            public byte Subtype;
            public short[] Args;
        }

        public MSGFormat Convert(ParFile source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var msgStream = new DataStream();
            source.Stream.WriteTo(msgStream);

            return new MSGFormat(msgStream)
            {
                CanBeCompressed = source.CanBeCompressed,
                IsCompressed = source.IsCompressed,
                DecompressedSize = source.DecompressedSize,
                Attributes = source.Attributes,
                Timestamp = source.Timestamp,
            };
        }

        public override void LoadFromBin()
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

            Misc = new MiscEntry[miscount];
            reader.Stream.Seek(misc_ptr);
            for (int e = 0; e < miscount; e++)
            {
                var entry = new MiscEntry();
                entry.Pointer = reader.ReadUInt32();
                reader.Stream.PushToPosition(entry.Pointer);
                entry.Export = reader.ReadString();
                entry.Import = entry.Export;
                reader.Stream.PopPosition();

                Misc[e] = entry;
            }

            reader.Stream.Seek(table_ptr);
            uint table = reader.ReadUInt32();
            ushort chairs = reader.ReadUInt16();

            Sections = new Section[chairs];
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
                        if (message.SpeakerIndex >= Misc.Length)
                            message.SpeakerIndex = -1;

                        header.Messages[m] = message;

                        reader.Stream.PopPosition();
                    }

                    section.Headers[h] = header;

                    reader.Stream.PopPosition();
                    reader.Stream.Position += 10;
                }

                Sections[s] = section;

                reader.Stream.PopPosition();
                reader.Stream.Position += 6;
            }
        }

        public override void LoadFromJSON(DataStream jsonStream)
        {
            byte[] json = new byte[jsonStream.Length];
            jsonStream.Read(json, 0, (int)jsonStream.Length);

            var opts = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            MSGFormat msg = JsonSerializer.Deserialize<MSGFormat>(json, opts);
            Misc = msg.Misc;
            Sections = msg.Sections;
            
            // Parse the function table strings.
            for (int s = 0; s < Sections.Length; s++)
                for (int h = 0; h < Sections[s].HeaderCount; h++)
                    for (int m = 0;  m < Sections[s].Headers[h].MessageCount; m++)
                    {
                        var message = Sections[s].Headers[h].Messages[m];
                        message._functions = new Function[message.FunctionCount];

                        for (int f = 0; f < message.FunctionCount; f++)
                        {
                            var fstr = message.Functions[f];
                            var _args = fstr[7..^1].Split(',');

                            var function = new Function()
                            {
                                Type = byte.Parse(fstr[0..2], NumberStyles.AllowHexSpecifier),
                                Subtype = byte.Parse(fstr[3..5], NumberStyles.AllowHexSpecifier),
                                Args = new short[9],
                            };

                            for (int a = 0; a < 9; a++)
                                function.Args[a] = System.Convert.ToInt16(_args[a]);

                            message._functions[f] = function;
                        }

                        Sections[s].Headers[h].Messages[m] = message;
                    }

            CanBeCompressed = msg.CanBeCompressed;
            IsCompressed = msg.IsCompressed;
            DecompressedSize = msg.DecompressedSize;
            Attributes = msg.Attributes;
            Timestamp = msg.Timestamp;
        }

        public override string ToJSONString()
        {
            var opts = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            //return JsonSerializer.Serialize(new object[2] { misc, sections }, opts);
            return JsonSerializer.Serialize(this, opts);
        }

        public override DataStream ToJSONStream()
        {
            var opts = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            //byte[] json = JsonSerializer.SerializeToUtf8Bytes(new object[2] { misc, sections }, opts);
            byte[] json = JsonSerializer.SerializeToUtf8Bytes(this, opts);
            return DataStreamFactory.FromArray(json, 0, json.Length);
        }

        public override void GenerateControls(Size formSize, Color formForeColor, Color formEditableColor, Color formBackColor, Font formFont)
        {
            EditedControls = new List<Control>();

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

            for (int m = 0; m < Misc.Length; m++)
            {
                var panel = new FlowLayoutPanel()
                {
                    Width = miscPanel.Width - 100,
                    Height = 50,
                    //BackColor = Color.FromArgb(65, 65, 65),
                    BackColor = formBackColor,
                    AutoScroll = true,
                    Margin = new Padding(0, 20, 0, 0),
                };

                var export = new RichTextBox()
                {
                    Width = 300,
                    Height = 40,
                    BackColor = panel.BackColor,
                    ForeColor = formForeColor,
                    Font = formFont,
                    Text = Misc[m].Export,
                    Multiline = false,
                    ReadOnly = true,
                    Margin = new Padding(30,0,15,0),
                    Tag = m,
                };

                var import = new RichTextBox()
                {
                    Width = 300,
                    Height = 40,
                    BackColor = formEditableColor,
                    ForeColor = formForeColor,
                    Font = formFont,
                    Text = Misc[m].Import,
                    Multiline = false,
                    Margin = new Padding(15,0,3,0),
                    Tag = m,             
                };
                import.LostFocus += delegate
                {
                    if (import.CanUndo)
                    {
                        if (!EditedControls.Contains(import)) EditedControls.Add(import);
                    }
                    else
                    {
                        if (EditedControls.Contains(import)) EditedControls.Remove(import);
                    }
                    UpdateSpeaker((int)import.Tag, import.Text);
                    Debug.WriteLine($"Number of edits: {EditedControls.Count}");
                };

                panel.Controls.Add(export);
                panel.Controls.Add(import);
                miscPanel.Controls.Add(panel);
            }

            miscTab.Controls.Add(miscPanel);
            topTabs.TabPages.Add(miscTab);


            // Populate sections and headers
            uint s = 0;
            foreach (var section in Sections)
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

                for (uint h = 0; h < section.Headers.Length; h++)
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

                    for (uint m = 0; m < header.MessageCount; m++)
                    {
                        var message = header.Messages[m];

                        var panel = new FlowLayoutPanel()
                        {
                            Width = mp.Width - 60,
                            Height = 260,
                            //BackColor = Color.FromArgb(65, 65, 65),
                            BackColor = formBackColor,
                            //AutoScroll = true,
                            Tag = m,
                        };

                        var label = new Label()
                        {
                            Text = $"Message {m + 1}:    ",
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

                        if (message.SpeakerIndex >= 0)
                        {
                            var speakerExport = new TextBox()
                            {
                                Width = 120,
                                Height = 20,
                                BackColor = panel.BackColor,
                                ForeColor = formForeColor,
                                Font = formFont,
                                BorderStyle = BorderStyle.None,
                                Text = Misc[message.SpeakerIndex].Export,
                                ReadOnly = true,
                                // Margin = new Padding(0, 0, exportPanel.Width, 3);
                                Tag = message.SpeakerIndex,
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
                                Text = Misc[message.SpeakerIndex].Import,
                                ReadOnly = true,
                                // Margin = new Padding(0, 0, importPanel.Width, 3),
                                Tag = message.SpeakerIndex,
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
                            Tag = m,
                        };
                        InitializeText(textExport, message, false);
                        exportPanel.Controls.Add(textExport);

                        var textImport = new RichTextBox()
                        {
                            Height = 140,
                            Width = exportPanel.Width - 2,
                            BackColor = formEditableColor,
                            ForeColor = formForeColor,
                            Font = formFont,
                            Tag = (s, h, m),
                        };
                        InitializeText(textImport, message, true);
                        if (textExport.Text == "") textImport.ReadOnly = true;
                        textImport.LostFocus += delegate
                        {
                            if (textImport.CanUndo)
                            {
                                if (!EditedControls.Contains(textImport)) EditedControls.Add(textImport);
                            }
                            else
                            {
                                if (EditedControls.Contains(textImport)) EditedControls.Remove(textImport);
                            }
                            Debug.WriteLine($"Number of edits: {EditedControls.Count}");
                        };
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
            if (Misc[entry].Import == import)
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

            Misc[entry].Import = import;
        }

        private void InitializeText(RichTextBox box, Message msg, bool import)
        {
            box.Clear();
            box.SelectionStart = 0;

            string text = import ? msg.Import : msg.Export;

            if (text == null || text.Length == 0) return;

            // Note: Code assumes well-formed text in RGG style. But that's true throughout, really.
            int i = 0, j = text.IndexOf("<Color:", i);
            while (j != -1 && i < text.Length)
            {
                box.AppendText(text[i..j]);
                i = text.IndexOf('>', j);
                byte[] argb = text[(j+7)..i].Split(',').Select(s => byte.Parse(s)).ToArray();
                box.SelectionColor = Color.FromArgb(argb[3], argb[0], argb[1], argb[2]);
                box.ScrollToCaret();

                j = text.IndexOf("<Color:", ++i);

                box.AppendText(text[i..j]);
                box.SelectionColor = box.ForeColor;
                box.ScrollToCaret();

                i = text.IndexOf('>', j) + 1;
                j = text.IndexOf("<Color:", i);
            }

            box.AppendText(text[i..]);

            while (box.CanUndo)
                box.ClearUndo();
        }

        // Logic inserted into UpdateMessage's loop, rather than calling the function directly.
        private string WetToDry(RichTextBox box)
        {
            var goro = new StringBuilder();

            box.SelectionStart = 0;
            box.SelectionLength = 1;
            bool openColorTag = false;

            foreach (char ch in box.Text)
            {
                if (!openColorTag && box.SelectionColor != box.ForeColor)
                {
                    Color c = box.SelectionColor;
                    goro.Append($"<Color:{c.R},{c.G},{c.B},{c.A}>");
                    openColorTag = true;
                }
                else if (openColorTag && box.SelectionColor == box.ForeColor)
                {
                    goro.Append("<Color:Default>");
                    openColorTag = false;
                }

                goro.Append(ch);

                box.SelectionStart++;
                box.SelectionLength = 1;
            }

            return goro.ToString();
        }

        public override void FormClosing()
        {
            foreach (RichTextBox box in EditedControls)
            {
                if (box.Tag is ValueTuple<uint, uint, uint>(var s, var h, var m))
                    UpdateMessage(box, s, h, m);
            }

            EditedControls.Clear();

            WriteToBin();
        }

        private void UpdateMessage(RichTextBox box, uint s, uint h, uint m)
        {
            if (box.CanUndo)
            {
                var msg = Sections[s].Headers[h].Messages[m];
                var goro = new StringBuilder();

                var funcs = new List<Function>();
                bool textHandled = false;
                foreach (var func in Sections[s].Headers[h].Messages[m]._functions)
                {
                    if (func.Type != 2)
                    {
                        if (func.Type == 1)
                        {
                            func.Args[2] = (short)box.TextLength;
                        }
                        funcs.Add(func);
                    }

                    else if (func.Subtype == 0xb || func.Subtype == 0xc || func.Subtype == 0x13) // Intentionally ignoring 020e (JIS parens function)
                        funcs.Add(func);

                    else if (!textHandled)
                    {
                        // Go through char-by-char and update functions as done in msgtool.
                        short charCount = 0;
                        box.SelectionStart = 0;
                        box.SelectionLength = 1;
                        bool openColorTag = false;
                        var newFunc = new Function() { Type = 2, Args = new short[9] };
                        foreach (char ch in box.Text)
                        {
                            if (ch == '<')
                            {
                                newFunc.Subtype = 0xa;
                                newFunc.Args[2] = charCount;
                                charCount++;
                            }

                            else if (newFunc.Type == 0xa)
                            {
                                if (ch == '>')
                                {
                                    string tag = box.Text[newFunc.Args[2]..(box.SelectionStart + 1)];
                                    short.TryParse(tag[6..^1], out newFunc.Args[3]); // 0 for letters (e.g. Sign:D), for now
                                    newFunc.Args[4] = (short)tag.Length;
                                    funcs.Add(newFunc);
                                    newFunc = new Function() { Type = 2, Args = new short[9] };
                                }
                            }

                            else if (ch != '\r')
                            {
                                if (!openColorTag && box.SelectionColor != box.ForeColor)
                                {
                                    openColorTag = true;
                                    Color c = box.SelectionColor;
                                    newFunc.Subtype = 7;
                                    string tag = $"<Color:{c.R},{c.G},{c.B},{c.A}>";
                                    goro.Append(tag);
                                    newFunc.Args[2] = charCount;
                                    newFunc.Args[4] = (short)tag.Length;
                                    Array.Copy(new byte[4] { c.A, c.R, c.G, c.B }, 0, newFunc.Args, 5, 4);
                                    funcs.Add(newFunc);
                                    newFunc = new Function() { Type = 2, Args = new short[9] };
                                }
                                else if (openColorTag && box.SelectionColor == box.ForeColor)
                                {
                                    openColorTag = false;
                                    newFunc.Subtype = 8;
                                    string tag = "<Color:Default>";
                                    goro.Append(tag);
                                    newFunc.Args[2] = charCount;
                                    newFunc.Args[4] = (short)tag.Length;
                                    funcs.Add(newFunc);
                                    newFunc = new Function() { Type = 2, Args = new short[9] };
                                }

                                charCount++;

                                if (ch == ',' || ch == '、')
                                {
                                    newFunc.Subtype = 1;
                                    newFunc.Args[0] = 0xa;
                                    newFunc.Args[2] = charCount;
                                    funcs.Add(newFunc);
                                    newFunc = new Function() { Type = 2, Args = new short[9] };
                                }
                                else if (ch == '.' || ch == '!' || ch == '?' || ch == '。' || ch == '！' || ch == '？')
                                {
                                    if (funcs.Count > 0 && funcs[^1].Subtype == 1 && funcs[^1].Args[2] == charCount - 1)
                                    {
                                        // Repeating Ender, e.g. "..."
                                        newFunc = funcs[^1];
                                        newFunc.Args[2]++;
                                        funcs.RemoveAt(funcs.Count - 1);
                                        funcs.Add(newFunc);
                                        newFunc = new Function() { Type = 2, Args = new short[9] };
                                    }
                                    else
                                    {
                                        newFunc.Subtype = 1;
                                        newFunc.Args[0] = 0x14;
                                        newFunc.Args[2] = charCount;
                                        funcs.Add(newFunc);
                                        newFunc = new Function() { Type = 2, Args = new short[9] };
                                    }
                                }
                            }

                            goro.Append(ch);
                            box.SelectionStart++;
                            box.SelectionLength = 1;
                        }
                        textHandled = true;
                    }
                }

                msg._functions = funcs.ToArray();
                msg.FunctionCount = (byte)funcs.Count;
                msg.Import = goro.ToString();

                msg.Functions = new string[funcs.Count];

                int f = 0;
                foreach (var func in funcs)
                    msg.Functions[f++] = String.Format("{0:X2} {1:X2} ({2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10})",
                                func.Type, func.Subtype, func.Args[0], func.Args[1],
                                func.Args[2], func.Args[3], func.Args[4], func.Args[5],
                                func.Args[6], func.Args[7], func.Args[8]);

                Sections[s].Headers[h].Messages[m] = msg;
            }
        }

        public override void WriteToBin()
        {
            var writer = new DataWriter(Stream)
            {
                DefaultEncoding = Encoding.GetEncoding(932),
                Endianness = EndiannessMode.BigEndian,
            };

            uint offset = Sections[0].Headers[0].Messages[0].FunctionTable; // Cheap hack


            var funcs = new DataWriter(new DataStream())
            {
                DefaultEncoding = Encoding.GetEncoding(932),
                Endianness = EndiannessMode.BigEndian,
            };

            var text = new DataWriter(new DataStream())
            {
                DefaultEncoding = Encoding.GetEncoding(932),
                Endianness = EndiannessMode.BigEndian,
            };

            foreach (var section in Sections)
                foreach (var header in section.Headers)
                    for (int m = 0; m < header.MessageCount; m++)
                    {
                        var message = header.Messages[m];
                        message.relativeOffset = text.Stream.Length;
                        text.Write(message.Import, true);
                        message.Bytecount = (ushort)(text.Stream.Length - message.relativeOffset - 1);
                        message.FunctionTable = (uint)(offset + funcs.Stream.Length);

                        foreach (var func in message._functions)
                        {
                            funcs.Write(func.Type); funcs.Write(func.Subtype);
                            funcs.Write(func.Args[0]); funcs.Write(func.Args[1]);
                            funcs.Write(func.Args[2]); funcs.Write(func.Args[3]);
                            funcs.Write(func.Args[4]); funcs.Write((byte)func.Args[5]);
                            funcs.Write((byte)func.Args[6]); funcs.Write((byte)func.Args[7]);
                            funcs.Write((byte)func.Args[8]);
                        }

                        header.Messages[m] = message;
                    }

            // Around twice, unfortunately.
            foreach (var section in Sections)
                foreach (var header in section.Headers)
                {
                    writer.Stream.Seek(header.Pointer);

                    foreach (var message in header.Messages)
                    {
                        writer.Write(message.Bytecount);
                        writer.Write(message.FunctionCount);
                        writer.Stream.Seek(1, SeekMode.Current);
                        writer.Write((uint)(offset + funcs.Stream.Length + message.relativeOffset));
                        writer.Write(message.FunctionTable);
                    }
                }

            uint len = (uint)(offset + funcs.Stream.Length + text.Stream.Length);
            text.WriteTimes(0, 4 - len % 4);
            len += 4 - len % 4;

            writer.Stream.Seek(0x3c);
            writer.Write(len);


            offset = len + (uint)(4 * Misc.Length);

            var mtext = new DataWriter(new DataStream())
            {
                DefaultEncoding = Encoding.GetEncoding(932),
                Endianness = EndiannessMode.BigEndian,
            };

            foreach (var entry in Misc)
            {
                text.Write((uint)(offset + mtext.Stream.Length));
                mtext.Write(entry.Import, true);
            }

            writer.Stream.Seek(offset - text.Stream.Length - funcs.Stream.Length);
            funcs.Stream.WriteTo(writer.Stream);
            text.Stream.WriteTo(writer.Stream);
            mtext.Stream.WriteTo(writer.Stream);

            writer.Stream.WriteTo("jeff.msg");

            DecompressedSize = (uint)Stream.Length;
        }
    }
}
