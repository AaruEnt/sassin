using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace KWS
{
    public class MeshQuadTree
    {
        public QuadTreeChunksData QuadTreeData = new QuadTreeChunksData();

        public class QuadTreeChunksData
        {
            public bool CanRender => InstancesArgs.Count > 0 && InstancesArgs[0] != null;
            public int ActiveInstanceIndex = 0;

            public List<Mesh> Instances = new List<Mesh>();
            public List<ComputeBuffer> InstancesArgs = new List<ComputeBuffer>();
            public Mesh BottomUnderwaterSkirt;
            public List<ChunkInstance> VisibleChunks = new List<ChunkInstance>();
            public ComputeBuffer VisibleChunksComputeBuffer;

            public void Release()
            {
                foreach (var instance in Instances) KW_Extensions.SafeDestroy(instance);
                foreach (var args in InstancesArgs) if (args != null) args.Release();
                KW_Extensions.SafeDestroy(BottomUnderwaterSkirt);
                if (VisibleChunksComputeBuffer != null) VisibleChunksComputeBuffer.Release();
                VisibleChunksComputeBuffer = null;

                Instances.Clear();
                InstancesArgs.Clear();
                
                ActiveInstanceIndex = 0;
            }
        }


        public struct ChunkInstance
        {
            public Vector4 Position;
            public Vector4 Size;

            public uint DownSeam;
            public uint LeftSeam;
            public uint TopSeam;
            public uint RightSeam;

            public uint DownInf;
            public uint LeftInf;
            public uint TopInf;
            public uint RightInf;
        }


        public enum QuadTreeTypeEnum
        {
            Finite,
            Infinite
        }

        internal List<Node> VisibleNodes = new List<Node>();

        private QuadTreeTypeEnum _quadTreeType;
        List<float> _lodDistances;
        private Vector3 _startWaterPos;
        private Vector3 _startCenter;
        private Vector3 _startSize;
        private Transform _waterTransform;
        private Vector3 _camPos;
        private Vector3 _currentWaterPos;
        private Plane[] _currentCameraFrustumPlanes;
        private Vector3[] _currentCameraFrustumCorners;
        private int[] _lastQuadTreeChunkSizesRelativeToWind;
        private float _maxWaveHeight;
        private Node _root;
        private bool _isCameraUnderwater;

        List<Node> _centerNodes = new List<Node>();
        List<Node> _offsetCenterNodes = new List<Node>();
        private bool _isAnyCenterNodeVisible;

        static int _maxFiniteLevels = 8;

        Vector3 _lastCameraPosition;
        Vector3 _lastCameraRotationEuler;
        Vector3 _lastWaterPos;
        private Vector3 _lastWaterRotation;

        public bool IsRequireReinitialize(Vector3 waterPos, Vector3 rotation, Camera currentCamera, WaterSystemScriptableData settings)
        {
            var requireUpdate = false;
            if (_quadTreeType == QuadTreeTypeEnum.Infinite)
            {
                var minFarDistance = (int)Mathf.Min(settings.OceanDetailingFarDistance, currentCamera.farClipPlane);
                if (Math.Abs(_startSize.x - minFarDistance) > 1) requireUpdate = true;
            }
            if ((_startWaterPos - waterPos).sqrMagnitude > 0.00001f) requireUpdate = true;
            if (Vector3.SqrMagnitude(_lastWaterRotation - rotation) > 0.01f) requireUpdate = true;

            return requireUpdate;
        }

        public void Initialize(QuadTreeTypeEnum quadTreeType, Bounds bounds, Vector3 waterPos, Vector3 waterRotation, WaterSystem.WaterMeshQualityEnum meshQuality, bool useTesselation)
        {
            _quadTreeType = quadTreeType;

            _lodDistances = _quadTreeType == QuadTreeTypeEnum.Finite
                ? InitialiseFiniteLodDistances(bounds.size)
                : InitialiseInfiniteLodDistances(bounds.size, KWS_Settings.Water.QuadtreeInfiniteOceanMinDistance);

            var leveledNodes = new List<LeveledNode>();
            for (int i = 0; i < _lodDistances.Count; i++) leveledNodes.Add(new LeveledNode());

            _centerNodes.Clear();
            _offsetCenterNodes.Clear();
            _startCenter = bounds.center;
            _startSize = bounds.size;
            _startWaterPos = waterPos;
            _lastWaterRotation = waterRotation;

            var nodePosition = _quadTreeType == QuadTreeTypeEnum.Finite ? Vector3.zero : _startCenter;
            _root = new Node(this, leveledNodes, 0, nodePosition, bounds.size);

            InitializeNeighbors(leveledNodes);

            _lastQuadTreeChunkSizesRelativeToWind = quadTreeType == QuadTreeTypeEnum.Infinite
                ? KWS_Settings.Water.QuadTreeChunkQuailityLevelsInfinite[meshQuality]
                : KWS_Settings.Water.QuadTreeChunkQuailityLevelsFinite[meshQuality];



            QuadTreeData.Release();


            for (int lodIndex = 0; lodIndex < _lastQuadTreeChunkSizesRelativeToWind.Length; lodIndex++)
            {
                var currentResolution = GetChunkLodResolution(quadTreeType, bounds, lodIndex, useTesselation);
                var instanceMesh = MeshUtils.GenerateInstanceMesh(currentResolution, _quadTreeType);

                QuadTreeData.Instances.Add(instanceMesh);
                QuadTreeData.InstancesArgs.Add(null);
            }
            QuadTreeData.BottomUnderwaterSkirt = MeshUtils.GenerateUnderwaterBottomSkirt(Vector2Int.one);
            QuadTreeData.ActiveInstanceIndex = QuadTreeData.Instances.Count - 1;
        }

        //public void UpdateQuadTree(Vector3 cameraPos, Vector3 cameraRotation, Vector3 cameraForward, ref Plane[] cameraFrustum, ref Vector3[] cameraCorners, Vector3 waterPos, float maxWaveHeight, Transform waterTransform, bool forceUpdate = false)
        //                                                                                       ref CurrentCameraFrustumPlanes, ref CurrentCameraFrustumCorners, WaterPivotWorldPosition, CurrentMaxWaveHeight, WaterRootTransform, forceUpdate
        public void UpdateQuadTree(Vector3 cameraPos, Vector3 cameraRotation, Vector3 cameraForward, WaterSystem waterInstance, bool forceUpdate = false)
        {
            var distanceToCamera = Vector3.Distance(cameraPos, _lastCameraPosition);
            //forceUpdate = true;
            if (!forceUpdate && !((cameraRotation - _lastCameraRotationEuler).magnitude > KWS_Settings.Water.UpdateQuadtreeEveryDegrees) &&
                !(distanceToCamera >= KWS_Settings.Water.UpdateQuadtreeEveryMetersForward) &&
                (!IsCameraMoveBackwards(cameraPos, cameraForward) || !(distanceToCamera >= KWS_Settings.Water.UpdateQuadtreeEveryMetersBackward)) &&
                !(Mathf.Abs(_lastWaterPos.y - waterInstance.WaterPivotWorldPosition.y) > 0.001f) &&
                QuadTreeData.CanRender) return;



            VisibleNodes.Clear();
            QuadTreeData.VisibleChunks.Clear();

            _camPos = cameraPos;
            _waterTransform = waterInstance.WaterRootTransform;
            _currentCameraFrustumPlanes = WaterSystem.CurrentCameraFrustumPlanes;
            _currentCameraFrustumCorners = WaterSystem.CurrentCameraFrustumCorners;
            _currentWaterPos = waterInstance.WaterPivotWorldPosition;
            _maxWaveHeight = waterInstance.CurrentMaxWaveHeight;
            _isCameraUnderwater = waterInstance.IsCameraUnderwater;
            _isAnyCenterNodeVisible = false;

            _root.UpdateVisibleNodes(this);

            if (_quadTreeType == QuadTreeTypeEnum.Infinite)
            {
                if (_isAnyCenterNodeVisible)
                {
                    var targetNodes = _maxWaveHeight * 2 > KWS_Settings.Water.UpdateQuadtreeEveryMetersForward ? _offsetCenterNodes : _centerNodes;

                    foreach (var centerNode in targetNodes)
                    {
                        if (!VisibleNodes.Contains(centerNode))
                        {
                            centerNode.UpdateCurrentBounds(this);
                            VisibleNodes.Add(centerNode);
                        }
                    }
                }
            }

            var rotation = waterInstance.WaterPivotWorldRotation;
            foreach (var visibleNode in VisibleNodes)
            {
                var meshData = new ChunkInstance();
                var center = visibleNode.CurrentCenter;
                meshData.Position = rotation * new Vector3(center.x, _currentWaterPos.y, center.z);
                meshData.Size = visibleNode.CurrentSize;
                meshData.Size.y = waterInstance.Settings.MeshSize.y;
                meshData = InitializeSeamDataRelativeToNeighbors(meshData, visibleNode);

                QuadTreeData.VisibleChunks.Add(meshData);
            }

            _lastCameraPosition = cameraPos;
            _lastCameraRotationEuler = cameraRotation;
            _lastWaterPos = waterInstance.WaterPivotWorldPosition;

            for (int i = 0; i < QuadTreeData.InstancesArgs.Count; i++)
            {
                QuadTreeData.InstancesArgs[i] = MeshUtils.InitializeInstanceArgsBuffer(QuadTreeData.Instances[i], QuadTreeData.VisibleChunks.Count, QuadTreeData.InstancesArgs[i]);
            }

            MeshUtils.InitializePropertiesBuffer(QuadTreeData.VisibleChunks, ref QuadTreeData.VisibleChunksComputeBuffer, WaterSystem.IsSinglePassStereoEnabled);

        }

        public void UpdateQuadTreeDetailingRelativeToWind(float windSpeed, int offset)
        {
            var windScales = KWS_Settings.Water.QuadTreeChunkLodRelativeToWind;
            var maxInstanceIdx = QuadTreeData.Instances.Count - 1;
            for (int i = 0; i < windScales.Length; i++)
            {
                if (windSpeed < windScales[i])
                {
                    QuadTreeData.ActiveInstanceIndex = Mathf.Clamp(offset + i, 0, maxInstanceIdx);
                    return;
                }
            }
            QuadTreeData.ActiveInstanceIndex = QuadTreeData.Instances.Count - 1;
        }

        public void Release()
        {
            QuadTreeData?.Release();


            _lastCameraPosition = Vector3.positiveInfinity;
            _lastCameraRotationEuler = Vector3.zero;
            _lastWaterPos = Vector3.positiveInfinity;

        }

        bool IsCameraMoveBackwards(Vector3 cameraPos, Vector3 forwardVector)
        {
            var direction = (cameraPos - _lastCameraPosition).normalized;
            var angle = Vector3.Dot(direction, forwardVector);
            return angle < -0.1;
        }

        Vector2Int GetChunkLodResolution(QuadTreeTypeEnum quadTreeType, Bounds bounds, int lodIndex, bool useTesselation)
        {
            Vector2Int chunkRes = Vector2Int.one;
            int lodRes;
            if (useTesselation)
            {
                lodRes = quadTreeType == QuadTreeTypeEnum.Infinite
                    ? KWS_Settings.Water.TesselationInfiniteMeshChunksSize
                    : KWS_Settings.Water.TesselationFiniteMeshChunksSize;
            }
            else lodRes = _lastQuadTreeChunkSizesRelativeToWind[lodIndex];

            var quarterRes = lodRes * 4;
            if (quadTreeType == QuadTreeTypeEnum.Infinite)
            {
                chunkRes *= quarterRes;
            }
            else
            {
                if (bounds.size.x < bounds.size.z)
                {
                    var sizeMul = bounds.size.x / bounds.size.z;
                    chunkRes.x = (int)Mathf.Clamp(quarterRes * chunkRes.x * sizeMul, 4, quarterRes);
                    chunkRes.y = Mathf.Clamp(quarterRes * chunkRes.y, 4, quarterRes);
                }
                else
                {
                    var sizeMul = bounds.size.z / bounds.size.x;
                    chunkRes.x = Mathf.Clamp(quarterRes * chunkRes.x, 4, quarterRes);
                    chunkRes.y = (int)Mathf.Clamp(quarterRes * chunkRes.y * sizeMul, 4, quarterRes);
                  
                }
            }

            return chunkRes;
        }

        void InitializeNeighbors(List<LeveledNode> leveledNodes)
        {
            foreach (var leveledNode in leveledNodes)
            {
                foreach (var pair in leveledNode.Chunks)
                {
                    var chunk = pair.Value;
                    chunk.NeighborLeft = leveledNode.GetLeftNeighbor(chunk.UV);
                    chunk.NeighborRight = leveledNode.GetRightNeighbor(chunk.UV);
                    chunk.NeighborTop = leveledNode.GetTopNeighbor(chunk.UV);
                    chunk.NeighborDown = leveledNode.GetDownNeighbor(chunk.UV);
                }
            }
        }

        List<float> InitialiseFiniteLodDistances(Vector3 size)
        {
            var maxSize = Mathf.Max(size.x, size.z);
            var lodDistances = new List<float>();
            var divider = 2f;
            var sizeRatio = size.x < size.z ? size.x / size.z : size.z / size.x;
           

            var maxLevelsRelativeToSize = Mathf.CeilToInt(Mathf.Log(sizeRatio * maxSize / 4f, 2));
            var maxLevels = Mathf.Min(_maxFiniteLevels, maxLevelsRelativeToSize);

            lodDistances.Add(200000);
            var lastDistance = maxSize;
            while (lodDistances.Count <= maxLevels)
            {
                var currentDistance = maxSize / divider;
                lodDistances.Add(Mathf.Lerp(currentDistance, lastDistance, 0.6f));
                lastDistance = currentDistance;
                divider *= 2;

            }

            return lodDistances;
        }

        List<float> InitialiseInfiniteLodDistances(Vector3 size, float minLodDistance)
        {
            var maxSize = Mathf.Max(size.x, size.z);
            var lodDistances = new List<float>();
            var divider = 2f;

            lodDistances.Add(float.MaxValue);
            while (lodDistances[lodDistances.Count - 1] > minLodDistance)
            {
                lodDistances.Add(maxSize / divider);
                divider *= 2;
            }

            return lodDistances;
        }

        internal class LeveledNode
        {
            public Dictionary<uint, Node> Chunks = new Dictionary<uint, Node>();

            public void AddNodeToArray(Vector2Int uv, Node node)
            {
                node.UV = uv;
                //long hashIdx = uv.x + uv.y * MaxLevelsRange;
                var hashIdx = GetHashFromUV(uv);
                if (!Chunks.ContainsKey(hashIdx)) Chunks.Add(hashIdx, node);
            }

            public Node GetLeftNeighbor(Vector2Int uv)
            {
                //long hashIdx = (uv.x - 1) + uv.y * MaxLevelsRange;
                uv.x -= 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            public Node GetRightNeighbor(Vector2Int uv)
            {
                //long hashIdx = (uv.x + 1) + uv.y * MaxLevelsRange;
                uv.x += 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            public Node GetTopNeighbor(Vector2Int uv)
            {
                // long hashIdx = uv.x + (uv.y + 1) * MaxLevelsRange;
                uv.y += 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            public Node GetDownNeighbor(Vector2Int uv)
            {
                //long hashIdx = uv.x + (uv.y - 1) * MaxLevelsRange;
                uv.y -= 1;
                var hashIdx = GetHashFromUV(uv);
                return Chunks.ContainsKey(hashIdx) ? Chunks[hashIdx] : null;
            }

            uint GetHashFromUV(Vector2Int uv)
            {
                return (((uint)uv.x & 0xFFFF) << 16) | ((uint)uv.y & 0xFFFF);
            }

            Vector2Int GetUVFromHash(uint p)
            {
                return new Vector2Int((int)((p >> 16) & 0xFFFF), (int)((p >> 12) & 0xFF));
            }

        }


        ChunkInstance InitializeSeamDataRelativeToNeighbors(ChunkInstance meshData, Node node)
        {
            var topNeighbor = node.NeighborTop;
            if (topNeighbor == null || !VisibleNodes.Contains(topNeighbor) && VisibleNodes.Contains(topNeighbor.Parent))
            {
                meshData.TopSeam = 1;
            }

            var leftNeighbor = node.NeighborLeft;
            if (leftNeighbor == null || !VisibleNodes.Contains(leftNeighbor) && VisibleNodes.Contains(leftNeighbor.Parent))
            {
                meshData.LeftSeam = 1;
            }

            var downNeighbor = node.NeighborDown;
            if (downNeighbor == null || !VisibleNodes.Contains(downNeighbor) && VisibleNodes.Contains(downNeighbor.Parent))
            {
                meshData.DownSeam = 1;
            }

            var rightNeighbor = node.NeighborRight;
            if (rightNeighbor == null || !VisibleNodes.Contains(rightNeighbor) && VisibleNodes.Contains(rightNeighbor.Parent))
            {
                meshData.RightSeam = 1;
            }

            if (node.CurrentLevel <= 2 || _quadTreeType == QuadTreeTypeEnum.Finite)
            {
                if (topNeighbor == null)
                {
                    meshData.TopInf = 1;
                    meshData.TopSeam = 0;
                }

                if (leftNeighbor == null)
                {
                    meshData.LeftInf = 1;
                    meshData.LeftSeam = 0;
                }

                if (downNeighbor == null)
                {
                    meshData.DownInf = 1;
                    meshData.DownSeam = 0;
                }

                if (rightNeighbor == null)
                {
                    meshData.RightInf = 1;
                    meshData.RightSeam = 0;
                }


            }

            return meshData;
        }

        static Vector2Int PositionToUV(Vector3 pos, Vector3 quadSize, int chunksCounts)
        {
            var uv = new Vector2(pos.x / quadSize.x, pos.z / quadSize.z); //range [-1.0 - 1.0]

            var x = (int)((uv.x * 0.5f + 0.5f) * chunksCounts * 0.999);
            var y = (int)((uv.y * 0.5f + 0.5f) * chunksCounts * 0.999);
            x = Mathf.Clamp(x, 0, chunksCounts - 1);
            y = Mathf.Clamp(y, 0, chunksCounts - 1);
            return new Vector2Int(x, y);
        }

        internal class Node
        {
            public int CurrentLevel;
            public Vector3 StartCenter;
            public Vector3 CurrentCenter;
            public Vector3 StartSize;
            public Vector3 CurrentSize;
            public Node Parent;
            public Node[] Children;

            public Node NeighborLeft;
            public Node NeighborRight;
            public Node NeighborTop;
            public Node NeighborDown;

            public Vector2Int UV;
            public bool IsCenterNode;

            internal Node(MeshQuadTree quadTree, List<LeveledNode> leveledNodes, int currentLevel, Vector3 center, Vector3 size, Node parent = null)
            {
                Parent = parent ?? this;
                //StartCenter = RotatePointAroundPivot(center, Vector3.zero, quadTree._startRotation);
                StartCenter = center;
                StartSize = size;
                CurrentLevel = currentLevel;

                var maxDistanceForLevel = quadTree._lodDistances[CurrentLevel];

                if (currentLevel == quadTree._lodDistances.Count - 1) quadTree._offsetCenterNodes.Add(this);

                if (quadTree._quadTreeType == QuadTreeTypeEnum.Infinite && (StartCenter - quadTree._startCenter).magnitude > maxDistanceForLevel) return;

                if (currentLevel == quadTree._lodDistances.Count - 1)
                {
                    IsCenterNode = true;
                    quadTree._centerNodes.Add(this);
                }

                if (currentLevel < quadTree._lodDistances.Count - 1)
                {
                    Subdivide(quadTree, leveledNodes);
                }
            }

            void Subdivide(MeshQuadTree root, List<LeveledNode> leveledNodes)
            {
                var nextLevel = CurrentLevel + 1;
                var quarterSize = StartSize / 4f;

                var halfSize = new Vector3(StartSize.x / 2f, StartSize.y, StartSize.z / 2f);
                var rootHalfSize = new Vector3(root._startSize.x / 2f, root._startSize.y, root._startSize.z / 2f);

                int chunksCounts = (int)Mathf.Pow(2, nextLevel);
                var level = leveledNodes[nextLevel];

                Children = new Node[4];

                var center = new Vector3(StartCenter.x - quarterSize.x, StartCenter.y, StartCenter.z + quarterSize.z);
                Children[0] = new Node(root, leveledNodes, nextLevel, center, halfSize, this);
                level.AddNodeToArray(PositionToUV(center, rootHalfSize, chunksCounts), Children[0]);

                center = new Vector3(StartCenter.x + quarterSize.x, StartCenter.y, StartCenter.z + quarterSize.z);
                Children[1] = new Node(root, leveledNodes, nextLevel, center, halfSize, this);
                level.AddNodeToArray(PositionToUV(center, rootHalfSize, chunksCounts), Children[1]);

                center = new Vector3(StartCenter.x - quarterSize.x, StartCenter.y, StartCenter.z - quarterSize.z);
                Children[2] = new Node(root, leveledNodes, nextLevel, center, halfSize, this);
                level.AddNodeToArray(PositionToUV(center, rootHalfSize, chunksCounts), Children[2]);

                center = new Vector3(StartCenter.x + quarterSize.x, StartCenter.y, StartCenter.z - quarterSize.z);
                Children[3] = new Node(root, leveledNodes, nextLevel, center, halfSize, this);
                level.AddNodeToArray(PositionToUV(center, rootHalfSize, chunksCounts), Children[3]);

            }

            internal enum ChunkVisibilityEnum
            {
                Visible,
                NotVisibile,
                NotVisibleLod,
                PartialVisible
            }

            internal Vector3 UpdateCurrentBounds(MeshQuadTree root)
            {
                Vector3 offset;
                if (root._quadTreeType == QuadTreeTypeEnum.Infinite)
                {
                    offset = root._camPos;
                    offset.y = 0;
                }
                else offset = root._waterTransform.InverseTransformVector(root._currentWaterPos);

                CurrentCenter = StartCenter + offset;
                CurrentSize = new Vector3(StartSize.x, StartSize.y + root._maxWaveHeight * 2, StartSize.z);
                return offset;
            }


            internal ChunkVisibilityEnum UpdateVisibleNodes(MeshQuadTree root)
            {
                var offset = UpdateCurrentBounds(root);


                var halfSize = CurrentSize / 2f;
                var camPos = root._camPos;
                if (root._quadTreeType == QuadTreeTypeEnum.Infinite)
                {
                    var cubeSize = StartCenter + offset;
                    if (CurrentLevel <= 2) halfSize += new Vector3(KWS_Settings.Water.QuadTreeMaxOceanFarDistance, 0, KWS_Settings.Water.QuadTreeMaxOceanFarDistance);
                    var min = cubeSize - halfSize;
                    var max = cubeSize + halfSize;
                    if (!root._isCameraUnderwater && !KW_Extensions.IsBoxVisibleAccurate(ref root._currentCameraFrustumPlanes, ref root._currentCameraFrustumCorners, min, max))
                        return ChunkVisibilityEnum.NotVisibile;
                }
                else
                {
                    halfSize *= 1.1f;
                    var cubeSize = root._waterTransform.TransformPoint(StartCenter);
                    var min = cubeSize - halfSize;
                    var max = cubeSize + halfSize;
                    if (!root._isCameraUnderwater && !KW_Extensions.IsBoxVisibleApproximated(ref root._currentCameraFrustumPlanes, min, max)) return ChunkVisibilityEnum.NotVisibile;
                    offset.y += CurrentSize.y * 0.5f;
                    camPos = root._waterTransform.InverseTransformPoint(root._camPos) + offset;
                }


                var surfacePos = CurrentCenter;
                surfacePos.y += halfSize.y;

                if ((surfacePos - camPos).magnitude > root._lodDistances[CurrentLevel]) return ChunkVisibilityEnum.NotVisibleLod;

                if (IsCenterNode) root._isAnyCenterNodeVisible = true;

                if (Children == null)
                {
                    root.VisibleNodes.Add(this);
                    return ChunkVisibilityEnum.Visible;
                }

                foreach (var child in Children)
                {
                    if (child.UpdateVisibleNodes(root) == ChunkVisibilityEnum.NotVisibleLod)
                    {
                        root.VisibleNodes.Add(child);
                    }
                }


                return ChunkVisibilityEnum.PartialVisible;
            }

        }

    }
}