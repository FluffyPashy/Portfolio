/*
 * Name: Tree.cs
 * Author: Rico Rothe
 * Created: 19. January 2023
 * Last Modified: 16. March 2023
 * License: None
 * Description: Class representing the AVL Tree data structure, with methods for adding, deleting, searching, and balancing the tree.
*/


namespace AVL_Tree

{
    public class Tree
    {
        public Node? Root { get; set; } = null;

        private readonly Func<int, int, CompareResult> compDel;
        
        public Tree(Func<int, int, CompareResult> compDel)
        {
            this.compDel = compDel ?? throw new ArgumentNullException(nameof(compDel));
        }


        /// <summary>
        /// Add a new value to the tree using the specified comparison delegate
        /// If Root is null, create a new node and assign it to Root
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            if (Root == null)
            {
                Root = new Node(value, compDel);
                return;
            }

            // Find the correct place to add the new node
            Root.Add(value);

            // Balance the tree after adding the new node
            Root = BalanceTree(Root);
        }


        /// <summary>
        /// Assign the target to the correct node to be deleted
        /// </summary>
        /// <param name="target"></param>
        public void Delete(int target)
        {
            if (Root == null) throw new InvalidOperationException(Constants.MSG_Tree_Empty);
            Root = Delete(Root, target);
        }
        
        /// <summary>
        /// Delete the target node by disconnecting it from the tree
        /// </summary>
        /// <param name="node"></param>
        /// <param name="target"></param>
        /// <returns> new child/node to be connected </returns>
        private Node? Delete(Node? node, int target)
        {
            Node parent;
            if (node == null) return null;
            else
            {
                //Balance Left subtree for deletion
                if (target < node.Value)
                {
                    node.LeftChild = Delete(node.LeftChild, target);
                    if (BalanceFactor(node) == -2)
                    {
                        if (BalanceFactor(node.RightChild) <= 0)
                        {
                            // Right Right Case
                            node = RotateRightRight(node);
                        }
                        else
                        {
                            // Right Left Case
                            node = RotateRightLeft(node);
                        }
                    }
                }
                //Balance Right subtree for deletion
                else if (target > node.Value) 
                {
                    node.RightChild = Delete(node.RightChild, target);
                    if (BalanceFactor(node) == 2)
                    {
                        if (BalanceFactor(node.LeftChild) >= 0)
                        {
                            // Left Left Case
                            node = RotateLeftLeft(node);
                        }
                        else
                        {
                            // Left Right Case
                            node = RotateLeftRight(node);
                        }
                    }
                }
                //Node to be deleted
                //Disconnect the node by rotating
                //so it will be "deleted" by C# garbage collector
                else
                {
                    if (node.RightChild != null)
                    {
                        // Assign the parent of the node to be deleted
                        parent = node.RightChild;
                        while (parent.LeftChild != null)
                        {
                            parent = parent.LeftChild;
                        }
                        node.Value = parent.Value;
                        node.RightChild = Delete(node.RightChild, parent.Value);

                        // Balance the tree after deleting the node
                        if (BalanceFactor(node) == 2)
                        {
                            if (BalanceFactor(node.LeftChild) >= 0)
                            {
                                node = RotateLeftLeft(node);
                            }
                            else
                            {
                                node = RotateLeftRight(node);
                            }
                        }
                    }
                    else
                    {
                        return node.LeftChild;
                    }
                }
            }
            return node;
        }

        /// <summary>
        /// This method returns true/false depending if the specified Value is stored in the tree
        /// </summary>
        /// <param name="value">The value to search for</param>
        /// <returns>True if the value is stored in the tree, false otherwise</returns>
        public bool IsValueStored(int value)
        {
            if(Root == null) throw new InvalidOperationException(Constants.MSG_Tree_Empty);

            var runner = Root;
            
            //Go through the tree to find the node with the specified value
            while(runner != null)
            {
                if(runner.Value == value) return true;
                if(runner.Value < value) runner = runner.RightChild;
                else runner = runner.LeftChild;
            }

            return false;
        }


