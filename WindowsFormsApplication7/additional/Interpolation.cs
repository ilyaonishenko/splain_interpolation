using System;
using System.Collections.Generic;

namespace WindowsFormsApplication7
{
    /// <summary>
    /// Interpolation Facade.
    /// </summary>
    /// <remarks>
    /// For most cases it is recommended to use the default scheme, see <see cref="Create"/>.
    /// </remarks>
    public static class Interpolation
    {
       

        /// <summary>
        /// Create a linear spline interpolation based on arbitrary points.
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static
        IInterpolationMethod
        CreateLinearSpline(
            IList<double> points,
            IList<double> values
            )
        {
            LinearSplineInterpolation method = new LinearSplineInterpolation();
            method.Init(points, values);
            return method;
        }

        /// <summary>
        /// Create a cubic spline interpolation based on arbitrary points, with specified boundary conditions.
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <param name="leftBoundaryCondition">Condition of the left boundary.</param>
        /// <param name="leftBoundary">Left boundary value. Ignored in the parabolic case.</param>
        /// <param name="rightBoundaryCondition">Condition of the right boundary.</param>
        /// <param name="rightBoundary">Right boundary value. Ignored in the parabolic case.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static
        IInterpolationMethod
        CreateCubicSpline(
            IList<double> points,
            IList<double> values,
            SplineBoundaryCondition leftBoundaryCondition,
            double leftBoundary,
            SplineBoundaryCondition rightBoundaryCondition,
            double rightBoundary
            )
        {
            CubicSplineInterpolation method = new CubicSplineInterpolation();
            method.Init(
                points,
                values,
                leftBoundaryCondition,
                leftBoundary,
                rightBoundaryCondition,
                rightBoundary
                );
            return method;
        }

        /// <summary>
        /// Create a natural cubic spline interpolation based on arbitrary points.
        /// Natural splines are cubic splines with zero second derivative at the boundaries (i.e. straigth lines).
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static
        IInterpolationMethod
        CreateNaturalCubicSpline(
            IList<double> points,
            IList<double> values
            )
        {
            CubicSplineInterpolation method = new CubicSplineInterpolation();
            method.Init(
                points,
                values
                );
            return method;
        }

        /// <summary>
        /// Create an akima cubic spline interpolation based on arbitrary points.
        /// Akima splines are cubic splines which are stable to outliers.
        /// </summary>
        /// <param name="points">The sample points t. Supports both lists and arrays.</param>
        /// <param name="values">The sample point values x(t). Supports both lists and arrays.</param>
        /// <returns>
        /// An interpolation scheme optimized for the given sample points and values,
        /// which can then be used to compute interpolations and extrapolations
        /// on arbitrary points.
        /// </returns>
        public static
        IInterpolationMethod
        CreateAkimaCubicSpline(
            IList<double> points,
            IList<double> values
            )
        {
            AkimaSplineInterpolation method = new AkimaSplineInterpolation();
            method.Init(points, values);
            return method;
        }
    }
}
