using LiveSplit.Model;
using LiveSplit.VoxSplitter;

namespace LiveSplit.Terraria {
    public class TerrariaComponent : Component {
        protected override SettingInfo? Reset => null;
        public TerrariaComponent(LiveSplitState state) : base(state) {
            memory = new TerrariaMemory(logger);
            settings = new TreeSettings(state, Start, Reset, Options);
            settings.SetSettings(state.Run.AutoSplitterSettings);
        }
    }
}