        /// <summary>
        /// Find the node with the specified value and return it
        /// </summary>
        /// <param name="node"></param>
        /// <returns> node if found, else return null </returns>
        public Node? Find(Node node)
        {
            if (node == null) throw new InvalidOperationException(Constants.MSG_Tree_Empty);
            var runner = Root;
            while (runner != null) //Go through the tree to find the node
            {
                if (runner.Value == node.Value) return runner;
                if (runner.Value < node.Value) runner = runner.RightChild;
                else runner = runner.LeftChild;
            }

            return null;
        }

        /// <summary>
        /// Return the height of the tree, to be used for balancing
        /// </summary>
        /// <param name="node"></param>
        /// <returns> int height of the tree </returns>
        public int GetHeight(Node? node)
        {
            int height = 0;
            if(node != null)
            {
                // Get the height of the left and right subtrees recursively
                int leftHeight = GetHeight(node.LeftChild);
                int rightHeight = GetHeight(node.RightChild);

                // Compare the heights of the left and right subtrees
                // return the larger of the two
                int maxHeight = Math.Max(leftHeight, rightHeight);

                // Add 1 to the max height to account for the current node
                // after returning from the recursive calls
                height = maxHeight + 1;
            }
            return height;
        }


        /// <summary>
        /// Using the balance factor, determine if the tree is balanced
        /// making use of the GetHeight method
        /// </summary>
        /// <param name="node"></param>
        /// <returns> int balanceFactor </returns>
        private int BalanceFactor(Node? node)
        {
            if (node == null) return 0;

            int leftHeight = GetHeight(node.LeftChild);
            int rightHeight = GetHeight(node.RightChild);
            int balanceFactor = leftHeight - rightHeight;
            return balanceFactor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns> Node </returns>
        private Node BalanceTree(Node node)
        {
            int balanceFactor = BalanceFactor(node);
            if(balanceFactor > 1) // Left subtree is heavier
            {
                // Check if the left subtree is right heavy
                if (BalanceFactor(node.LeftChild) >= 0)
                {
                    node = RotateLeftLeft(node);
                }
                else
                {
                    node = RotateLeftRight(node);
                }
            }
            else if(balanceFactor < -1) // Right subtree is heavier
            {
                // Check if the right subtree is left heavy
                if (BalanceFactor(node.RightChild) > 0)
                {
                    node = RotateRightLeft(node);
                }
                else
                {
                    node = RotateRightRight(node);
                }
            }
            
            // Recursively balance the subtrees
            // if they are not balanced
            if (node.LeftChild != null) node.LeftChild = BalanceTree(node.LeftChild);
            if (node.RightChild != null) node.RightChild = BalanceTree(node.RightChild);

            return node;
        }

        // Left Left Case - Rotation
        private Node RotateLeftLeft(Node node)
        {
            if (node.LeftChild == null) throw new InvalidOperationException(Constants.EXC_Rotation_LeftLeft);

            Node pivot = node.LeftChild;
            node.LeftChild = pivot.RightChild;
            pivot.RightChild = node;
            return pivot;
        }
        
        // Left Right Case - Rotation
        private Node RotateLeftRight(Node node)
        {
            if (node.LeftChild == null) throw new InvalidOperationException(Constants.EXC_Rotation_LeftRight);

            Node pivot = node.LeftChild;
            node.LeftChild = RotateRightRight(pivot);
            return RotateLeftLeft(node);
        }

        // Right Right Case - Rotation
        private Node RotateRightRight(Node node)
        {
            if (node.RightChild == null) throw new InvalidOperationException(Constants.EXC_Rotation_RightRight);

            Node pivot = node.RightChild;
            node.RightChild = pivot.LeftChild;
            pivot.LeftChild = node;
            return pivot;
        }

        // Right Left Case - Rotation
        private Node RotateRightLeft(Node node)
        {
            if (node.RightChild == null) throw new InvalidOperationException(Constants.EXC_Rotation_RightLeft);

            Node pivot = node.RightChild;
            node.RightChild = RotateLeftLeft(pivot);
            return RotateRightRight(node);
        }

    }
}