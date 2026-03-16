/*
 * Name: Node.cs
 * Author: Rico Rothe
 * Created: 19. January 2023
 * Last Modified: 16. March 2023
 * License: None
 * Description: Class representing a node in the AVL Tree, with properties for value, left child, and right child.
*/

namespace AVL_Tree
{
    public class Node
    {
        public int Value {get; set; } = 0;
        public Node? LeftChild { get; set; } = null;
        public Node? RightChild { get; set; } = null;

        public Node(int value, Func<int, int, CompareResult> compDel)
        {
            Value = value;
            this.compDel = compDel;
        }

        private readonly Func<int, int, CompareResult> compDel;


        /// <summary>
        /// Add a new value to the tree using the specified comparison delegate
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            // Compare the new value to the current node's value
            if(compDel(value, Value) == CompareResult.Bigger ||
               compDel(value, Value) == CompareResult.Equal)
            {
                // If the new value is bigger or equal, add it to the right child
                if(RightChild == null) RightChild = new Node(value, compDel);
                else RightChild.Add(value);
            }
            else
            {
                // If the new value is smaller, add it to the left child
                if(LeftChild == null) LeftChild = new Node(value, compDel);
                else LeftChild.Add(value);
            }
        }
    }
}