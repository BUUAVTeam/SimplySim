using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimplySim.Xna.Engine;

using register = SimplySim.Xna.Engine.Register;
using render = SimplySim.Xna.Engine.Render;

namespace CheckPointController
{
    public class EllipsoidPrimitive : Entity
    {
        private const float Diameter = 1f;
        private const int Tessellation = 12;

        private static short[] _indicesWF;

        private static short[] _indices;

        private static VertexPositionColor[] _vertices;

        private static IndexBuffer _indexBuffer;

        private static VertexBuffer _vertexBuffer;

        private static IndexBuffer _indexWFBuffer;

        private BoundingSphere _boundingSphere;

        private Matrix _lastWorld;

        static EllipsoidPrimitive()
        {
            List<short> indices = new List<short>();
            List<short> indicesWF = new List<short>();
            List<VertexPositionColor> vertices = new List<VertexPositionColor>();

            int verticalSegments = Tessellation;
            int horizontalSegments = Tessellation * 2;
            float radius = Diameter / 2;
            
            //Vertex
            vertices.Add(new VertexPositionColor(Vector3.Down * radius, Color.White));
            
            for (int i = 0; i < verticalSegments - 1; i++)
            {
                float latitude = ((i + 1) * MathHelper.Pi / verticalSegments) - MathHelper.PiOver2;
                float dy = (float)System.Math.Sin(latitude);
                float dxz = (float)System.Math.Cos(latitude);

                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;
                    float dx = (float)System.Math.Cos(longitude) * dxz;
                    float dz = (float)System.Math.Sin(longitude) * dxz;
                    Vector3 normal = new Vector3(dx, dy, dz);
                    vertices.Add(new VertexPositionColor(normal * radius, Color.White));
                }
            }
            vertices.Add(new VertexPositionColor(Vector3.Up * radius, Color.White));

            //Indices
            for (int i = 0; i < horizontalSegments; i++)
            {
                indices.Add(0);
                indices.Add((short)(1 + (i + 1) % horizontalSegments));
                indices.Add((short)(1 + i));

                indicesWF.Add(0);
                indicesWF.Add((short)(1 + (i + 1) % horizontalSegments));
                indicesWF.Add((short)(1 + (i + 1) % horizontalSegments));
                indicesWF.Add((short)(1 + i));
                indicesWF.Add((short)(1 + i));
                indicesWF.Add(0);
            }
            for (int i = 0; i < verticalSegments - 2; i++)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;
                    indices.Add((short)(1 + i * horizontalSegments + j));
                    indices.Add((short)(1 + i * horizontalSegments + nextJ));
                    indices.Add((short)(1 + nextI * horizontalSegments + j));
                    indices.Add((short)(1 + i * horizontalSegments + nextJ));
                    indices.Add((short)(1 + nextI * horizontalSegments + nextJ));
                    indices.Add((short)(1 + nextI * horizontalSegments + j));

