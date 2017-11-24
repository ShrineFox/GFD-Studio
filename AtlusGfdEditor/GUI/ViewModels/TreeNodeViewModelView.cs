﻿using System.Windows.Forms;

namespace AtlusGfdEditor.GUI.ViewModels
{
    public class TreeNodeViewModelView : TreeView
    {
        public new TreeNodeViewModel TopNode
        {
            get => ( TreeNodeViewModel )base.TopNode;
            set => base.TopNode = value;
        }

        /// <summary>
        /// Sets the first visible node in the tree. Clears all nodes if not empty.
        /// </summary>
        /// <param name="node"></param>
        public void SetTopNode( TreeNodeViewModel node )
        {
            // clear nodes if not empty
            if ( Nodes.Count != 0 )
            {
                Nodes.Clear();
            }

            // initialize its view as it will be the first visible node
            node.InitializeView();

            // add top node
            Nodes.Add( node );
        }

        public void RefreshSelection()
        {
            OnAfterSelect( new TreeViewEventArgs( SelectedNode ) );
        }

        public void ExpandNode( TreeNodeViewModel viewModel )
        {
            // check if the first child node is a dummy node
            if ( viewModel.Nodes.Count > 0 && viewModel.Nodes[0].Text == string.Empty )
            {
                // initialize the view so the user doesn't get to see the dummy node
                viewModel.InitializeView();
            }

            foreach ( TreeNodeViewModel childNode in viewModel.Nodes )
            {
                if ( childNode.Nodes.Count == 0 && childNode.NodeFlags.HasFlag( TreeNodeViewModelFlags.Branch ) )
                {
                    // HACK: add a dummy node for each branch
                    // so the expand icon shows up even when a node hasn't initialized yet
                    childNode.Nodes.Add( new TreeNode() );
                }
            }
        }

        protected override void OnAfterSelect( TreeViewEventArgs e )
        {
            // initialize view for selected node
            var adapter = ( TreeNodeViewModel )e.Node;
            adapter.InitializeView();

            base.OnAfterSelect( e );
        }

        protected override void OnNodeMouseClick( TreeNodeMouseClickEventArgs e )
        {
            if ( e.Button == MouseButtons.Right )
                SelectedNode = e.Node;
        }

        protected override void OnAfterExpand( TreeViewEventArgs e )
        {
            ExpandNode( ( TreeNodeViewModel )e.Node );

            base.OnAfterExpand( e );
        }
    }
}