using ParLibrary;
using System.Diagnostics;
using System.Text;
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
        public override Control Handle { get; set; }

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
            public int speakerIndex;
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
                misc.import = misc.export;
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
                        message.speakerIndex = message.functions[0].args[3];
                        if (message.speakerIndex >= msg.misc.Length)
                            message.speakerIndex = -1;
                            //message.speakerExport = msg.misc[message.speakerIndex].export;

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

            // msg.GenerateControls();

            return msg;
        }

        public void gimme()
        {
            foreach (var s in sections)
                foreach (var h in s.headers)
                    foreach (var m in h.messages)
                        Debug.WriteLine(m.export);
        }

        private MiscEntry[] buffer_misc;
        public override void GenerateControls(Size formSize, Color formForeColor, Color formEditableColor, Color formBackColor, Font formFont)
        {
            buffer_misc = new MiscEntry[misc.Length];
            misc.CopyTo(buffer_misc, 0);

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
                    Margin = new Padding(0,20,0,0),
                };

                var export = new TextBox()
                {
                    Width = 200,
                    Height = 30,
                    BackColor = panel.BackColor,
                    ForeColor = formForeColor,
                    Font = formFont,
                    Text = misc[m].export,
                    ReadOnly = true,
                    Margin = new Padding(30,0,15,0),
                };

                var import = new TextBox()
                {
                    Width = 200,
                    Height = 30,
                    BackColor = formEditableColor,
                    ForeColor = formForeColor,
                    Font = formFont,
                    Text = misc[m].import,
                    Margin = new Padding(15,0,3,0),
                    Tag = m,             
                };
                import.LostFocus += delegate { UpdateSpeaker((int)import.Tag, import.Text); };

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

                for (int h = 0; h < section.headers.Length; h++)
                {
                    var header = section.headers[h];

                    TabPage headerTab = new TabPage()
                    {
                        Text = $"Header {h + 1}",
                        Height = sectionTab.Height - 96,
                        Width = sectionTab.Width - 32,
                        BackColor = formBackColor,
                        BorderStyle = BorderStyle.None,
                        Margin = new Padding(0),
                    };
                    if (header.messageCount > 1) headerTab.Text += $" ({header.messageCount})";

                    var mp = new FlowLayoutPanel();
                    mp.Height = headerTab.Height;
                    mp.Width = headerTab.Width;
                    mp.AutoScroll = true;
                    mp.BackColor = formBackColor;
                    mp.Margin = new Padding(0);
                    //mp.Dock = DockStyle.Fill;

                    for (int i = 0; i < header.messageCount; i++)
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

                        if (header.messages[i].speakerIndex >= 0)
                        {
                            var speakerExport = new TextBox()
                            {
                                Width = 120,
                                Height = 20,
                                BackColor = panel.BackColor,
                                Font = formFont,
                                BorderStyle = BorderStyle.None,
                                Text = misc[header.messages[i].speakerIndex].export,
                                ReadOnly = true,
                                // Margin = new Padding(0, 0, exportPanel.Width, 3);
                                Tag = header.messages[i].speakerIndex,
                            };
                            exportPanel.Controls.Add(speakerExport);

                            var speakerImport = new TextBox()
                            {
                                Width = 120,
                                Height = 20,
                                BackColor = panel.BackColor,
                                Font = formFont,
                                BorderStyle = BorderStyle.None,
                                Text = misc[header.messages[i].speakerIndex].import,
                                ReadOnly = true,
                                // Margin = new Padding(0, 0, importPanel.Width, 3),
                                Tag = header.messages[i].speakerIndex,
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
                            Font = formFont,
                            // BorderStyle = BorderStyle.None,
                            Text = header.messages[i].export,
                            ReadOnly = true,
                        };
                        exportPanel.Controls.Add(textExport);

                        var textImport = new RichTextBox()
                        {
                            Height = 140,
                            Width = exportPanel.Width - 2,
                            BackColor = formEditableColor,
                            Font = formFont,
                            // BorderStyle = BorderStyle.None,
                            Text = header.messages[i].export,
                        };
                        if (textImport.Text == "")
                        {
                            textImport.ReadOnly = true;
                        }
                        importPanel.Controls.Add(textImport);

                        foreach (Control control in exportPanel.Controls)
                        {
                            control.ForeColor = formForeColor;
                        }
                        foreach (Control control in importPanel.Controls)
                        {
                            control.ForeColor = formForeColor;
                        }

                        //panel.Margin = new Padding(0, 0, 0, 40);

                        panel.Controls.Add(exportPanel);
                        panel.Controls.Add(importPanel);

                        //mainPanel.Controls.Add(panel);
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
            if (buffer_misc[entry].import == import)
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

                            if (speakerImport.Tag != null && (int)(speakerImport.Tag) == entry)
                                speakerImport.Text = import;
                        }
                    }
                }
            }

            buffer_misc[entry].import = import;
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
