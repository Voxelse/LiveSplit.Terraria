using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace LiveSplit.Terraria {
    public partial class TerrariaBossChecklist : Form {

        private static readonly Dictionary<string, string> BossLookup = new Dictionary<string, string> {
            { "WallofFlesh", "wall_of_flesh" },
            { EBosses.EyeofCthulhu.ToString(), "eye_of_cthulhu" },
            { EBosses.EaterofWorldsBrainofCthulhu.ToString(), "eater_and_brain" },
            { EBosses.Skeletron.ToString(), "skeletron" },
            { EBosses.QueenBee.ToString(), "queen_bee" },
            { EBosses.KingSlime.ToString(), "king_slime" },
            { EBosses.Plantera.ToString(), "plantera" },
            { EBosses.Golem.ToString(), "golem" },
            { EBosses.DukeFishron.ToString(), "duke_fishron" },
            { EBosses.LunaticCultist.ToString(), "lunatic_cultist" },
            { EBosses.MoonLord.ToString(), "moon_lord" },
            { EBosses.EmpressofLight.ToString(), "empress_of_light" },
            { EBosses.QueenSlime.ToString(), "queen_slime" },
            { EBosses.TheDestroyer.ToString(), "the_destroyer" },
            { EBosses.TheTwins.ToString(), "the_twins" },
            { EBosses.SkeletronPrime.ToString(), "skeletron_prime" },
        };

        public TerrariaBossChecklist() {
            InitializeComponent();
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            HtmlDocument doc = WebBrowser.Document;
            HtmlElement head = doc.GetElementsByTagName("head")[0];
            HtmlElement s = doc.CreateElement("script");
            s.SetAttribute("text", @"
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
            ");
            head.AppendChild(s);
        }

        private void WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e) {
            e.Cancel = true;
            Process.Start(e.Url.ToString());
        }

        public void CheckBoss(string bossName) {
            if(BossLookup.TryGetValue(bossName, out string outName)) {
                WebBrowser.Document.InvokeScript("checkBoss", new string[] { outName });
            }
        }

        public void ResetBosses() {
            WebBrowser.Document.InvokeScript("resetBosses");
        }
    }
}