                    indicesWF.Add((short)(1 + i * horizontalSegments + j));
                    indicesWF.Add((short)(1 + i * horizontalSegments + nextJ));
                    indicesWF.Add((short)(1 + i * horizontalSegments + nextJ));
                    indicesWF.Add((short)(1 + nextI * horizontalSegments + j));
                    indicesWF.Add((short)(1 + nextI * horizontalSegments + j));
                    indicesWF.Add((short)(1 + i * horizontalSegments + nextJ));
                    indicesWF.Add((short)(1 + i * horizontalSegments + nextJ));
                    indicesWF.Add((short)(1 + nextI * horizontalSegments + nextJ));
                    indicesWF.Add((short)(1 + nextI * horizontalSegments + nextJ));
                    indicesWF.Add((short)(1 + nextI * horizontalSegments + j));
                    indicesWF.Add((short)(1 + nextI * horizontalSegments + j));
                    indicesWF.Add((short)(1 + i * horizontalSegments + j));
                }
            }
            for (int i = 0; i < horizontalSegments; i++)
            {
                indices.Add((short)(vertices.Count - 1));
                indices.Add((short)(vertices.Count - 2 - (i + 1) % horizontalSegments));
                indices.Add((short)(vertices.Count - 2 - i));

                indicesWF.Add((short)(vertices.Count - 1));
                indicesWF.Add((short)(vertices.Count - 2 - (i + 1) % horizontalSegments));
                indicesWF.Add((short)(vertices.Count - 2 - (i + 1) % horizontalSegments));
                indicesWF.Add((short)(vertices.Count - 2 - i));
                indicesWF.Add((short)(vertices.Count - 2 - i));
                indicesWF.Add((short)(vertices.Count - 1));
            }

            _indices = indices.ToArray();
            _indicesWF = indicesWF.ToArray();
            _vertices = vertices.ToArray();
        }


        public EllipsoidPrimitive(Color color)
            : base(Guid.NewGuid().ToString(), "Ellipsoid Primitive")
        {
            Color = color;
            _boundingSphere = ComputeBounding();
        }

        public bool IsOpaque { get; set; }

        public Color Color { get; set; }
               
        public override void LoadContent(ContentProvider provider)
        {
            base.LoadContent(provider);

            if (_vertexBuffer == null)
            {
                _vertexBuffer = new VertexBuffer(provider.Device, typeof(VertexPositionColor), _vertices.Length, BufferUsage.WriteOnly);
                _vertexBuffer.SetData<VertexPositionColor>(_vertices);
            }

            if (_indexBuffer == null)
            {
                _indexBuffer = new IndexBuffer(provider.Device, typeof(short), _indices.Length, BufferUsage.WriteOnly);
                _indexBuffer.SetData<short>(_indices);
            }

            if (_indexWFBuffer == null)
            {
                _indexWFBuffer = new IndexBuffer(provider.Device, typeof(short), _indicesWF.Length, BufferUsage.WriteOnly);
                _indexWFBuffer.SetData<short>(_indicesWF);
            }
        }

        public override void Update(SceneNode node, EngineTime time)
        {
            base.Update(node, time);
            _lastWorld = node.WorldPose;
        }

        public override void Register(List<register.IRegisterable> registerables, register.ReflectSet reflects, render.GpuTaskManager gpuManager, MaterialManager materialManager, PoolManager poolManager)
        {
            base.Register(registerables, reflects, gpuManager, materialManager, poolManager);

            BoundingSphere currentBounding;
            Helpers.TransformBoundingSphere(ref _boundingSphere, ref _lastWorld, out currentBounding);

            registerables.Add(EllipsoidPrimitiveRegisterable.CreateInstance(
                poolManager.GetPool<EllipsoidPrimitiveRegisterable>(), _vertices.Length, _indicesWF.Length, _indices.Length,
                   _vertexBuffer, _indexWFBuffer, _indexBuffer, _lastWorld, Color, IsOpaque, currentBounding));
        }

        protected BoundingSphere ComputeBounding()
        {
            Vector3[] points = new Vector3[_vertices.Length];

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = _vertices[i].Position;
            }

            return BoundingSphere.CreateFromPoints(points);
        }
    }

    internal class EllipsoidPrimitiveRegisterable : register.IRegisterable
    {
        private int _verticesCount;

        private int _indicesWFCount;

        private int _indicesCount;

        private VertexBuffer _vertexBuffer;

        private IndexBuffer _indicesWFBuffer;

        private IndexBuffer _indicesBuffer;

        private Matrix _world;

        private Color _color;

        private bool _isOpaque;

        private BoundingSphere _boundingSphere;

        private EllipsoidPrimitiveRegisterable()
        {
        }

        public static EllipsoidPrimitiveRegisterable CreateInstance(Pool<EllipsoidPrimitiveRegisterable> pool,
            int verticesCount, int indicesWFCount, int indicesCount, VertexBuffer vertexBuffer, IndexBuffer indicesWFBuffer,
            IndexBuffer indicesBuffer, Matrix world, Color color, bool isOpaque, BoundingSphere boundingSphere)
        {
            EllipsoidPrimitiveRegisterable result = null;

            if (!pool.TryGetInstance(out result))
            {
                result = new EllipsoidPrimitiveRegisterable();
                pool.Add(result);
            }

            result.Initialize(verticesCount, indicesWFCount, indicesCount, vertexBuffer, indicesWFBuffer, indicesBuffer, ref world, ref color, isOpaque, ref boundingSphere);

            return result;
        }

        private void Initialize(int verticesCount, int indicesWFCount, int indicesCount, VertexBuffer vertexBuffer, IndexBuffer indicesWFBuffer,
            IndexBuffer indicesBuffer, ref Matrix world, ref Color color, bool isOpaque, ref BoundingSphere boundingSphere)
        {
            _verticesCount = verticesCount;
            _indicesWFCount = indicesWFCount;
            _indicesCount = indicesCount;
            _vertexBuffer = vertexBuffer;
            _indicesBuffer = indicesBuffer;
            _indicesWFBuffer = indicesWFBuffer;
            _world = world;
            _color = color;
            _isOpaque = isOpaque;
            _boundingSphere = boundingSphere;
        }

        #region IRegisterable Members

        void register.IRegisterable.RenderRegister(register.Viewpoint viewpoint, ref Viewport viewport, render.RenderQueueGroup queueGroup, PoolManager poolManager)
        {
            ContainmentType containment;
            viewpoint.Frustum.Contains(ref _boundingSphere, out containment);

            if (containment == ContainmentType.Disjoint)
            {
                return;
            }

            queueGroup.Register(
                EllipsoidPrimitiveRenderable.CreateInstance(poolManager.GetPool<EllipsoidPrimitiveRenderable>(),
                _verticesCount, _indicesWFCount, _indicesCount, _vertexBuffer, _indicesWFBuffer, _indicesBuffer, _world, viewpoint.View, viewpoint.Projection, _color, _isOpaque),
                 render.RenderQueueNames.Custom4);

        }

        void register.IRegisterable.SceneRegister(register.SceneInformation sceneInformation, register.LightsAndCameras lightsAndCameras, PoolManager poolManager)
        {
        }

        #endregion
    }


    internal class EllipsoidPrimitiveRenderable : render.IRenderable
    {
        private int _verticesCount;

        private int _indicesWFCount;

        private int _indicesCount;

        private VertexBuffer _vertexBuffer;

        private IndexBuffer _indicesWFBuffer;

        private IndexBuffer _indicesBuffer;

        private Matrix _world;

        private Matrix _view;

        private Matrix _projection;

        private Color _color;

        private bool _isOpaque;

        private EllipsoidPrimitiveRenderable()
        {
        }

        public static EllipsoidPrimitiveRenderable CreateInstance(Pool<EllipsoidPrimitiveRenderable> pool,
            int verticesCount, int indicesWFCount, int indicesCount, VertexBuffer vertexBuffer, IndexBuffer indicesWFBuffer,
            IndexBuffer indicesBuffer, Matrix world, Matrix view, Matrix projection, Color color, bool isOpaque)
        {
            EllipsoidPrimitiveRenderable result = null;

            if (!pool.TryGetInstance(out result))
            {
                result = new EllipsoidPrimitiveRenderable();
                pool.Add(result);
            }

            result.Initialize(verticesCount, indicesWFCount, indicesCount, vertexBuffer, indicesWFBuffer, indicesBuffer, ref world, ref view, ref projection, ref color, isOpaque);

            return result;
        }

        private void Initialize(int verticesCount, int indicesWFCount, int indicesCount, VertexBuffer vertexBuffer, IndexBuffer indicesWFBuffer,
            IndexBuffer indicesBuffer, ref Matrix world, ref Matrix view, ref Matrix projection, ref Color color, bool isOpaque)
        {
            _verticesCount = verticesCount;
            _indicesWFCount = indicesWFCount;
            _indicesCount = indicesCount;
            _vertexBuffer = vertexBuffer;
            _indicesBuffer = indicesBuffer;
            _indicesWFBuffer = indicesWFBuffer;
            _world = world;
            _view = view;
            _projection = projection;
            _color = color;
            _isOpaque = isOpaque;
        }

        #region IRenderable Members

        public virtual void Render(render.GraphicsDeviceManager manager, string technique)
        {
            if (!String.IsNullOrEmpty(technique))
            {
                return;
            }

            manager.BasicEffect.World = _world;
            manager.BasicEffect.View = _view;
            manager.BasicEffect.Projection = _projection;
            manager.BasicEffect.DiffuseColor = _color.ToVector3();
            manager.BasicEffect.VertexColorEnabled = false;

            manager.GraphicsDevice.VertexDeclaration = manager.VertexPositionColor;
            manager.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, manager.VertexPositionColor.GetVertexStrideSize(0));

            if (!_isOpaque)
            {
                manager.BasicEffect.Alpha = 0.1f;
            }

            manager.BasicEffect.Begin();
            foreach (EffectPass effectPass in manager.BasicEffect.CurrentTechnique.Passes)
            {
                effectPass.Begin();
                if (!_isOpaque)
                {
                    manager.GraphicsDevice.Indices = _indicesWFBuffer;
                    manager.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, _verticesCount, 0, _indicesWFCount / 2);
                    bool oldAB = manager.GraphicsDevice.RenderState.AlphaBlendEnable;
                    bool oldDBW = manager.GraphicsDevice.RenderState.DepthBufferWriteEnable;
                    Blend oldSrcBlend = manager.GraphicsDevice.RenderState.SourceBlend;
                    Blend oldDestBlend = manager.GraphicsDevice.RenderState.DestinationBlend;
                    manager.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
                    manager.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                    manager.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
                    manager.GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
                    manager.GraphicsDevice.Indices = _indicesBuffer;
                    manager.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _verticesCount, 0, _indicesCount / 3);
                    manager.GraphicsDevice.RenderState.DepthBufferWriteEnable = oldDBW;
                    manager.GraphicsDevice.RenderState.AlphaBlendEnable = oldAB;
                    manager.GraphicsDevice.RenderState.SourceBlend = oldSrcBlend;
                    manager.GraphicsDevice.RenderState.DestinationBlend = oldDestBlend;

                }
                else
                {
                    manager.GraphicsDevice.Indices = _indicesBuffer;
                    manager.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _verticesCount, 0, _indicesCount / 3);
                }

                effectPass.End();
            }
            manager.BasicEffect.End();
            manager.BasicEffect.Alpha = 1.0f;
        }

        #endregion
    }

}
