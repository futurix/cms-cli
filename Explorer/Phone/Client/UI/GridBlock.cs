using Wave.Platform;
using Wave.Platform.Messaging;

namespace Wave.UI
{
    public class GridBlock : ContainerBlock
    {
        private GridBlockDefinition Data
        {
            get { return Definition as GridBlockDefinition; }
        }

        public GridBlock(View hostView, Node parentNode, BlockBase parentBlock, BlockDefinition definition, FieldList content, bool isRoot)
            : base(hostView, parentNode, parentBlock, definition, content, isRoot)
        {
        }
    }
}
