using LiveSplit.VoxSplitter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace LiveSplit.Terraria {
    public partial class TerrariaBossChecklist : Form {

        private static readonly Dictionary<string, string> BossLookup = new Dictionary<string, string> {
            { "WallofFlesh", "wall_of_flesh" },
            { EBosses.EyeofCthulhu.ToString(), "cthulhuForm" },
            { EBosses.EaterofWorldsBrainofCthulhu.ToString(), "eaterBrainForm" },
            { EBosses.Skeletron.ToString(), "skeletron" },
            { EBosses.QueenBee.ToString(), "queen_bee" },
            { EBosses.KingSlime.ToString(), "king_slime" },
            { EBosses.Plantera.ToString(), "planteraForm" },
            { EBosses.Golem.ToString(), "golem" },
            { EBosses.DukeFishron.ToString(), "duke_fishron" },
            { EBosses.LunaticCultist.ToString(), "lunatic_cultist" },
            { EBosses.MoonLord.ToString(), "moon_lord" },
            { EBosses.EmpressofLight.ToString(), "empress_of_light" },
            { EBosses.QueenSlime.ToString(), "queen_slime" },
            { EBosses.TheDestroyer.ToString(), "the_destroyer" },
            { EBosses.TheTwins.ToString(), "twinsForm" },
            { EBosses.SkeletronPrime.ToString(), "skeletron_prime" },
        };

        private const string SiteURL = "https://dryoshiyahu.github.io/terraria-boss-checklist/";

        private bool isRunning = false;

        private HashSet<int> bossOffsets;
        private bool isHardmode;

        public TerrariaBossChecklist() {
            InitializeComponent();
            WebBrowser.Url = new Uri(SiteURL);
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            HtmlDocument doc = WebBrowser.Document;
            HtmlElement head = doc.GetElementsByTagName("head")[0];
            HtmlElement script = doc.CreateElement("script");
            script.SetAttribute("text", @"
                function checkBoss(name) {
                    document.querySelector('.checklist .boss.' + name).click();
                }

                function resetBosses() {
                    document.querySelectorAll('.checklist .boss').forEach(function(e) {
                        if(e.getAttribute('style') && !e.style.backgroundImage.endsWith('dark.png"")')) {
                            e.click();
                        }
                    });
                }

                function getForm(name) {
                    return document.querySelector('#' + name + ' input:checked').value;
                }
            ");
            head.AppendChild(script);
        }

        private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            if(!e.Url.ToString().StartsWith(SiteURL)) {
                e.Cancel = true;
                Process.Start(e.Url.ToString());
            }
        }

        public string Url {
            get {
                string[] url = ((string)WebBrowser.Document.InvokeScript("getQueryUrl")).Split('?');
                return url.Length > 1 ? url[1] : "";
            }
            set {
                WebBrowser.Url = new Uri(SiteURL + (String.IsNullOrEmpty(value) ? "" : "?" + value));
            }
        }

        public void SetRunning(bool value) {
            isRunning = value;
            WebBrowser.Document.InvokeScript("resetBosses");
            bossOffsets = new HashSet<int>(TerrariaMemory.AllBosses.Cast<int>());
            isHardmode = false;
        }

        public void Update(TerrariaMemory memory) {
            if(!isRunning) { return; }

            foreach(int offset in bossOffsets.ToArray()) {
                if(memory.Game.Read<bool>(memory.Bosses.New + offset)) {
                    bossOffsets.Remove(offset);
                    CheckBoss(TerrariaMemory.GetBossName(offset));
                }
            }

            if(!isHardmode && memory.IsHardmode.New) {
                isHardmode = true;
                CheckBoss("WallofFlesh");
            }
        }

        public void CheckBoss(string bossName) {
            if(BossLookup.TryGetValue(bossName, out string outName)) {
                if(outName.EndsWith("Form")) {
                    outName = (string)WebBrowser.Document.InvokeScript("getForm", new object[] { outName });
                }
                WebBrowser.Document.InvokeScript("checkBoss", new string[] { outName });
            }
        }
    }
}