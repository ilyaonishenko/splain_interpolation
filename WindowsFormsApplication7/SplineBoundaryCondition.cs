using System;

namespace WindowsFormsApplication7
{
    /// Left and right boundary conditions.
    public enum SplineBoundaryCondition
    {
        /// Natural Boundary (Zero second derivative).
        Natural = 0,

        /// Parabolically Terminated boundary.
        ParabolicallyTerminated,

        /// Fixed first derivative at the boundary.
        FirstDerivative,

        /// Fixed second derivative at the boundary.
        SecondDerivative
    }
}
