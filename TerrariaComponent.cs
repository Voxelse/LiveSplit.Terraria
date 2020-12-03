using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.Terraria {

    public enum EOption {
        [Description("Boss Checklist"), Type(typeof(OptionButton))]
        BossChecklist
    }

    public class TerrariaComponent : VoxSplitter.Component {

        private BossChecklistData? bossChecklistData = null;

        protected override SettingInfo? Reset => null;
        protected override OptionsInfo? Options => new OptionsInfo(new string[0], CreateControlsFromEnum<EOption>());
        public TerrariaComponent(LiveSplitState state) : base(state) {
            memory = new TerrariaMemory(state, logger);
            settings = new TreeSettings(state, Start, Reset, Options);
            settings.OptionChanged += OptionChanged;
        }

        public override void SetSettings(XmlNode settings) {
            base.SetSettings(settings);

            XmlElement bossCheckList = settings["BossChecklist"];
            if(bossCheckList != null) {
                string sizeText = bossCheckList["Size"].InnerText;
                int sizeSeparator = sizeText.IndexOf(',');
                string locText = bossCheckList["Location"].InnerText;
                int locSeparator = locText.IndexOf(',');
                bossChecklistData = new BossChecklistData(
                    bossCheckList["Url"].InnerText,
                    new Size(Int32.Parse(sizeText.Substring(0, sizeSeparator)), Int32.Parse(sizeText.Substring(sizeSeparator + 1))),
                    new Point(Int32.Parse(locText.Substring(0, locSeparator)), Int32.Parse(locText.Substring(locSeparator + 1)))
                );
                if(bossCheckList["Open"] != null && Boolean.Parse(bossCheckList["Open"].InnerText)) {
                    OpenBossChecklist();
                }
            }
        }

        public override XmlNode GetSettings(XmlDocument doc) {
            XmlElement xmlElement = (XmlElement)base.GetSettings(doc);

            TerrariaMemory mem = (TerrariaMemory)memory;
            if(mem.bossChecklist != null || bossChecklistData != null) {
                XmlElement xmlChecklist = doc.CreateElement("BossChecklist");

                bool isOpen = mem.bossChecklist != null;
                xmlChecklist.AppendChild(doc.ToElement("Open", isOpen));
                
                string url = isOpen ? mem.bossChecklist.Url : bossChecklistData?.url;
                xmlChecklist.AppendChild(doc.ToElement("Url", url));
                
                Size size = isOpen ? mem.bossChecklist.Size : (Size)bossChecklistData?.size;
                xmlChecklist.AppendChild(doc.ToElement("Size", size.Width + "," + size.Height));
                
                Point location = isOpen ? mem.bossChecklist.Location : (Point)bossChecklistData?.location;
                xmlChecklist.AppendChild(doc.ToElement("Location", location.X + "," + location.Y));

                xmlElement.AppendChild(xmlChecklist);
            }
            return xmlElement;
        }

        private void OptionChanged(object sender, OptionEventArgs e) {
            if(e.Name == EOption.BossChecklist.ToString()) {
                OpenBossChecklist();
            }
        }

        private void OpenBossChecklist() {
            TerrariaMemory mem = (TerrariaMemory)memory;
            if(mem.bossChecklist == null) {
                mem.bossChecklist = new TerrariaBossChecklist();
                if(bossChecklistData != null) {
                    mem.bossChecklist.Url = bossChecklistData?.url;
                    mem.bossChecklist.Size = (Size)(bossChecklistData?.size);
                    mem.bossChecklist.StartPosition = FormStartPosition.Manual;
                    mem.bossChecklist.Location = (Point)bossChecklistData?.location;
                }
                mem.bossChecklist.Disposed += (s, evt) => mem.bossChecklist = null;
                mem.bossChecklist.Show();
            } else {
                mem.bossChecklist.BringToFront();
            }
        }

        public override void Dispose() {
            settings.OptionChanged -= OptionChanged;
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