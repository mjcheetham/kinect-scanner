using KaptureLibrary.Points;
using KaptureLibrary.Tracking;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KaptureLibrary.Kinect
{
    public class Mapper
    {
        /// <summary>
        /// Internal CoordinateMapper object.
        /// </summary>
        private CoordinateMapper mapper;

        #region Constructors
        public Mapper(ICollection<byte> mappingParams)
        {
            this.mapper = new CoordinateMapper(mappingParams);
        }
        #endregion

        #region Public Mapping Methods
        public ICollection<Point3D> GeneratePointCloud(DepthFormat dFormat, ICollection<short> depthShorts,
            ColourFormat cFormat, ICollection<byte> colourPixels)
        {
            var kdFormat = FormatConvertor.ConvertToKinect(dFormat);
            var kcFormat = FormatConvertor.ConvertToKinect(cFormat);

            var dWidth = FormatConvertor.PixelWidth(kdFormat);
            var cWidth = FormatConvertor.PixelWidth(kcFormat);

            var points = new List<Point3D>();
            var d = depthShorts.ToArray();
            var c = colourPixels.ToArray();

            for (int i = 0; i < d.Length; i++)
            {
                var depth = (short)(d[i] >> 3);
                if (depth < 0) continue;
                var dip = new DepthImagePoint()
                    {
                        Depth = depth,
                        X = i % dWidth,
                        Y = i / dWidth
                    };
                var skel = this.mapper.MapDepthPointToSkeletonPoint(kdFormat, dip);

                var cip = this.mapper.MapDepthPointToColorPoint(kdFormat, dip, kcFormat);
                var cIndex = 4 * (cip.X + cip.Y * cWidth);
                if (cIndex > c.Length || cIndex < 0) continue;
                var r = c[cIndex + 2];
                var g = c[cIndex + 1];
                var b = c[cIndex];
                points.Add(new Point3D(skel.X, skel.Y, skel.Z, r, g, b));
            }

            return points;
        }

        public IDictionary<TrackingMarker,Point3D> Convert2DTrackingPointsTo3DTrackingPoints(
            IDictionary<TrackingMarker, Point2D> trackingPoints2D,
            DepthFormat dFormat, ICollection<short> depthShorts, ColourFormat cFormat)
        {
            var kdFormat = FormatConvertor.ConvertToKinect(dFormat);
            var kcFormat = FormatConvertor.ConvertToKinect(cFormat);

            var dShorts = depthShorts.ToArray();

            // HACK!!!!!!!!!!!!!!!!
            return _ConvertColourPointsToWorldPoints(trackingPoints2D, kdFormat, dShorts, kcFormat);
        }
        #endregion

        #region Internal HACKY Methods
        [Obsolete("HACK: This method is very badly implemented!")]
        private IDictionary<TrackingMarker, Point3D> _ConvertColourPointsToWorldPoints(
            IDictionary<TrackingMarker, Point2D> trackingPoints2D,
            DepthImageFormat kdFormat, short[] dShorts, ColorImageFormat kcFormat)
        {
            var dWidth = FormatConvertor.PixelWidth(kdFormat);
            var dSize = FormatConvertor.PixelDataLength(kdFormat);

            var dips = new DepthImagePoint[dSize];
            var cips = new ColorImagePoint[dSize];
            var trackingPoints3D = new Dictionary<TrackingMarker, Point3D>();

            for (int i = 0; i < dSize; i++)
            {
                var depth = (short)(dShorts[i] >> 3);
                if (depth < 0) continue;
                dips[i] = (new DepthImagePoint()
                {
                    Depth = depth,
                    X = i % dWidth,
                    Y = i / dWidth
                });
                cips[i] = (this.mapper.MapDepthPointToColorPoint(kdFormat, dips[i], kcFormat));
            }
            int dIndex = 0;
            for (; dIndex < cips.Length; dIndex++)
            {
                foreach (var qPair in trackingPoints2D)
                {
                    var q = qPair.Value;
                    if (cips[dIndex].X == q.X && cips[dIndex].Y == q.Y)
                    {
                        var sk = this.mapper.MapDepthPointToSkeletonPoint(kdFormat, dips[dIndex]);
                        var p = new Point3D(sk.X, sk.Y, sk.Z);
                        trackingPoints3D.Add(qPair.Key, p);
                    }
                }
                if (trackingPoints3D.Count == trackingPoints2D.Count) break;
            }
            return trackingPoints3D;
        }
        #endregion

        #region Static Methods
        [Obsolete("Building DepthImagePixels like this shouldn't be used.", true)]
        private static ICollection<DepthImagePixel> UnpackDepthShorts(ICollection<short> depthShorts)
        {
            var dShortsArray = depthShorts.ToArray();
            var dPixels = new DepthImagePixel[depthShorts.Count];
            //Parallel.For(0, depthShorts.Count, (i) =>
            for (int i = 0; i < depthShorts.Count; i++)
            {
                short depth = (short)(dShortsArray[i] >> 3);
                short player = (short)(dShortsArray[i] & 0x7);
                if (depth >= 0)
                {
                    dPixels[i] = new DepthImagePixel()
                    {
                        PlayerIndex = player,
                        Depth = depth
                    };
                }
            }
            return dPixels;
        }

        private static ICollection<Point3D> ConvertSkeletonPoints(ICollection<SkeletonPoint> skeletonPoints)
        {
            var skelPointsArray = skeletonPoints.ToArray();
            var points = new Point3D[skeletonPoints.Count];
            Parallel.For(0, skeletonPoints.Count, (i) =>
            {
                points[i] = new Point3D(skelPointsArray[i].X,
                                              skelPointsArray[i].Y,
                                              skelPointsArray[i].Z);
            });
            return points;
        }
        #endregion

    }
}
