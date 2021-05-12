using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using Voxif.AutoSplitter;
using Voxif.IO;

[assembly: ComponentFactory(typeof(Factory))]
namespace LiveSplit.Terraria {
    public partial class TerrariaComponent : Voxif.AutoSplitter.Component {
        
        public enum EOption {
            [Description("Boss Checklist"), Type(typeof(OptionButton))]
            BossChecklist
        }

        protected override SettingsInfo? ResetSettings => null;
        protected override OptionsInfo? OptionsSettings => new OptionsInfo(new string[0], CreateControlsFromEnum<EOption>());

        protected TerrariaMemory memory;

        private BossChecklistData? bossChecklistData = null;
        private TerrariaBossChecklist bossChecklist;

        public TerrariaComponent(LiveSplitState state) : base(state) {
            System.Diagnostics.Trace.WriteLine("cctor");
#if DEBUG
            logger = new ConsoleLogger();
#else
            logger = new FileLogger("_" + Factory.ExAssembly.GetName().Name.Substring(10) + ".log");
#endif
            logger.StartLogger();

            memory = new TerrariaMemory(logger);

            settings = new TreeSettings(state, StartSettings, ResetSettings, OptionsSettings);
            settings.OptionChanged += OptionChanged;
        }

        public override void SetSettings(XmlNode settings) {
            base.SetSettings(settings);

            XmlElement bossCheckList = settings["BossChecklist"];
            if(bossCheckList != null) {
                string[] sizeText = bossCheckList["Size"].InnerText.Split(',');
                string[] locText = bossCheckList["Location"].InnerText.Split(',');
                bossChecklistData = new BossChecklistData(
                    bossCheckList["Url"].InnerText,
                    new Size(Int32.Parse(sizeText[0]), Int32.Parse(sizeText[1])),
                    new Point(Int32.Parse(locText[0]), Int32.Parse(locText[1]))
                );
                if(bossCheckList["Open"] != null && Boolean.Parse(bossCheckList["Open"].InnerText)) {
                    OpenBossChecklist();
                }
            }
        }

        public override XmlNode GetSettings(XmlDocument doc) {
            XmlElement xmlElement = (XmlElement)base.GetSettings(doc);

            if(bossChecklist != null || bossChecklistData != null) {
                XmlElement xmlChecklist = doc.CreateElement("BossChecklist");

                bool isOpen = bossChecklist != null;
                xmlChecklist.AppendChild(doc.ToElement("Open", isOpen));
                
                string url = isOpen ? bossChecklist.Url : bossChecklistData?.url;
                xmlChecklist.AppendChild(doc.ToElement("Url", url));
                
                Size size = isOpen ? bossChecklist.Size : (Size)bossChecklistData?.size;
                xmlChecklist.AppendChild(doc.ToElement("Size", size.Width + "," + size.Height));
                
                Point location = isOpen ? bossChecklist.Location : (Point)bossChecklistData?.location;
                xmlChecklist.AppendChild(doc.ToElement("Location", location.X + "," + location.Y));

                xmlElement.AppendChild(xmlChecklist);
            }
            return xmlElement;
        }

        private void OptionChanged(object sender, OptionEventArgs e) {
            switch(Enum.Parse(typeof(EOption), e.Name)) {
                case EOption.BossChecklist:
                    OpenBossChecklist();
                    break;
            }
        }

        private void OpenBossChecklist() {
            if(bossChecklist == null) {
                bossChecklist = new TerrariaBossChecklist();
                if(bossChecklistData != null) {
                    bossChecklist.Url = bossChecklistData?.url;
                    bossChecklist.Size = (Size)(bossChecklistData?.size);
                    bossChecklist.StartPosition = FormStartPosition.Manual;
                    bossChecklist.Location = (Point)bossChecklistData?.location;
                }
                bossChecklist.Disposed += (s, e) => bossChecklist = null;
                bossChecklist.Show();
            } else {
                bossChecklist.BringToFront();
            }
        }

        public override void Dispose() {
            settings.OptionChanged -= OptionChanged;
            bossChecklist?.Dispose();
            memory.Dispose();
            base.Dispose();
        }

        private struct BossChecklistData {
            public string url;
            public Size size;
            public Point location;

            public BossChecklistData(string url, Size size, Point location) {
                this.url = url;
                this.size = size;
                this.location = location;
            }
        }
    }
}