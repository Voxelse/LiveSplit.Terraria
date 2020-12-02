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

        protected string bossChecklistUrl = null;
        protected Size? bossChecklistSize = null;
        protected Point? bossChecklistLocation = null;

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
                bossChecklistUrl = bossCheckList["Url"].InnerText;
                string sizeText = bossCheckList["Size"].InnerText;
                int sizeSeparator = sizeText.IndexOf(',');
                bossChecklistSize = new Size(Int32.Parse(sizeText.Substring(0, sizeSeparator)), Int32.Parse(sizeText.Substring(sizeSeparator + 1)));
                string locText = bossCheckList["Location"].InnerText;
                int locSeparator = locText.IndexOf(',');
                bossChecklistLocation = new Point(Int32.Parse(locText.Substring(0, locSeparator)), Int32.Parse(locText.Substring(locSeparator + 1)));
            }
        }

        public override XmlNode GetSettings(XmlDocument doc) {
            XmlElement xmlElement = (XmlElement)base.GetSettings(doc);
            TerrariaMemory mem = (TerrariaMemory)memory;
            if(!mem.bossChecklist?.IsDisposed ?? false) {
                XmlElement xmlChecklist = doc.CreateElement("BossChecklist");
                xmlChecklist.AppendChild(doc.ToElement("Url", mem.bossChecklist.Url));
                xmlChecklist.AppendChild(doc.ToElement("Size", mem.bossChecklist.Size.Width + "," + mem.bossChecklist.Size.Height));
                xmlChecklist.AppendChild(doc.ToElement("Location", mem.bossChecklist.Location.X + "," + mem.bossChecklist.Location.Y));
                xmlElement.AppendChild(xmlChecklist);
            }
            return xmlElement;
        }

        private void OptionChanged(object sender, OptionEventArgs e) {
            if(e.Name == EOption.BossChecklist.ToString()) {
                TerrariaMemory mem = (TerrariaMemory)memory;
                if(mem.bossChecklist?.IsDisposed ?? true) {
                    mem.bossChecklist = new TerrariaBossChecklist();
                    if(bossChecklistUrl != null) {
                        mem.bossChecklist.Url = bossChecklistUrl;
                    }
                    if(bossChecklistSize != null) {
                        mem.bossChecklist.Size = (Size)bossChecklistSize;
                    }
                    if(bossChecklistLocation != null) {
                        mem.bossChecklist.StartPosition = FormStartPosition.Manual;
                        mem.bossChecklist.Location = (Point)bossChecklistLocation;
                    }
                    mem.bossChecklist.Disposed += (s, evt) => mem.bossChecklist = null;
                    mem.bossChecklist.Show();
                } else {
                    mem.bossChecklist.BringToFront();
                }
            }
        }

        public override void Dispose() {
            settings.OptionChanged -= OptionChanged;
            base.Dispose();
        }
    }
}