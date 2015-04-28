using System;
using Matrix = MathNet.Numerics.LinearAlgebra.Single.DenseMatrix;
using Vector = MathNet.Numerics.LinearAlgebra.Single.DenseVector;

namespace KaptureLibrary.Processing.DataFiltering
{
    public class KalmanFilter
    {
        #region Generator Functions
        private Func<float, Matrix> F;
        private Func<float, Matrix> B;
        private Func<float, float, Matrix> Q;
        #endregion

        #region Time-invariant Properties
        private Matrix R;
        private Matrix H;
        private float stdDevProcess;
        private float stdDevMeasurement;
        #endregion

        #region Filter State Variables
        private Vector X;
        private Matrix P;
        public Vector StateVector
        {
            get { return X; }
            set { X = value; }
        }
        public Matrix StateCovarienceMatrix
        {
            get { return P; }
            set { P = value; }
        }
        #endregion

        #region Constructors
        public KalmanFilter(KalmanSetup setup)
        {
            this.stdDevProcess = setup.StdDevProcess;
            this.stdDevMeasurement = setup.StdDevMeasurement;
            this.X = setup.X0;
            this.P = setup.P0;
            this.H = setup.H;

            this.F = setup.funcF;
            this.B = setup.funcB;
            this.Q = setup.funcQ;
            this.R = setup.funcR(stdDevMeasurement);
        }
        #endregion

        #region Prediction Equations
        private Vector PredictState(float dt, Vector controlInput)
        {
            return F(dt) * X + B(dt) * controlInput;
        }
        private Matrix PredictCovariance(float dt)
        {
            return F(dt) * P * (Matrix)F(dt).Transpose() + Q(dt, this.stdDevProcess);
        }
        #endregion

        #region Update Equations
        private Vector MeasurementResidual(Vector measurement)
        {
            return measurement - H * X;
        }
        private Matrix CovarianceResidual()
        {
            return H * P * (Matrix)H.Transpose() + R;
        }
        private Matrix KalmanGain(Matrix covarianceResidual)
        {
            return P * (Matrix)(H.Transpose() * covarianceResidual.Inverse());
        }
        private Vector StateEstimation(Matrix kalmanGain, Vector measurementResidual)
        {
            return X + kalmanGain * measurementResidual;
        }
        private Matrix CovarianceEstimation(Matrix kalmanGain)
        {
            var KH = (kalmanGain * H);
            var khOrder = KH.ColumnCount;
            return (Matrix.Identity(khOrder) - KH) * P;
        }
        #endregion

        #region Publicly Exposed Filtering Updates
        public Vector Update(float dt, Vector measurement, Vector controlInput)
        {
            // Predict
            this.X = PredictState(dt, controlInput);
            this.P = PredictCovariance(dt);

            // Update
            var y = MeasurementResidual(measurement);
            var S = CovarianceResidual();
            var K = KalmanGain(S);
            this.X = StateEstimation(K, y);
            this.P = CovarianceEstimation(K);

            return this.X;
        }
        #endregion

    }
}
