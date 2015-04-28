using System;
using Matrix = MathNet.Numerics.LinearAlgebra.Single.DenseMatrix;
using Vector = MathNet.Numerics.LinearAlgebra.Single.DenseVector;

namespace KaptureLibrary.Processing.DataFiltering
{
    public struct KalmanSetup
    {
        /// <summary>
        /// State prediction matrix generator function (of dt).
        /// </summary>
        public Func<float, Matrix> funcF;
        /// <summary>
        /// State control matrix generator function (of dt).
        /// </summary>
        public Func<float, Matrix> funcB;
        /// <summary>
        /// State-measurement projection matrix.
        /// </summary>
        public Matrix H;
        /// <summary>
        /// Initial state vector.
        /// </summary>
        public Vector X0;
        /// <summary>
        /// Initial state co-varience vector.
        /// </summary>
        public Matrix P0;
        /// <summary>
        /// Prediction co-variance generator function (of dt and stdDevProcess).
        /// </summary>
        public Func<float, float, Matrix> funcQ;
        /// <summary>
        /// Measurement co-variance generator function (of stdDevMeasurement).
        /// </summary>
        public Func<float, Matrix> funcR;
        /// <summary>
        /// Standard deviation of the process noise distribution w.
        /// </summary>
        public float StdDevProcess;
        /// <summary>
        /// Standard deviation of the measurement noise distribution v.
        /// </summary>
        public float StdDevMeasurement;
    }
}
