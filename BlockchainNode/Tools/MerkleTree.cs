using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MainBlockchain
{
    public class MerkleTree
    {
        private readonly List<byte[]> _leaves;
        private readonly List<List<byte[]>> _levels = new();

        public MerkleTree(IEnumerable<byte[]> leaves)
        {
            if (leaves == null) throw new ArgumentNullException(nameof(leaves));
            _leaves = leaves.Select(Clone).ToList();
            if (_leaves.Count == 0) throw new ArgumentException("Leaf collection cannot be empty", nameof(leaves));
            Build();
        }
        public static MerkleTree FromStrings(IEnumerable<string> strings)
        {
            var leaves = strings.Select(s => HashLeaf(System.Text.Encoding.UTF8.GetBytes(s)));
            return new MerkleTree(leaves);
        }

        public static MerkleTree FromHexLeaves(IEnumerable<string> hexLeaves)
        {
            var leaves = hexLeaves.Select(hex => Convert.FromHexString(hex));
            return new MerkleTree(leaves);
        }


        public byte[] Root => Clone(_levels[^1][0]);
        public string RootHex => ToHex(Root);

        public int IndexOfLeaf(byte[] leafHash)
        {
            for (int i = 0; i < _leaves.Count; i++)
                if (_leaves[i].SequenceEqual(leafHash)) return i;
            return -1;
        }

        public IReadOnlyList<ProofElement> GetProof(int index)
        {
            if (index < 0 || index >= _leaves.Count) throw new ArgumentOutOfRangeException(nameof(index));
            var proof = new List<ProofElement>();
            for (int level = 0; level < _levels.Count - 1; level++)
            {
                var levelNodes = _levels[level];
                var isRight = (index & 1) == 1;
                var siblingIdx = isRight ? index - 1 : index + 1;
                if (siblingIdx >= levelNodes.Count) siblingIdx = index; 
                proof.Add(new ProofElement(levelNodes[siblingIdx], !isRight));
                index >>= 1;
            }
            return proof;
        }

        public List<ProofHex> GetProofHexByLeafHex(string leafHex, int depth = 20)
        {
            var proof = GetProof(IndexOfLeaf(Convert.FromHexString(leafHex)))
                        .Select(p => new ProofHex
                        {
                            SiblingHex = ToHex(p.Hash),
                            Dir = p.IsLeftSibling ? "left" : "right"
                        })
                        .ToList();

            while (proof.Count < depth)
            {
                proof.Add(new ProofHex
                {
                    SiblingHex = leafHex,
                    Dir = "left"
                });
            }
            return proof;
        }


        public static bool VerifyProof(byte[] leafHash, IReadOnlyList<ProofElement> proof, byte[] root)
        {
            var cur = Clone(leafHash);
            foreach (var p in proof)
                cur = p.IsLeftSibling ? HashPair(p.Hash, cur) : HashPair(cur, p.Hash);
            return cur.SequenceEqual(root);
        }

       
        public static byte[] HashLeaf(byte[] data)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(data);
        }

        public static string ToHex(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();

      
        private void Build()
        {
            _levels.Clear();
            _levels.Add(_leaves);
            while (_levels[^1].Count > 1)
            {
                var cur = _levels[^1];
                var next = new List<byte[]>((cur.Count + 1) / 2);
                for (int i = 0; i < cur.Count; i += 2)
                {
                    var left = cur[i];
                    var right = (i + 1 < cur.Count) ? cur[i + 1] : cur[i];
                    next.Add(HashPair(left, right));
                }
                _levels.Add(next);
            }
        }

        private static byte[] HashPair(byte[] left, byte[] right)
        {
            using var sha = SHA256.Create();
            var buf = new byte[left.Length + right.Length];
            Buffer.BlockCopy(left, 0, buf, 0, left.Length);
            Buffer.BlockCopy(right, 0, buf, left.Length, right.Length);
            return sha.ComputeHash(buf);
        }

        private static byte[] Clone(byte[] src)
        {
            var dst = new byte[src.Length];
            Buffer.BlockCopy(src, 0, dst, 0, src.Length);
            return dst;
        }

      
        public readonly struct ProofElement
        {
            public byte[] Hash { get; }
            public bool IsLeftSibling { get; }
            public ProofElement(byte[] hash, bool isLeftSibling)
            {
                Hash = Clone(hash);
                IsLeftSibling = isLeftSibling;
            }
        }

        public readonly struct ProofHex
        {
            public string SiblingHex { get; init; }
            public string Dir { get; init; }  
        }
    }
}
