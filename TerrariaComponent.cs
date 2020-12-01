using LiveSplit.Model;
using LiveSplit.VoxSplitter;
using System.ComponentModel;

namespace LiveSplit.Terraria {

    public enum EOption {
        [Description("Boss Checklist"), Type(typeof(OptionButton))]
        BossChecklist
    }
    
    public class TerrariaComponent : VoxSplitter.Component {
        protected override SettingInfo? Reset => null;
        protected override OptionsInfo? Options => new OptionsInfo(new string[0], CreateControlsFromEnum<EOption>());
        public TerrariaComponent(LiveSplitState state) : base(state) {
            memory = new TerrariaMemory(state, logger);
            settings = new TreeSettings(state, Start, Reset, Options);
            settings.OptionChanged += OptionChanged;
        }

        private void OptionChanged(object sender, OptionEventArgs e) {
            if(e.Name == EOption.BossChecklist.ToString()) {
                ((TerrariaMemory)memory).ShowBossChecklist();
            }
        }

        public override void Dispose() {
            settings.OptionChanged -= OptionChanged;
            base.Dispose();
        }
    }
}