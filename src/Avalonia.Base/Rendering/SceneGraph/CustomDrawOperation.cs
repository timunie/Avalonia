using System;
using Avalonia.Logging;
using Avalonia.Media;
using Avalonia.Platform;

namespace Avalonia.Rendering.SceneGraph
{
    internal sealed class CustomDrawOperation : DrawOperationWithTransform
    {
        public ICustomDrawOperation Custom { get; }
        public CustomDrawOperation(ICustomDrawOperation custom, Matrix transform) 
            : base(custom.Bounds, transform)
        {
            Custom = custom;
        }

        public override bool HitTestTransformed(Point p) => Custom.HitTest(p);

        public override void Render(IDrawingContextImpl context)
        {
            using var immediateDrawingContext = new ImmediateDrawingContext(context, false);
            try
            {
                Custom.Render(immediateDrawingContext);
            }
            catch (Exception e)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea.Visual)
                    ?.Log(Custom, $"Exception in {Custom.GetType().Name}.{nameof(ICustomDrawOperation.Render)} {{0}}", e);
            }
        }

        public override void Dispose() => Custom.Dispose();

        public bool Equals(Matrix transform, ICustomDrawOperation custom) =>
            Transform == transform && Custom?.Equals(custom) == true;
    }

    public interface ICustomDrawOperation : IEquatable<ICustomDrawOperation>, IDisposable
    {
        /// <summary>
        /// Gets the bounds of the visible content in the node in global coordinates.
        /// </summary>
        Rect Bounds { get; }

        /// <summary>
        /// Hit test the geometry in this node.
        /// </summary>
        /// <param name="p">The point in global coordinates.</param>
        /// <returns>True if the point hits the node's geometry; otherwise false.</returns>
        /// <remarks>
        /// This method does not recurse to childs, if you want
        /// to hit test children they must be hit tested manually.
        /// </remarks>
        bool HitTest(Point p);

        /// <summary>
        /// Renders the node to a drawing context.
        /// </summary>
        /// <param name="context">The drawing context.</param>
        void Render(ImmediateDrawingContext context);
    }
}
