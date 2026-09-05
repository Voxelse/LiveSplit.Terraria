using LiveSplit.Web.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Voxif.AutoSplitter;

namespace LiveSplit.Terraria {
    public class TerrariaSettings : TreeSettings {
        // Might remove
        private readonly Dictionary<string, string> groupSplits = new Dictionary<string, string> {
            { "HermesBoots", "Boots" },
            { "SandBoots", "Boots" },
            { "FlurryBoots", "Boots" },
            { "SailfishBoots", "Boots" }
        };

        public TerrariaSettings(LiveSplit.Model.LiveSplitState state, SettingsInfo? start, SettingsInfo? reset, OptionsInfo? options)
            : base(state, start, reset, options) {

            // TODO: TEST
            convertableSettings = new Dictionary<string, string> {
                { "Item_795", "Item_BloodButcherer" },
                { "Item_46", "Item_LightsBane" },
                { "Item_155", "Item_Muramasa" },
                { "Item_190", "Item_BladeofGrass" },
                { "Item_121", "Item_FieryGreatsword" },
                { "Item_273", "Item_NightsEdge" },
                { "Item_757", "Item_TerraBlade" },
                { "Item_4956", "Item_Zenith" },
                { "Item_5043", "Item_TorchGodsFavor" },
                { "Item_224", "Item_Bed" },
                { "Item_5066", "Item_BeeHive" },
                { "Item_4263", "Item_MagicConch" },
                { "Item_97", "Item_MusketBall" },
                { "Item_800", "Item_TheUndertaker" },
                { "Item_96", "Item_Musket" },
                { "Item_168", "Item_Grenade" },
                { "Item_4795", "Item_WallOfFleshGoatMountItem" },
                { "Item_964", "Item_Boomstick" },
                { "Item_534", "Item_Shotgun" },
                { "Item_122", "Item_MoltenPickaxe" },
                { "Item_1220", "Item_OrichalcumAnvil" },
                { "Item_525", "Item_MythrilAnvil" },
                { "Item_748", "Item_Jetpack" },
                { "Item_1229", "Item_ChlorophyteShotbow" },
                { "Item_2673", "Item_TruffleWorm" },
                { "Item_4988", "Item_QueenSlimeCrystal" },
                { "Item_1826", "Item_TheHorsemansBlade" },
                { "Item_2880", "Item_InfluxWaver" },
                { "Item_74", "Item_PlatinumCoin" }
            };
        }

        protected override void Settings_Load(object sender, EventArgs e) {
            if(FindForm() == null) {
                return;
            }

            base.Settings_Load(sender, e);
        }

        public override void SetSettings(XmlNode settings) {
            XmlNodeList splitNodes = settings.SelectNodes("Splits/Split");
            if(splitNodes != null) {
                bool addedAny = false;

                string parentCategory = "Items"; // Could have it so that it uses a new one...

                foreach(XmlNode node in splitNodes) {
                    string splitName = node.InnerText;

                    if(splitName.StartsWith("Item_") && !settingsDict.ContainsKey(splitName)) {
                        string itemName = splitName.Substring(5);

                        var newNode = new NewTreeNode(
                            index: settingsDict.Count,
                            type: ETreeNodeType.Split,
                            name: splitName,
                            desc: itemName,
                            parent: parentCategory,
                            ico: null, tipText: null, tipImg: null
                        );
                        settingsDict.Add(splitName, newNode);
                        addedAny = true;
                    }
                }

                if(addedAny) {
                    UpdateSorting(true);
                }
            }

            base.SetSettings(settings);
        }

        public void AddItemSplits(List<string> itemNames) {
            bool addedAny = false;
            List<string> skippedItems = new List<string>();

            string parentCategory = "Items"; // Could have it so that it uses a new one...

            SuspendDrawing();

            foreach(string itemName in itemNames) {
                // Check for dummy EItems that will set specific grouped splits
                if(itemName == "Boots") {
                    if(settingsDict.ContainsKey("Boots")) {
                        settingsDict["Boots"].Checked = true;
                        splits.Add("Boots");
                        continue;
                    }
                }

                string splitName = "Item_" + itemName;

                if(settingsDict.ContainsKey(splitName)) {
                    continue;
                }

                // Part of group like boots
                if(groupSplits.TryGetValue(itemName, out string groupName)) {
                    if(settingsDict.ContainsKey(groupName) && settingsDict[groupName].Checked) {
                        skippedItems.Add(itemName);
                        continue;
                    }
                }

                var newNode = new NewTreeNode(
                    index: settingsDict.Count,
                    type: ETreeNodeType.Split,
                    name: splitName,
                    desc: itemName,
                    parent: parentCategory,
                    ico: null, tipText: null, tipImg: null
                );

                settingsDict.Add(splitName, newNode);
                newNode.Checked = true;
                splits.Add(splitName);
                addedAny = true;
            }
            // EnsureVisible?

            if(addedAny) {
                UpdateSorting(true);
                if(parentCategory != null) {
                    settingsDict[parentCategory].Expand();
                }
            }

            ResumeDrawing();

            // Unsure if should keep, ask for feedback
            if(skippedItems.Count > 0) {
                MessageBox.Show(
                    "The following items were not added because they are already covered by the 'Boots' group split:\n\n" +
                    string.Join("\n", skippedItems),
                    "Items Already Covered"
                );
            }
        }
    }
}
