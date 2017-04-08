using System;

namespace WindowsFormsApplication7
{
    /// <summary>
    /// Interpolation algorithm
    /// </summary>
    [Obsolete("Please use IInterpolationMethod instead. This interface is obsolete and will be removed in future versions.")]
    public interface IInterpolationAlgorithm
    {
        /// <summary>
        /// Maximum interpolation order.
        /// </summary>
        /// <seealso cref="EffectiveOrder"/>
        int MaximumOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Effective interpolation order.
        /// </summary>
        /// <seealso cref="MaximumOrder"/>
        int EffectiveOrder
        {
            get;
        }

        /// <summary>
        /// Precompute/optimize the algoritm for the given sample set.
        /// </summary>
        void Prepare(SampleList samples);

        /// <summary>
        /// Interpolate at point t.
        /// </summary>
        double Interpolate(double t);

        /// <summary>
        /// Extrapolate at point t.
        /// </summary>
        double Extrapolate(double t);

        /// <summary>
        /// True if the alorithm supports error estimation.
        /// </summary>
        bool SupportErrorEstimation
        {
            get;
        }

        /// <summary>
        /// Interpolate at point t and return the estimated error as error-parameter.
        /// </summary>
        double Interpolate(double t, out double error);
    }
}
