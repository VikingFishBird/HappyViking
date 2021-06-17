using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections;

namespace BinarySearchTree {

    public class BinarySearchTree<K, V> where K : System.IComparable {

        public class Node {
            public V Data;
            public K key;
            public Node Left;
            public Node Right;
            public void DisplayNode() {
                Console.Write(Data + " ");
            }
        }
        public Node root;
        public BinarySearchTree() {
            root = null;
        }
        public void Insert(V item, K key) {
            Node newNode = new Node();
            newNode.Data = item;
            if (root == null)
                root = newNode;
            else {
                Node current = root;
                Node parent;
                while (true) {
                    parent = current;
                    if (key.CompareTo(current.key) < 0) {
                        current = current.Left;
                        if (current == null) {
                            parent.Left = newNode;
                            break;
                        }
                    } else {
                        current = current.Right;
                        if (current == null) {
                            parent.Right = newNode;
                            break;
                        }
                    }
                }
            }
        }

        public V SearchForNearest(K key) {
            if (root == null) {
                return default(V);
            }

            Node current = root;
            Node parent;
            while (true) {
                parent = current;
                if (key.CompareTo(current.key) < 0) {
                    current = current.Left;
                    if (current == null) {
                        return parent.Data;
                    }
                } else {
                    current = current.Right;
                    if (current == null) {
                        return parent.Data;
                    }
                }
            }
        } 
    }

}
