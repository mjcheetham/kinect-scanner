using KaptureLibrary.ShapeAndMeasure;
using MathNet.Numerics.LinearAlgebra.Single;

namespace KaptureLibrary.Points
{
    public interface ITransformable { }

    public interface ITransformable2D : ITransformable
    {
        void ApplyAffineTransformation(Matrix T);
        void Rotate(double angle);
        void Translate(int x, int y);
        void Scale(int x, int y);
    }

    public interface ITransformable3D : ITransformable
    {
        void ApplyAffineTransformation(Matrix T);
        void RotateAboutAxis(Axis axis, double angle);
        void Translate(float x, float y, float z);
        void Scale(float x, float y, float z);
    }
}
