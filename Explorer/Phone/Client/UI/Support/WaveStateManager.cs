using System.Collections.Generic;
using System.Windows;
using Wave.Platform;

namespace Wave.UI
{
    public class WaveStateManager
    {
        public BlockState CurrentState { get; private set; }

        private Dictionary<BlockState, List<UIElement>> states = new Dictionary<BlockState, List<UIElement>>();

        public WaveStateManager(BlockState initialState = BlockState.Normal)
        {
            EnsureState(BlockState.Normal);
        }

        public void Add(BlockState state, UIElement item)
        {
            EnsureState(state);

            states[state].Add(item);
        }

        public void Remove(BlockState state, UIElement item)
        {
            EnsureState(state);

            states[state].Remove(item);
        }

        public void SwitchToState(BlockState state)
        {
            if (CurrentState != state)
            {
                EnsureState(CurrentState);

                // hide
                foreach (UIElement item in states[state])
                    item.Visibility = Visibility.Collapsed;
                
                EnsureState(state);

                // and show
                foreach (UIElement item in states[state])
                    item.Visibility = Visibility.Visible;
            }
        }

        private void EnsureState(BlockState state)
        {
            if (!states.ContainsKey(state))
                states[state] = new List<UIElement>();
        }
    }
}
