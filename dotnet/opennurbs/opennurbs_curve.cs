using System;
using Rhino.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;
// don't wrap ON_MeshCurveParameters. It is only needed for the ON_Curve::MeshCurveFunction

namespace Rhino.Geometry
{
  /// <summary>
  /// Lists all possible corner styles for curve offsets.
  /// </summary>
  public enum CurveOffsetCornerStyle : int
  {
    None = 0,
    Sharp = 1,
    Round = 2,
    Smooth = 3,
    Chamfer = 4
  }

  /// <summary>
  /// Lists all possible knot spacing styles for Interpolated curves.
  /// </summary>
  public enum CurveKnotStyle : int
  {
    /// <summary>
    /// Parameter spacing between consecutive knots is 1.0
    /// </summary>
    Uniform = 0,

    /// <summary>
    /// Chord length spacing, requires degree=3 with CV1 and CVn1 specified.
    /// </summary>
    Chord = 1,

    /// <summary>
    /// Square root of chord length, requires degree=3 with CV1 and CVn1 specified.
    /// </summary>
    ChordSquareRoot = 2,

    /// <summary>
    /// Periodic with uniform spacing.
    /// </summary>
    UniformPeriodic = 3,

    /// <summary>
    /// Periodic with chord length spacing.
    /// </summary>
    ChordPeriodic = 4,

    /// <summary>
    /// Periodic with square roor of chord length spacing. 
    /// </summary>
    ChordSquareRootPeriodic = 5
  }

  /// <summary>
  /// Lists all possible closed curve orientations.
  /// </summary>
  public enum CurveOrientation : int
  {
    /// <summary>
    /// Orientation is undefined.
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// The curve's orientation is clockwise in the xy plane.
    /// </summary>
    Clockwise = -1,

    /// <summary>
    /// The curve's orientation is counter clockwise in the xy plane.
    /// </summary>
    CounterClockwise = +1
  }

  /// <summary>
  /// Enumerates all possible closed curve/point spatial relationships.
  /// </summary>
  public enum PointContainment : int
  {
    /// <summary>
    /// Relation is meaningless.
    /// </summary>
    Unset,

    /// <summary>
    /// Point is on the interior of the region implied by the closed curve.
    /// </summary>
    Inside,

    /// <summary>
    /// Point is on the exterior of the region implied by the closed curve.
    /// </summary>
    Outside,

    /// <summary>
    /// Point is coincident with the curve and therefor neither inside not outside.
    /// </summary>
    Coincident
  }

  /// <summary>
  /// Enumerates all possible styles to use during curve Extension.
  /// </summary>
  public enum CurveExtensionStyle : int
  {
    /// <summary>
    /// Curve ends will be propagated linearly according to tangents.
    /// </summary>
    Line = 0,

    /// <summary>
    /// Curve ends will be propagated arc-wise according to curvature.
    /// </summary>
    Arc = 1,

    /// <summary>
    /// Curve ends will be propagated smoothly according to curvature.
    /// </summary>
    Smooth = 2
  }

  /// <summary>
  /// Enumerates the options to use when simplifying a curve.
  /// </summary>
  [FlagsAttribute]
  public enum CurveSimplifyOptions : int
  {
    None = 0,
    /// <summary>
    /// Split NurbsCurves at fully multiple knots. 
    /// Effectively turning single nurbs segments with kinks into multiple segments.
    /// </summary>
    SplitAtFullyMultipleKnots = 1,
    /// <summary>
    /// Replace linear segments with LineCurves.
    /// </summary>
    RebuildLines = 2,
    /// <summary>
    /// Replace partially circualr segments with ArcCurves.
    /// </summary>
    RebuildArcs = 4,
    /// <summary>
    /// Replace rational nurbscurves with constant weights 
    /// with an equivalent non-rational NurbsCurve.
    /// </summary>
    RebuildRationals = 8,
    /// <summary>
    /// Adjust Curves at G1-joins.
    /// </summary>
    AdjustG1 = 16,
    /// <summary>
    /// Merge adjacent co-linear lines or co-circular arcs 
    /// or combine consecutive line segments into a polyline.
    /// </summary>
    Merge = 32,
    /// <summary>
    /// Implies all of the simplification functions will be used.
    /// </summary>
    All = SplitAtFullyMultipleKnots | RebuildLines | RebuildArcs | RebuildRationals | AdjustG1 | Merge
  }

  [FlagsAttribute]
  public enum CurveEnd : int
  {
    None = 0,
    Start = 1,
    End = 2,
    Both = 3
  };
  /// <summary>
  /// Enumerates the possible options for curve evaluation side.
  /// </summary>
  public enum CurveEvaluationSide : int
  {
    Default = 0,
    Below = -1,
    Above = +1
  }

  public class Curve : GeometryBase
  {
    #region statics

    /// <summary>
    /// Interpolates a sequence of points. Used by InterpCurve Command
    /// This routine works best when degree=3.
    /// </summary>
    /// <param name="degree">The degree of the curve >=1.  Degree must be odd.</param>
    /// <param name="points">
    /// Points to interpolate (Count must be >= 2)
    /// </param>
    /// <returns>interpolated curve on success. null on failure</returns>
    public static Curve CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree)
    {
      return CreateInterpolatedCurve(points, degree, CurveKnotStyle.Uniform);
    }
    /// <summary>
    /// Interpolates a sequence of points. Used by InterpCurve Command
    /// This routine works best when degree=3.
    /// </summary>
    /// <param name="degree">The degree of the curve >=1.  Degree must be odd.</param>
    /// <param name="points">
    /// Points to interpolate. For periodic curves if the final point is a
    /// duplicate of the initial point it is  ignored. (Count must be >=2)
    /// </param>
    /// <param name="knots">
    /// Knot-style to use  and specifies if the curve should be periodic.
    /// </param>
    /// <returns>interpolated curve on success. null on failure</returns>
    public static Curve CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree, CurveKnotStyle knots)
    {
      return CreateInterpolatedCurve(points, degree, knots, Vector3d.Unset, Vector3d.Unset);
    }
    /// <summary>
    /// Interpolates a sequence of points. Used by InterpCurve Command
    /// This routine works best when degree=3.
    /// </summary>
    /// <param name="degree">The degree of the curve >=1.  Degree must be odd.</param>
    /// <param name="points">
    /// Points to interpolate. For periodic curves if the final point is a
    /// duplicate of the initial point it is  ignored. (Count must be >=2)
    /// </param>
    /// <param name="knots">
    /// Knot-style to use  and specifies if the curve should be periodic.
    /// </param>
    /// <param name="startTangent"></param>
    /// <param name="endTangent"></param>
    /// <returns>interpolated curve on success. null on failure</returns>
    public static Curve CreateInterpolatedCurve(IEnumerable<Point3d> points, int degree, CurveKnotStyle knots, Vector3d startTangent, Vector3d endTangent)
    {
      if (null == points)
        throw new ArgumentNullException("points");

      int count = 0;
      Point3d[] ptArray = Point3dList.GetConstPointArray(points, out count);
      if (count < 2)
        throw new InvalidOperationException("Insufficient points for an interpolated curve");

      if (2 == count && !startTangent.IsValid && !endTangent.IsValid)
        return new LineCurve(ptArray[0], ptArray[1]);

      if (1 == degree && count > 2 && !startTangent.IsValid && !endTangent.IsValid)
        return PolylineCurve.FromArray(ptArray);
      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoInterpCurve(degree, count, ptArray, startTangent, endTangent, (int)knots);
      return GeometryBase.CreateGeometryHelper(ptr, null) as NurbsCurve;
    }

    /// <summary>
    /// Create a curve from a set of control-point locations.
    /// </summary>
    /// <param name="points">Control points.</param>
    /// <param name="degree">Degree of curve. The number of control points must be at least degree+1.</param>
    public static Curve CreateControlPointCurve(IEnumerable<Point3d> points, int degree)
    {
      int count = 0;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 2)
        return null;

      if (2 == count)
        return new LineCurve(ptArray[0], ptArray[1]);

      if (1 == degree && count > 2)
        return PolylineCurve.FromArray(ptArray);


      IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_CreateControlPointCurve(count, ptArray, degree);
      return GeometryBase.CreateGeometryHelper(ptr, null) as NurbsCurve;
    }
    /// <summary>
    /// Create a control-point of degree=3 (or less).
    /// </summary>
    /// <param name="points">Control points of curve.</param>
    public static Curve CreateControlPointCurve(IEnumerable<Point3d> points)
    {
      return CreateControlPointCurve(points, 3);
    }

    /// <summary>
    /// Join a collection of curve segments together.
    /// </summary>
    /// <param name="inputCurves">Curve segments to join.</param>
    /// <returns>An array of curves which contains</returns>
    public static Curve[] JoinCurves(IEnumerable<Curve> inputCurves)
    {
      return JoinCurves(inputCurves, 0.0, false);
    }
    /// <summary>
    /// Join a collection of curve segments together.
    /// </summary>
    /// <param name="inputCurves">Curve segments to join.</param>
    /// <param name="joinTolerance">Joining tolerance, 
    /// i.e. the distance between segment end-points that is allowed.</param>
    /// <returns>An array of curves which contains</returns>
    public static Curve[] JoinCurves(IEnumerable<Curve> inputCurves, double joinTolerance)
    {
      return JoinCurves(inputCurves, joinTolerance, false);
    }
    /// <summary>
    /// Join a collection of curve segments together.
    /// </summary>
    /// <param name="inputCurves">Curve segments to join.</param>
    /// <param name="joinTolerance">Joining tolerance, 
    /// i.e. the distance between segment end-points that is allowed.</param>
    /// <param name="preserveDirection">
    /// If true, curve endpoints will be compared to curve startpoints. 
    /// If false, all start and endpoints will be compared and copies of input curves may be reversed in output.
    /// </param>
    public static Curve[] JoinCurves(IEnumerable<Curve> inputCurves, double joinTolerance, bool preserveDirection)
    {
      // 1 March 2010 S. Baer
      // JoinCurves calls the unmanaged RhinoMergeCurves function which appears to be a "better"
      // implementation of ON_JoinCurves. We removed the wrapper for ON_JoinCurves for this reason.
      if (null == inputCurves)
        return null;

      SimpleArrayCurvePointer input = new SimpleArrayCurvePointer(inputCurves);
      IntPtr inputPtr = input.ConstPointer();
      SimpleArrayCurvePointer output = new SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();

      bool rc = UnsafeNativeMethods.RHC_RhinoMergeCurves(inputPtr,
        outputPtr, joinTolerance, preserveDirection);

      if (!rc)
        return null;
      return output.ToNonConstArray();
    }

    /// <summary>
    /// Find points at which to cut a pair of curves so that a fillet of given radius can be inserted.
    /// </summary>
    /// <param name="curve0">First curve to fillet.</param>
    /// <param name="curve1">Second curve to fillet.</param>
    /// <param name="radius">Fillet radius.</param>
    /// <param name="t0Base">Parameter value for base point on curve0.</param>
    /// <param name="t1Base">Parameter value for base point on curve1.</param>
    /// <param name="t0">Parameter value of fillet point on curve 0.</param>
    /// <param name="t1">Parameter value of fillet point on curve 1.</param>
    /// <param name="filletPlane">
    /// The fillet is contained in this plane with the fillet center at the plane origin.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>
    /// A fillet point is a pair of curve parameters (t0,t1) such that there is a circle
    /// of radius point3 tangent to curve c0 at t0 and tangent to curve c1 at t1. Of all possible
    /// fillet points this function returns the one which is the closest to the base point
    /// t0Base, t1Base. Distance from the base point is measured by the sum of arc lengths
    /// along the two curves. 
    /// </remarks>
    public static bool GetFilletPoints(Curve curve0, Curve curve1, double radius, double t0Base, double t1Base,
                                       out double t0, out double t1, out Plane filletPlane)
    {
      t0 = 0;
      t1 = 0;
      filletPlane = new Plane();
      if (null == curve0 || null == curve1)
        return false;
      IntPtr pCurve0 = curve0.ConstPointer();
      IntPtr pCurve1 = curve1.ConstPointer();
      bool rc = UnsafeNativeMethods.RHC_GetFilletPoints(pCurve0, pCurve1, radius, t0Base, t1Base, ref t0, ref t1, ref filletPlane);
      return rc;
    }
    /// <summary>
    /// Compute the fillet arc for a curve filleting operation.
    /// </summary>
    /// <param name="curve0">First curve to fillet.</param>
    /// <param name="curve1">Second curve to fillet.</param>
    /// <param name="radius">Fillet radius.</param>
    /// <param name="t0Base">Parameter on curve0 where the fillet ought to start (approximately).</param>
    /// <param name="t1Base">Parameter on curve1 where the fillet ought to end (approximately).</param>
    /// <returns>The fillet arc on success, or Arc.Unset on failure.</returns>
    public static Arc CreateFillet(Curve curve0, Curve curve1, double radius, double t0Base, double t1Base)
    {
      Arc arc = Arc.Unset;

      double t0, t1;
      Plane plane;
      if (GetFilletPoints(curve0, curve1, radius, t0Base, t1Base, out t0, out t1, out plane))
      {
        Vector3d radial0 = curve0.PointAt(t0) - plane.Origin;
        Vector3d radial1 = curve1.PointAt(t1) - plane.Origin;
        radial0.Unitize();
        radial1.Unitize();

        double angle = System.Math.Acos(radial0 * radial1);
        Plane fillet_plane = new Plane(plane.Origin, radial0, radial1);
        arc = new Arc(fillet_plane, plane.Origin, radius, angle);
      }
      return arc;
    }

    const int idxBooleanUnion = 0;
    const int idxBooleanIntersection = 1;
    const int idxBooleanDifference = 2;
    /// <summary>
    /// Calculates the boolean union of two or more closed, planar curves. 
    /// Note, curves must be co-planar.
    /// </summary>
    /// <param name="curves">The co-planar curves to union.</param>
    /// <returns>Result curves on success, null if no union could be calculated.</returns>
    public static Curve[] CreateBooleanUnion(IEnumerable<Curve> curves)
    {
      if (null == curves)
        return null;

      SimpleArrayCurvePointer input = new SimpleArrayCurvePointer(curves);
      IntPtr inputPtr = input.ConstPointer();
      SimpleArrayCurvePointer output = new SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();

      int rc = UnsafeNativeMethods.ON_Curve_BooleanOperation(inputPtr, outputPtr, idxBooleanUnion);
      if (rc < 1)
        return null;
      return output.ToNonConstArray();
    }
    /// <summary>
    /// Calculates the boolean intersection of two closed, planar curves. 
    /// Note, curves must be co-planar.
    /// </summary>
    /// <param name="curveA">The first closed, planar curve.</param>
    /// <param name="curveB">The second closed, planar curve.</param>
    /// <returns>Result curves on success, null if no intersection could be calculated.</returns>
    public static Curve[] CreateBooleanIntersection(Curve curveA, Curve curveB)
    {
      if (null == curveA || null == curveB)
        return null;

      SimpleArrayCurvePointer input = new SimpleArrayCurvePointer(new Curve[] { curveA, curveB });
      IntPtr inputPtr = input.ConstPointer();
      SimpleArrayCurvePointer output = new SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();
      int rc = UnsafeNativeMethods.ON_Curve_BooleanOperation(inputPtr, outputPtr, idxBooleanIntersection);
      if (rc < 1)
        return null;
      return output.ToNonConstArray();
    }
    /// <summary>
    /// Calculates the boolean difference between two closed, planar curves. 
    /// Note, curves must be co-planar.
    /// </summary>
    /// <param name="curveA">The first closed, planar curve.</param>
    /// <param name="curveB">The second closed, planar curve.</param>
    /// <returns>Result curves on success, null if no difference could be calculated.</returns>
    public static Curve[] CreateBooleanDifference(Curve curveA, Curve curveB)
    {
      if (null == curveA || null == curveB)
        return null;
      return CreateBooleanDifference(curveA, new Curve[] { curveB });
    }

    /// <summary>
    /// Calculates the boolean difference between a closed planar curve, and a list of closed planar curves. 
    /// Note, curves must be co-planar.
    /// </summary>
    /// <param name="curveA">The first closed, planar curve.</param>
    /// <param name="subtractors">curves to subtract from the first closed curve</param>
    /// <returns>Result curves on success, null if no difference could be calculated.</returns>
    public static Curve[] CreateBooleanDifference(Curve curveA, IEnumerable<Curve> subtractors)
    {
      if (null == curveA || null == subtractors)
        return null;

      List<Curve> curves = new List<Curve>();
      curves.Add(curveA);
      curves.AddRange(subtractors);
      SimpleArrayCurvePointer input = new SimpleArrayCurvePointer(curves);
      IntPtr inputPtr = input.ConstPointer();
      SimpleArrayCurvePointer output = new SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();
      int rc = UnsafeNativeMethods.ON_Curve_BooleanOperation(inputPtr, outputPtr, idxBooleanDifference);
      if (rc < 1)
        return null;
      return output.ToNonConstArray();
    }

    /// <summary>
    /// Compare two curves to see if they travel more or less in the same direction.
    /// </summary>
    /// <param name="curveA">First curve to test.</param>
    /// <param name="curveB">Second curve to test.</param>
    /// <returns>True if both curves more or less point in the same direction, 
    /// false if they point in the opposite directions.</returns>
    public static bool DoDirectionsMatch(Curve curveA, Curve curveB)
    {
      IntPtr ptr0 = curveA.ConstPointer();
      IntPtr ptr1 = curveB.ConstPointer();

      return UnsafeNativeMethods.ON_Curve_DoCurveDirectionsMatch(ptr0, ptr1);
    }

    public static Curve[] ProjectToMesh(Curve curve, Mesh mesh, Vector3d direction, double tolerance)
    {
      Curve[] curves = new Curve[] { curve };
      Mesh[] meshes = new Mesh[] { mesh };
      return ProjectToMesh(curves, meshes, direction, tolerance);
    }
    public static Curve[] ProjectToMesh(Curve curve, IEnumerable<Mesh> meshes, Vector3d direction, double tolerance)
    {
      Curve[] curves = new Curve[] { curve };
      return ProjectToMesh(curves, meshes, direction, tolerance);
    }
    public static Curve[] ProjectToMesh(IEnumerable<Curve> curves, IEnumerable<Mesh> meshes, Vector3d direction, double tolerance)
    {
      foreach (Curve crv in curves)
      {
        if (crv == null)
          throw new ArgumentNullException("List of curves contains a null entry");
      }
      List<GeometryBase> g = new List<GeometryBase>();
      foreach (Mesh msh in meshes)
      {
        if (msh == null)
          throw new ArgumentNullException("List of meshes contains a null entry");
        g.Add(msh);
      }

      SimpleArrayCurvePointer crv_array = new SimpleArrayCurvePointer(curves);
      Runtime.INTERNAL_GeometryArray mesh_array = new Runtime.INTERNAL_GeometryArray(g);

      IntPtr pCurvesIn = crv_array.ConstPointer();
      IntPtr pMeshes = mesh_array.ConstPointer();

      SimpleArrayCurvePointer curves_out = new SimpleArrayCurvePointer();
      IntPtr pCurvesOut = curves_out.NonConstPointer();

      Curve[] rc = null;
      if (UnsafeNativeMethods.RHC_RhinoProjectCurveToMesh(pMeshes, pCurvesIn, direction, tolerance, pCurvesOut))
      {
        rc = curves_out.ToNonConstArray();
      }

      crv_array.Dispose();
      mesh_array.Dispose();
      curves_out.Dispose();
      return rc;
    }

    /// <summary>
    /// Project a Curve onto a Brep along a given direction.
    /// </summary>
    /// <param name="curve">Curve to project.</param>
    /// <param name="brep">Brep to project onto.</param>
    /// <param name="direction">Direction of projection.</param>
    /// <param name="tolerance">Tolerance to use for projection.</param>
    /// <returns>An array of projected curves or null if the projection set is empty.</returns>
    public static Curve[] ProjectToBrep(Curve curve, Brep brep, Vector3d direction, double tolerance)
    {
      IntPtr brep_ptr = brep.ConstPointer();
      IntPtr curve_ptr = curve.ConstPointer();

      SimpleArrayCurvePointer rc = new SimpleArrayCurvePointer();
      IntPtr rc_ptr = rc.NonConstPointer();

      if (UnsafeNativeMethods.RHC_RhinoProjectCurveToBrep(brep_ptr, curve_ptr, direction, tolerance, rc_ptr))
      { return rc.ToNonConstArray(); }

      return null;
    }
    /// <summary>
    /// Project a Curve onto a collection of Breps along a given direction.
    /// </summary>
    /// <param name="curve">Curve to project.</param>
    /// <param name="breps">Breps to project onto.</param>
    /// <param name="direction">Direction of projection.</param>
    /// <param name="tolerance">Tolerance to use for projection.</param>
    /// <returns>An array of projected curves or null if the projection set is empty.</returns>
    public static Curve[] ProjectToBrep(Curve curve, IEnumerable<Brep> breps, Vector3d direction, double tolerance)
    {
      int[] brep_ids;
      return ProjectToBrep(curve, breps, direction, tolerance, out brep_ids);
    }
    /// <summary>
    /// Project a Curve onto a collection of Breps along a given direction.
    /// </summary>
    /// <param name="curve">Curve to project.</param>
    /// <param name="breps">Breps to project onto.</param>
    /// <param name="direction">Direction of projection.</param>
    /// <param name="tolerance">Tolerance to use for projection.</param>
    /// <param name="brepIndices">(out) Integers that identify for each resulting curve which Brep it was projected onto.</param>
    /// <returns>An array of projected curves or null if the projection set is empty.</returns>
    public static Curve[] ProjectToBrep(Curve curve, IEnumerable<Brep> breps, Vector3d direction, double tolerance, out int[] brepIndices)
    {
      int[] curveIndices = null;
      IEnumerable<Curve> crvs = new Curve[] { curve };
      return ProjectToBrep(crvs, breps, direction, tolerance, out curveIndices, out brepIndices);
    }
    /// <summary>
    /// Project a collection of Curves onto a collection of Breps along a given direction.
    /// </summary>
    /// <param name="curves">Curves to project.</param>
    /// <param name="breps">Breps to project onto.</param>
    /// <param name="direction">Direction of projection.</param>
    /// <param name="tolerance">Tolerance to use for projection.</param>
    /// <returns>An array of projected curves or null if the projection set is empty.</returns>
    public static Curve[] ProjectToBrep(IEnumerable<Curve> curves, IEnumerable<Brep> breps, Vector3d direction, double tolerance)
    {
      int[] c_top;
      int[] b_top;
      return ProjectToBrep(curves, breps, direction, tolerance, out c_top, out b_top);
    }
    /// <summary>
    /// Project a collection of Curves onto a collection of Breps along a given direction.
    /// </summary>
    /// <param name="curves">Curves to project.</param>
    /// <param name="breps">Breps to project onto.</param>
    /// <param name="direction">Direction of projection.</param>
    /// <param name="tolerance">Tolerance to use for projection.</param>
    /// <param name="curveIndices">Index of which curve in the input list was the source for a curve in the return array.</param>
    /// <param name="brepIndices">Index of which brep was used to generate a curve in the return array.</param>
    /// <returns>An array of projected curves or null if the projection set is empty.</returns>
    public static Curve[] ProjectToBrep(IEnumerable<Curve> curves, IEnumerable<Brep> breps, Vector3d direction, double tolerance, out int[] curveIndices, out int[] brepIndices)
    {
      curveIndices = null;
      brepIndices = null;

      foreach (Curve crv in curves) { if (crv == null) { throw new ArgumentNullException("List of curves contains a null entry"); } }
      foreach (Brep brp in breps) { if (brp == null) { throw new ArgumentNullException("List of breps contains a null entry"); } }

      SimpleArrayCurvePointer crv_array = new SimpleArrayCurvePointer(curves);
      Runtime.INTERNAL_BrepArray brp_array = new Runtime.INTERNAL_BrepArray();
      foreach (Brep brp in breps) { brp_array.AddBrep(brp, true); }

      IntPtr ptr_crv_array = crv_array.ConstPointer();
      IntPtr ptr_brp_array = brp_array.ConstPointer();

      Runtime.INTERNAL_IntArray brp_top = new Rhino.Runtime.INTERNAL_IntArray();
      Runtime.INTERNAL_IntArray crv_top = new Rhino.Runtime.INTERNAL_IntArray();

      SimpleArrayCurvePointer rc = new SimpleArrayCurvePointer();
      IntPtr ptr_rc = rc.NonConstPointer();

      if (UnsafeNativeMethods.RHC_RhinoProjectCurveToBrepEx(ptr_brp_array,
                                                            ptr_crv_array,
                                                            direction,
                                                            tolerance,
                                                            ptr_rc,
                                                            brp_top.m_ptr,
                                                            crv_top.m_ptr))
      {
        brepIndices = brp_top.ToArray();
        curveIndices = crv_top.ToArray();
        return rc.ToNonConstArray();
      }

      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="curveA"></param>
    /// <param name="curveB"></param>
    /// <param name="tolerance"></param>
    /// <param name="maxDistance"></param>
    /// <param name="maxDistanceParameterA"></param>
    /// <param name="maxDistanceParameterB"></param>
    /// <param name="minDistance"></param>
    /// <param name="minDistanceParameterA"></param>
    /// <param name="minDistanceParameterB"></param>
    /// <returns></returns>
    public static bool GetDistancesBetweenCurves(Curve curveA, Curve curveB, double tolerance,
      out double maxDistance, out double maxDistanceParameterA, out double maxDistanceParameterB,
      out double minDistance, out double minDistanceParameterA, out double minDistanceParameterB)
    {
      IntPtr pConstCrvA = curveA.ConstPointer();
      IntPtr pConstCrvB = curveB.ConstPointer();
      maxDistance = 0;
      maxDistanceParameterA = 0;
      maxDistanceParameterB = 0;
      minDistance = 0;
      minDistanceParameterA = 0;
      minDistanceParameterB = 0;

      bool rc = UnsafeNativeMethods.RHC_RhinoGetOverlapDistance(pConstCrvA, pConstCrvB, tolerance,
        ref maxDistanceParameterA, ref maxDistanceParameterB, ref maxDistance,
        ref minDistanceParameterA, ref minDistanceParameterB, ref minDistance);
      return rc;
    }

    public static int PlanarClosedCurveContainmentTest(Curve curveA, Curve curveB, Plane testPlane, double tolerance)
    {
      IntPtr pConstCurveA = curveA.ConstPointer();
      IntPtr pConstCurveB = curveB.ConstPointer();
      return UnsafeNativeMethods.RHC_RhinoPlanarClosedCurveContainmentTest(pConstCurveA, pConstCurveB, ref testPlane, tolerance);
    }
    #endregion statics

    #region constructors
    protected Curve() { }

    internal Curve(IntPtr ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(ptr, parent_object, obj_ref)
    {
    }
    internal Curve(IntPtr ptr, object parent, int subobject_index)
      : base(ptr, parent, subobject_index)
    {
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      if (m_subobject_index >= 0)
      {
        Rhino.Geometry.PolyCurve polycurve_parent = m__parent as Rhino.Geometry.PolyCurve;
        if (polycurve_parent != null)
        {
          IntPtr pConstPolycurve = polycurve_parent.ConstPointer();
          IntPtr pConstThis = UnsafeNativeMethods.ON_PolyCurve_SegmentCurve(pConstPolycurve, m_subobject_index);
          return pConstThis;
        }
      }
      return base._InternalGetConstPointer();
    }

    //private PolyCurve m_parent_polycurve; //runtime will initialize this to null
    //internal void SetParentPolyCurve(PolyCurve curve)
    //{
    //  m_ptr = IntPtr.Zero;
    //  m_parent_polycurve = curve;
    //}

    /// <summary>
    /// Create an exact duplicate of this Curve.
    /// </summary>
    /// <seealso cref="DuplicateCurve"/>
    public override GeometryBase Duplicate()
    {
      IntPtr ptr = ConstPointer();
      IntPtr pNewCurve = UnsafeNativeMethods.ON_Curve_DuplicateCurve(ptr);
      return GeometryBase.CreateGeometryHelper(pNewCurve, null) as Curve;
    }

    /// <summary>
    /// Create an exact duplicate of this curve.
    /// </summary>
    /// <returns>An exact copy of this curve.</returns>
    public Curve DuplicateCurve()
    {
      Curve rc = Duplicate() as Curve;
      return rc;
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Curve(IntPtr.Zero, null, null);
    }

#if USING_V5_SDK
    /// <summary>
    /// Polylines will be exploded into line segments. ExplodeCurves will
    /// return the curves in topological order.
    /// </summary>
    /// <returns>
    /// An array of all the segments that make up this curve.
    /// </returns>
    public Curve[] DuplicateSegments()
    {
      IntPtr ptr = ConstPointer();
      SimpleArrayCurvePointer output = new SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();

      int rc = UnsafeNativeMethods.RHC_RhinoDuplicateCurveSegments(ptr, outputPtr);
      if (rc < 1)
        return null;
      return output.ToNonConstArray();
    }
#endif

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = true;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_DuplicateCurve(pConstPointer);
    }

    protected override void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pCurveDisplay)
      {
        UnsafeNativeMethods.CurveDisplay_Delete(m_pCurveDisplay);
        m_pCurveDisplay = IntPtr.Zero;
      }
      base.Dispose(disposing);
    }
    #endregion

    #region internal methods
    const int idxIgnoreNone = 0;
    const int idxIgnorePlane = 1;
    const int idxIgnorePlaneArcOrEllipse = 2;

    protected override void NonConstOperation()
    {
      if (IntPtr.Zero != m_pCurveDisplay)
      {
        UnsafeNativeMethods.CurveDisplay_Delete(m_pCurveDisplay);
        m_pCurveDisplay = IntPtr.Zero;
      }
      base.NonConstOperation();
    }

    internal IntPtr m_pCurveDisplay = IntPtr.Zero;
    internal virtual void Draw(Display.DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      IntPtr ptr = ConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCurve(pDisplayPipeline, ptr, argb, thickness);
    }

    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the domain of the curve.
    /// </summary>
    public Interval Domain
    {
      get
      {
        Interval rc = new Interval();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_Curve_Domain(ptr, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Curve_Domain(ptr, true, ref value);
      }
    }

    public int Dimension
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Curve_Dimension(ptr);
      }
    }
    // skipping ChangeDimension for now. seems slightly unusual for plug-in developers
    // to use non 3 dimensional curves
    //public bool ChangeDimension(int desired_dimension)

    /// <summary>
    /// Gets the number of non-empty smooth (c-infinity) spans in the curve.
    /// </summary>
    public int SpanCount
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Curve_SpanCount(ptr);
      }
    }

    /// <summary>
    /// Gets the maximum algebraic degree of any span
    /// or a good estimate if curve spans are not algebraic.
    /// </summary>
    public int Degree
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Curve_Degree(ptr);
      }
    }

    #region platonic shape properties
    /// <summary>
    /// Test a curve to see if it is linear to within RhinoMath.ZeroTolerance units (1e-12).
    /// </summary>
    /// <returns>True if the curve is linear.</returns>
    public bool IsLinear()
    {
      return IsLinear(RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Test a curve to see if it is linear to within the custom tolerance.
    /// </summary>
    /// <param name="tolerance">Tolerance to use when checking linearity.</param>
    /// <returns>
    /// True if the ends of the curve are farther than tolerance apart
    /// and the maximum distance from any point on the curve to
    /// the line segment connecting the curve ends is &lt;= tolerance.
    /// </returns>
    public bool IsLinear(double tolerance)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsLinear(ptr, tolerance);
    }

    /// <summary>
    /// Several types of Curve can have the form of a polyline
    /// including a degree 1 NurbsCurve, a PolylineCurve,
    /// and a PolyCurve all of whose segments are some form of
    /// polyline. IsPolyline tests a curve to see if it can be
    /// represented as a polyline.
    /// </summary>
    /// <returns></returns>
    public bool IsPolyline()
    {
      IntPtr ptr = ConstPointer();
      return (UnsafeNativeMethods.ON_Curve_IsPolyline1(ptr, IntPtr.Zero) != 0);
    }
    /// <summary>
    /// Several types of Curve can have the form of a polyline 
    /// including a degree 1 NurbsCurve, a PolylineCurve, 
    /// and a PolyCurve all of whose segments are some form of 
    /// polyline. IsPolyline tests a curve to see if it can be 
    /// represented as a polyline.
    /// </summary>
    /// <param name="polyline">
    /// If true is returned, then the polyline form is returned here.
    /// </param>
    /// <returns>True if the curve can be represented as a polyline, false if not.</returns>
    public bool TryGetPolyline(out Polyline polyline)
    {
      polyline = null;

      SimpleArrayPoint3d outputPts = new SimpleArrayPoint3d();
      int pointCount = 0;
      IntPtr pCurve = ConstPointer();
      IntPtr outputPointsPointer = outputPts.NonConstPointer();

      UnsafeNativeMethods.ON_Curve_IsPolyline2(pCurve, outputPointsPointer, ref pointCount);
      if (pointCount > 0)
      {
        polyline = Polyline.PolyLineFromNativeArray(outputPts);
      }

      outputPts.Dispose();
      return (pointCount != 0);
    }
    /// <summary>
    /// Several types of Curve can have the form of a polyline 
    /// including a degree 1 NurbsCurve, a PolylineCurve, 
    /// and a PolyCurve all of whose segments are some form of 
    /// polyline. IsPolyline tests a curve to see if it can be 
    /// represented as a polyline.
    /// </summary>
    /// <param name="polyline">
    /// If true is returned, then the polyline form is returned here.
    /// </param>
    /// <param name="parameters">
    /// if true is returned, then the parameters of the polyline
    /// points are returned here
    /// </param>
    /// <returns>True if the curve can be represented as a polyline, false if not.</returns>
    public bool TryGetPolyline(out Polyline polyline, out double[] parameters)
    {
      polyline = null;
      parameters = null;
      SimpleArrayPoint3d outputPts = new SimpleArrayPoint3d();
      int pointCount = 0;
      IntPtr pCurve = ConstPointer();
      IntPtr outputPointsPointer = outputPts.NonConstPointer();
      IntPtr pParameters = UnsafeNativeMethods.ON_Curve_IsPolyline2(pCurve, outputPointsPointer, ref pointCount);
      if (pointCount > 0)
      {
        polyline = Polyline.PolyLineFromNativeArray(outputPts);
        if (pParameters != IntPtr.Zero)
        {
          parameters = new double[pointCount];
          System.Runtime.InteropServices.Marshal.Copy(pParameters, parameters, 0, pointCount);
        }
      }

      outputPts.Dispose();
      return (pointCount != 0);
    }

    /// <summary>
    /// Test a curve to see if it can be represented by an arc or circle within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <returns>
    /// True if the curve can be represented by an Arc or a Circle to within tolerance.
    /// </returns>
    public bool IsArc()
    {
      return IsArc(RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Test a curve to see if it can be represented by an arc or circle within the given tolerance.
    /// </summary>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>
    /// True if the curve can be represented by an Arc or a Circle to within tolerance.
    /// </returns>
    public bool IsArc(double tolerance)
    {
      Arc arc = new Arc();
      Plane p = Plane.WorldXY;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsArc(ptr, idxIgnorePlaneArcOrEllipse, ref p, ref arc, tolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Arc using RhinoMath.ZeroTolerance.
    /// </summary>
    /// <param name="arc">On success, the Arc will be filled in.</param>
    /// <returns>True if the curve could be converted into an Arc.</returns>
    public bool TryGetArc(out Arc arc)
    {
      return TryGetArc(out arc, RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Arc using a custom tolerance.
    /// </summary>
    /// <param name="arc">On success, the Arc will be filled in.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>True if the curve could be converted into an Arc.</returns>
    public bool TryGetArc(out Arc arc, double tolerance)
    {
      arc = new Arc();
      Plane p = Plane.WorldXY;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsArc(ptr, idxIgnorePlane, ref p, ref arc, tolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Arc using RhinoMath.ZeroTolerance.
    /// </summary>
    /// <param name="plane">Plane in which the comparison is performed.</param>
    /// <param name="arc">On success, the Arc will be filled in.</param>
    /// <returns>True if the curve could be converted into an Arc within the given plane.</returns>
    public bool TryGetArc(Plane plane, out Arc arc)
    {
      return TryGetArc(plane, out arc, RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Arc using a custom tolerance.
    /// </summary>
    /// <param name="plane">Plane in which the comparison is performed.</param>
    /// <param name="arc">On success, the Arc will be filled in.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>True if the curve could be converted into an Arc within the given plane.</returns>
    public bool TryGetArc(Plane plane, out Arc arc, double tolerance)
    {
      arc = new Arc();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsArc(ptr, idxIgnoreNone, ref plane, ref arc, tolerance);
    }

    /// <summary>
    /// Test a curve to see if it can be represented by a circle within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <returns>
    /// True if the Curve can be represented by a circle within tolerance.
    /// </returns>
    public bool IsCircle()
    {
      return IsCircle(RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Test a curve to see if it can be represented by a circle within the given tolerance.
    /// </summary>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>
    /// True if the curve can be represented by a Circle to within tolerance.
    /// </returns>
    public bool IsCircle(double tolerance)
    {
      Arc arc = new Arc();
      if (TryGetArc(out arc, tolerance))
      {
        return arc.IsCircle;
      }

      return false;
    }
    /// <summary>
    /// Try to convert this curve into a Circle using RhinoMath.ZeroTolerance.
    /// </summary>
    /// <param name="circle">On success, the Circle will be filled in.</param>
    /// <returns>True if the curve could be converted into a Circle.</returns>
    public bool TryGetCircle(out Circle circle)
    {
      return TryGetCircle(out circle, RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Try to convert this curve into a Circle using a custom tolerance.
    /// </summary>
    /// <param name="circle">On success, the Circle will be filled in.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>True if the curve could be converted into a Circle within tolerance.</returns>
    public bool TryGetCircle(out Circle circle, double tolerance)
    {
      circle = new Circle();

      Arc arc = new Arc();
      if (TryGetArc(out arc, tolerance))
      {
        if (arc.IsCircle)
        {
          circle = new Circle(arc);
          return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Test a curve to see if it can be represented by an ellipse within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <returns>
    /// True if the Curve can be represented by an ellipse within tolerance.
    /// </returns>
    public bool IsEllipse()
    {
      return IsEllipse(RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Test a curve to see if it can be represented by an ellipse within a given tolerance.
    /// </summary>
    /// <param name="tolerance">Tolerance to use for checking.</param>
    /// <returns>
    /// True if the Curve can be represented by an ellipse within tolerance.
    /// </returns>
    public bool IsEllipse(double tolerance)
    {
      Plane plane = Plane.WorldXY;
      Ellipse ellipse = new Ellipse();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsEllipse(ptr, idxIgnorePlaneArcOrEllipse, ref plane, ref ellipse, tolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Ellipse within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <param name="ellipse">On success, the Ellipse will be filled in.</param>
    /// <returns>True if the curve could be converted into an Ellipse.</returns>
    public bool TryGetEllipse(out Ellipse ellipse)
    {
      return TryGetEllipse(out ellipse, RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Ellipse using a custom tolerance.
    /// </summary>
    /// <param name="ellipse">On success, the Ellipse will be filled in.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>True if the curve could be converted into an Ellipse.</returns>
    public bool TryGetEllipse(out Ellipse ellipse, double tolerance)
    {
      Plane plane = Plane.WorldXY;
      ellipse = new Ellipse();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsEllipse(ptr, idxIgnorePlane, ref plane, ref ellipse, tolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Ellipse within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <param name="plane">Plane in which the comparison is performed.</param>
    /// <param name="ellipse">On success, the Ellipse will be filled in.</param>
    /// <returns>True if the curve could be converted into an Ellipse within the given plane.</returns>
    public bool TryGetEllipse(Plane plane, out Ellipse ellipse)
    {
      return TryGetEllipse(plane, out ellipse, RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Try to convert this curve into an Ellipse using a custom tolerance.
    /// </summary>
    /// <param name="plane">Plane in which the comparison is performed.</param>
    /// <param name="ellipse">On success, the Ellipse will be filled in.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>True if the curve could be converted into an Ellipse within the given plane.</returns>
    public bool TryGetEllipse(Plane plane, out Ellipse ellipse, double tolerance)
    {
      ellipse = new Ellipse();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsEllipse(ptr, idxIgnoreNone, ref plane, ref ellipse, tolerance);
    }

    /// <summary>Test a curve for planarity.</summary>
    /// <returns>
    /// True if the curve is planar (flat) to within RhinoMath.ZeroTolerance units (1e-12).
    /// </returns>
    public bool IsPlanar()
    {
      return IsPlanar(RhinoMath.ZeroTolerance);
    }
    /// <summary>Test a curve for planarity.</summary>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>
    /// True if there is a plane such that the maximum distance from the curve to the plane is &lt;= tolerance.
    /// </returns>
    public bool IsPlanar(double tolerance)
    {
      Plane plane = Plane.WorldXY;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsPlanar(ptr, true, ref plane, tolerance);
    }
    /// <summary>Test a curve for planarity and return the plane.</summary>
    /// <param name="plane">On success, the plane parameters are filled in.</param>
    /// <returns>
    /// True if there is a plane such that the maximum distance from the curve to the plane is &lt;= RhinoMath.ZeroTolerance.
    /// </returns>
    public bool TryGetPlane(out Plane plane)
    {
      return TryGetPlane(out plane, RhinoMath.ZeroTolerance);
    }
    /// <summary>Test a curve for planarity and return the plane.</summary>
    /// <param name="plane">On success, the plane parameters are filled in.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>
    /// True if there is a plane such that the maximum distance from the curve to the plane is &lt;= tolerance.
    /// </returns>
    public bool TryGetPlane(out Plane plane, double tolerance)
    {
      plane = Plane.WorldXY;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsPlanar(ptr, false, ref plane, tolerance);
    }
    /// <summary>Test a curve to see if it lies in a specific plane.</summary>
    /// <param name="testPlane">Plane to test for.</param>
    /// <returns>
    /// True if the maximum distance from the curve to the testPlane is &lt;= RhinoMath.ZeroTolerance.
    /// </returns>
    public bool IsInPlane(Plane testPlane)
    {
      return IsInPlane(testPlane, RhinoMath.ZeroTolerance);
    }
    /// <summary>Test a curve to see if it lies in a specific plane.</summary>
    /// <param name="testPlane">Plane to test for.</param>
    /// <param name="tolerance">Tolerance to use when checking.</param>
    /// <returns>
    /// True if the maximum distance from the curve to the testPlane is &lt;= tolerance.
    /// </returns>
    public bool IsInPlane(Plane testPlane, double tolerance)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsInPlane(ptr, ref testPlane, tolerance);
    }
    #endregion
    #endregion

    #region methods
    /// <summary>
    /// If this curve is closed, then modify it so that the start/end point is at curve parameter t.
    /// </summary>
    /// <param name="t">
    /// Curve parameter of new start/end point. The returned curves domain will start at t.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool ChangeClosedCurveSeam(double t)
    {
      IntPtr ptr = NonConstPointer();
      bool rc = UnsafeNativeMethods.ON_Curve_ChangeClosedCurveSeam(ptr, t);
      return rc;
    }

    const int idxIsClosed = 0;
    const int idxIsPeriodic = 1;

    /// <summary>
    /// Gets a value indicating whether or not this curve is a closed curve.
    /// </summary>
    public bool IsClosed
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Curve_GetBool(ptr, idxIsClosed);
      }
    }
    /// <summary>
    /// Gets a value indicating whether or not this curve is considered to be Periodic.
    /// </summary>
    public bool IsPeriodic
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Curve_GetBool(ptr, idxIsPeriodic);
      }
    }

    /// <summary>
    /// Decide if it makes sense to close off this curve by moving the endpoint 
    /// to the start based on start-end gap size and length of curve as 
    /// approximated by chord defined by 6 points.
    /// </summary>
    /// <param name="tolerance">
    /// Maximum allowable distance between start and end. 
    /// If start - end gap is greater than tolerance, this function will return False.
    /// </param>
    /// <returns>True if start and end points are close enough based on above conditions.</returns>
    public bool IsClosable(double tolerance)
    {
      return IsClosable(tolerance, 0.0, 10.0);
    }
    /// <summary>
    /// Decide if it makes sense to close off this curve by moving the endpoint
    /// to the start based on start-end gap size and length of curve as
    /// approximated by chord defined by 6 points.
    /// </summary>
    /// <param name="tolerance">
    /// Maximum allowable distance between start and end. 
    /// If start - end gap is greater than tolerance, this function will return False.
    /// </param>
    /// <param name="minimumAbsoluteSize">
    /// If greater than 0.0 and none of the interior sampled points are at
    /// least minimumAbsoluteSize from start, this function will return False.
    /// </param>
    /// <param name="minimumRelativeSize">
    /// If greater than 1.0 and chord length is less than 
    /// minimumRelativeSize*gap, this function will return False.
    /// </param>
    /// <returns>True if start and end points are close enough based on above conditions.</returns>
    public bool IsClosable(double tolerance, double minimumAbsoluteSize, double minimumRelativeSize)
    {
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Curve_IsClosable(ptr, tolerance, minimumAbsoluteSize, minimumRelativeSize);
      return rc;
    }

    /// <summary>
    /// If IsClosed, just return True. Otherwise, decide if curve can be closed as 
    /// follows: Linear curves polylinear curves with 2 segments, Nurbs with 3 or less 
    /// control points cannot be made closed. Also, if tolerance > 0 and the gap between 
    /// start and end is larger than tolerance, curve cannot be made closed. 
    /// Adjust the curve's endpoint to match its start point.
    /// </summary>
    /// <param name="tolerance">
    /// If nonzero, and the gap is more than tolerance, curve cannot be made closed.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool MakeClosed(double tolerance)
    {
      if (IsClosed)
        return true;
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoMakeCurveClosed(ptr, tolerance);
    }


    /// <summary>
    /// Determine the orientation (counterclockwise or clockwise) of a closed planar curve in a given plane.
    /// Only works with simple (no self intersections) closed planar curves.
    /// </summary>
    /// <param name="upDirection">
    /// </param>
    /// <returns>The orientation of this curve with respect to a defined up direction.</returns>
    public CurveOrientation ClosedCurveOrientation(Vector3d upDirection)
    {
      Plane plane = new Plane(Point3d.Origin, upDirection);
      return ClosedCurveOrientation(plane);
    }
    /// <summary>
    /// Determine the orientation (counterclockwise or clockwise) of a closed planar curve in a given plane.
    /// Only works with simple (no self intersections) closed planar curves.
    /// </summary>
    /// <param name="plane">
    /// The plane in which to solve the orientation.
    /// </param>
    /// <returns>The orientation of this curve in the given plane.</returns>
    public CurveOrientation ClosedCurveOrientation(Plane plane)
    {
      if (!plane.IsValid) { return CurveOrientation.Undefined; }

      //WARNING! David wrote this code without testing it. Is the order of planes in the ChangeBasis function correct?
      Transform xform = Geometry.Transform.ChangeBasis(Plane.WorldXY, plane);
      return ClosedCurveOrientation(xform);
    }
    /// <summary>
    /// Determine the orientation (counterclockwise or clockwise) of a closed planar curve.
    /// Only works with simple (no self intersections) closed planar curves.
    /// </summary>
    /// <param name="xform">
    /// Transformation to map the curve to the xy plane. If the curve is parallel
    /// to the xy plane, you may pass Identity matrix.
    /// </param>
    /// <returns>The orientation of this curve in the world xy-plane.</returns>
    public CurveOrientation ClosedCurveOrientation(Transform xform)
    {
      IntPtr ptr = ConstPointer();
      int orientation = UnsafeNativeMethods.ON_Curve_ClosedCurveOrientation(ptr, ref xform);

      if (orientation == +1) { return CurveOrientation.Clockwise; }
      if (orientation == -1) { return CurveOrientation.CounterClockwise; }

      return CurveOrientation.Undefined;
    }

    /// <summary>
    /// Reverse the direction of the curve.
    /// </summary>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>If reversed, the domain changes from [a,b] to [-b,-a]</remarks>
    public bool Reverse()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Curve_Reverse(ptr);
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoint() instead.")]
    public bool GetClosestPoint(Point3d testPoint, out double t)
    {
      return ClosestPoint(testPoint, out t);
    }
    /// <summary>
    /// Find parameter of the point on a curve that is closest to testPoint.
    /// If the maximumDistance parameter is > 0, then only points whose distance
    /// to the given point is &lt;= maximumDistance will be returned.  Using a 
    /// positive value of maximumDistance can substantially speed up the search.
    /// </summary>
    /// <param name="testPoint">Point to search from.</param>
    /// <param name="t">Parameter of local closest point.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool ClosestPoint(Point3d testPoint, out double t)
    {
      return ClosestPoint(testPoint, out t, -1.0);
    }
    /// <summary>
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoint() instead.")]
    public bool GetClosestPoint(Point3d testPoint, out double t, double maximumDistance)
    {
      return ClosestPoint(testPoint, out t, maximumDistance);
    }
    /// <summary>
    /// Find parameter of the point on a curve that is closest to testPoint.
    /// If the maximumDistance parameter is > 0, then only points whose distance
    /// to the given point is &lt;= maximumDistance will be returned.  Using a 
    /// positive value of maximumDistance can substantially speed up the search.
    /// </summary>
    /// <param name="testPoint">Point to project.</param>
    /// <param name="t">parameter of local closest point returned here</param>
    /// <param name="maximumDistance"></param>
    /// <returns>True on success, false on failure.</returns>
    public bool ClosestPoint(Point3d testPoint, out double t, double maximumDistance)
    {
      t = 0.0;
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Curve_GetClosestPoint(ptr, testPoint, ref t, maximumDistance);
      return rc;
    }

#if USING_V5_SDK
    /// <summary>
    /// This method is Obsolete, use ClosestPoints() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoints() instead.")]
    public bool GetClosestPoints(IEnumerable<GeometryBase> geometry,
      out Point3d pointOnCurve,
      out Point3d pointOnObject,
      out int whichGeometry,
      double maximumDistance)
    {
      return ClosestPoints(geometry, out pointOnCurve, out pointOnObject, out whichGeometry, maximumDistance);
    }
    /// <summary>
    /// Finds the object, and the closest point in that object, that is closest to
    /// this curve. Allowable objects to test the curve against include Brep, Surface,
    /// Curve, and PointCloud objects.
    /// </summary>
    /// <param name="geometry">list of geometry to test this curve against</param>
    /// <param name="pointOnCurve"></param>
    /// <param name="pointOnObject"></param>
    /// <param name="whichGeometry"></param>
    /// <param name="maximumDistance">maximum allowable distance</param>
    /// <returns>True on success, false on failure</returns>
    public bool ClosestPoints(IEnumerable<GeometryBase> geometry,
      out Point3d pointOnCurve,
      out Point3d pointOnObject,
      out int whichGeometry,
      double maximumDistance)
    {
      Runtime.INTERNAL_GeometryArray geom = new Rhino.Runtime.INTERNAL_GeometryArray(geometry);
      pointOnCurve = Point3d.Unset;
      pointOnObject = Point3d.Unset;
      IntPtr pConstThis = ConstPointer();
      IntPtr pGeometryArray = geom.ConstPointer();
      whichGeometry = 0;
      bool rc = UnsafeNativeMethods.RHC_RhinoGetClosestPoint(pConstThis, pGeometryArray, maximumDistance, ref pointOnCurve, ref pointOnObject, ref whichGeometry);
      geom.Dispose();
      return rc;
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoints() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoints() instead.")]
    public bool GetClosestPoints(IEnumerable<GeometryBase> geometry,
      out Point3d pointOnCurve,
      out Point3d pointOnObject,
      out int whichGeometry)
    {
      return ClosestPoints(geometry, out pointOnCurve, out pointOnObject, out whichGeometry);
    }
    /// <summary>
    /// Finds the object, and the closest point in that object, that is closest to
    /// this curve. Allowable objects to test the curve against include Brep, Surface,
    /// Curve, and PointCloud objects.
    /// </summary>
    /// <param name="geometry">list of geometry to test this curve against</param>
    /// <param name="pointOnCurve"></param>
    /// <param name="pointOnObject"></param>
    /// <param name="whichGeometry"></param>
    /// <returns>True on success, false on failure</returns>
    public bool ClosestPoints(IEnumerable<GeometryBase> geometry,
      out Point3d pointOnCurve,
      out Point3d pointOnObject,
      out int whichGeometry)
    {
      return ClosestPoints(geometry, out pointOnCurve, out pointOnObject, out whichGeometry, 0.0);
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoints() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoints() instead.")]
    public bool GetClosestPoints(Curve otherCurve, out Point3d pointOnThisCurve, out Point3d pointOnOtherCurve)
    {
      return ClosestPoints(otherCurve, out pointOnThisCurve, out pointOnOtherCurve);
    }
    /// <summary>
    /// Get closest points between two curves
    /// </summary>
    /// <param name="otherCurve"></param>
    /// <param name="pointOnThisCurve"></param>
    /// <param name="pointOnOtherCurve"></param>
    /// <returns></returns>
    public bool ClosestPoints(Curve otherCurve, out Point3d pointOnThisCurve, out Point3d pointOnOtherCurve)
    {
      GeometryBase[] a = new GeometryBase[] { otherCurve };
      int which = 0;
      return ClosestPoints(a, out pointOnThisCurve, out pointOnOtherCurve, out which, 0.0);
    }
#endif

    /// <summary>
    /// Compute the relationship between a point and a closed curve region. 
    /// This curve must be closed or the return value will be Unset.
    /// Both curve and point are projected to the World XY plane.
    /// </summary>
    /// <param name="testPoint">Point to test.</param>
    /// <returns>Relationship between point and curve region.</returns>
    public PointContainment Contains(Point3d testPoint)
    {
      return Contains(testPoint, Plane.WorldXY, 0.0);
    }
    /// <summary>
    /// Compute the relationship between a point and a closed curve region. 
    /// This curve must be closed or the return value will be Unset.
    /// </summary>
    /// <param name="testPoint">Point to test.</param>
    /// <param name="plane">Plane in in which to compare point and region.</param>
    /// <returns>Relationship between point and curve region.</returns>
    public PointContainment Contains(Point3d testPoint, Plane plane)
    {
      return Contains(testPoint, plane, 0.0);
    }
    /// <summary>
    /// Compute the relationship between a point and a closed curve region. 
    /// This curve must be closed or the return value will be Unset.
    /// </summary>
    /// <param name="testPoint">Point to test.</param>
    /// <param name="plane">Plane in in which to compare point and region.</param>
    /// <param name="tolerance">Tolerance to use during comparison.</param>
    /// <returns>Relationship between point and curve region.</returns>
    public PointContainment Contains(Point3d testPoint, Plane plane, double tolerance)
    {
      if (testPoint.IsValid && plane.IsValid && IsClosed)
      {
        IntPtr ptr = ConstPointer();
        int rc = UnsafeNativeMethods.RHC_PointInClosedRegion(ptr, testPoint, plane, tolerance);

        if (0 == rc)
          return PointContainment.Outside;
        if (1 == rc)
          return PointContainment.Inside;
        if (2 == rc)
          return PointContainment.Coincident;
      }
      return PointContainment.Unset;
    }

    #region evaluators
    // [skipping]
    // BOOL EvPoint
    // BOOL Ev1Der
    // BOOL Ev2Der
    // BOOL EvTangent
    // BOOL EvCurvature
    // BOOL Evaluate

    const int idxPointAtT = 0;
    const int idxPointAtStart = 1;
    const int idxPointAtEnd = 2;

    /// <summary>Evaluate point at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>Point (location of curve at the parameter t).</returns>
    /// <remarks>No error handling.</remarks>
    public Point3d PointAt(double t)
    {
      Point3d rc = new Point3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Curve_PointAt(ptr, t, ref rc, idxPointAtT);
      return rc;
    }
    /// <summary>
    /// Evaluate point at the start of the curve.
    /// </summary>
    public Point3d PointAtStart
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_Curve_PointAt(ptr, 0, ref rc, idxPointAtStart);
        return rc;
      }
    }
    /// <summary>
    /// Evaluate point at the end of the curve.
    /// </summary>
    public Point3d PointAtEnd
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_Curve_PointAt(ptr, 1, ref rc, idxPointAtEnd);
        return rc;
      }
    }

    /// <summary>
    /// Get a point at a certain length along the curve. The length must be 
    /// non-negative and less than or equal to the length of the curve. 
    /// Lengths will not be wrapped when the curve is closed or periodic.
    /// </summary>
    /// <param name="length">Length along the curve between the start point and the returned point.</param>
    /// <returns>Point on the curve at the specified length from the start point or Poin3d.Unset on failure.</returns>
    public Point3d PointAtLength(double length)
    {
      double t;
      if (!LengthParameter(length, out t)) { return Point3d.Unset; }
      return PointAt(t);
    }
    /// <summary>
    /// Get a point at a certain normalized length along the curve. The length must be 
    /// between or including 0.0 and 1.0, where 0.0 equals the start of the curve and 
    /// 1.0 equals the end of the curve. 
    /// </summary>
    /// <param name="length">Normalized length along the curve between the start point and the returned point.</param>
    /// <returns>Point on the curve at the specified normalized length from the start point or Poin3d.Unset on failure.</returns>
    public Point3d PointAtNormalizedLength(double length)
    {
      double t;
      if (!NormalizedLengthParameter(length, out t)) { return Point3d.Unset; }
      return PointAt(t);
    }

    /// <summary>Force the curve to start at a specified point. 
    /// Not all curve types support this operation.</summary>
    /// <param name="point">New start point of curve.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>Some start points cannot be moved. Be sure to check return code.</remarks>
    public bool SetStartPoint(Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Curve_SetPoint(ptr, point, true);
    }
    /// <summary>Force the curve to end at a specified point. 
    /// Not all curve types support this operation.</summary>
    /// <param name="point">New end point of curve.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>Some end points cannot be moved. Be sure to check return code</remarks>
    public bool SetEndPoint(Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Curve_SetPoint(ptr, point, false);
    }

    /// <summary>Evaluate unit tangent vector at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>Unit tangent vector of the curve at the parameter t.</returns>
    /// <remarks>No error handling.</remarks>
    public Vector3d TangentAt(double t)
    {
      Vector3d rc = new Vector3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Curve_GetVector(ptr, idxTangentAt, t, ref rc);
      return rc;
    }
    /// <summary>Evaluate unit tangent vector at the start of the curve.</summary>
    /// <returns>Unit tangent vector of the curve at the start point.</returns>
    /// <remarks>No error handling.</remarks>
    public Vector3d TangentAtStart
    {
      get { return TangentAt(Domain.Min); }
    }
    /// <summary>Evaluate unit tangent vector at the end of the curve.</summary>
    /// <returns>Unit tangent vector of the curve at the end point.</returns>
    /// <remarks>No error handling.</remarks>
    public Vector3d TangentAtEnd
    {
      get { return TangentAt(Domain.Max); }
    }

    const int idxDerivativeAt = 0;
    const int idxTangentAt = 1;
    const int idxCurvatureAt = 2;

    /// <summary>Return a 3d frame at a parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <param name="plane">The frame is returned here.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool FrameAt(double t, out Plane plane)
    {
      plane = Plane.WorldXY;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_FrameAt(ptr, t, ref plane);
    }
    /// <summary>Evaluate the first derivative at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>First derivative of the curve at the parameter t.</returns>
    /// <remarks>No error handling.</remarks>
    [Obsolete("This method will be removed in a future release, please use the alternative DerivativeAt overloads")]
    public Vector3d DerivativeAt(double t)
    {
      Vector3d rc = new Vector3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Curve_GetVector(ptr, idxDerivativeAt, t, ref rc);
      return rc;
    }
    /// <summary>
    /// Evaluate the derivatives at the specified curve parameter.
    /// </summary>
    /// <param name="t">Curve parameter to evaluate.</param>
    /// <param name="derivativeCount">Number of derivatives to evaluate, must be at least 0.</param>
    /// <returns>An array of vectors that represents all the derivatives starting at zero.</returns>
    public Vector3d[] DerivativeAt(double t, int derivativeCount)
    {
      return DerivativeAt(t, derivativeCount, CurveEvaluationSide.Default);
    }
    /// <summary>
    /// Evaluate the derivatives at the specified curve parameter.
    /// </summary>
    /// <param name="t">Curve parameter to evaluate.</param>
    /// <param name="derivativeCount">Number of derivatives to evaluate, must be at least 0.</param>
    /// <param name="side">Side of parameter to evaluate. If the parameter is at a kink, 
    /// it makes a big difference whether the evaluation is from below or above.</param>
    /// <returns>An array of vectors that represents all the derivatives starting at zero.</returns>
    public Vector3d[] DerivativeAt(double t, int derivativeCount, CurveEvaluationSide side)
    {
      if (derivativeCount < 0) { throw new InvalidOperationException("The derivativeCount must be larger than or equal to zero"); }

      Vector3d[] rc = null;
      SimpleArrayPoint3d points = new SimpleArrayPoint3d();
      IntPtr pPoints = points.NonConstPointer();
      if (UnsafeNativeMethods.ON_Curve_Evaluate(ConstPointer(), derivativeCount, (int)side, t, pPoints))
      {
        Point3d[] pts = points.ToArray();
        rc = new Vector3d[pts.Length];
        for (int i = 0; i < pts.Length; i++)
        {
          rc[i] = new Vector3d(pts[i]);
        }
      }
      points.Dispose();
      return rc;
    }
    /// <summary>Evaluate the curvature vector at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>Curvature vector of the curve at the parameter t.</returns>
    /// <remarks>No error handling.</remarks>
    public Vector3d CurvatureAt(double t)
    {
      Vector3d rc = new Vector3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Curve_GetVector(ptr, idxCurvatureAt, t, ref rc);
      return rc;
    }

    /// <summary>
    /// Get a collection of perpendicular frames along the curve. Perpendicular frames 
    /// are also known as 'Zero-twisting frames' and they minimize rotation from one frame to the next.
    /// </summary>
    /// <param name="parameters">A collection of <i>strictly increasing</i> curve parameters to place perpendicular frames on.</param>
    /// <returns>An array of perpendicular frames on success or null on failure.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the curve parameters are not increasing.</exception>
    public Plane[] GetPerpendicularFrames(IEnumerable<double> parameters)
    {
      if (null == parameters)
        return null;

      RhinoList<double> ts = new RhinoList<double>();
      double t0 = double.MinValue;

      foreach (double t in parameters)
      {
        if (t <= t0)
        {
          throw new InvalidOperationException("Curve parameters must be strictly increasing");
        }
        ts.Add(t);
        t0 = t;
      }
      // looks like we need at least two parameters to have this function make sense
      if (ts.Count < 2)
        return null;

      double[] _parameters = ts.ToArray();
      int count = _parameters.Length;
      Plane[] frames = new Plane[count];

      IntPtr pConstCurve = ConstPointer();
      int rc_count = UnsafeNativeMethods.RHC_RhinoGet1RailFrames(pConstCurve, count, _parameters, frames);
      if (rc_count == count)
        return frames;

      if (rc_count > 0)
      {
        Plane[] rc = new Plane[rc_count];
        Array.Copy(frames, rc, rc_count);
        return rc;
      }

      return null;
    }

    /// <summary>
    /// Test continuity at a curve parameter value.
    /// </summary>
    /// <param name="continuityType">Type of continuity to test for.</param>
    /// <param name="t">Parameter to test.</param>
    /// <returns>
    /// True if the curve has at least the c type continuity at the parameter t.
    /// </returns>
    public bool IsContinuous(Continuity continuityType, double t)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsContinuous(ptr, (int)continuityType, t);
    }
    /// <summary>
    /// Search for a derivative, tangent, or curvature discontinuity.
    /// </summary>
    /// <param name="continuityType">Type of continuity to search for.</param>
    /// <param name="t0">
    /// Search begins at t0. If there is a discontinuity at t0, it will be ignored. This makes it
    /// possible to repeatedly call GetNextDiscontinuity() and step through the discontinuities.
    /// </param>
    /// <param name="t1">
    /// (t0 != t1)  If there is a discontinuity at t1 it will be ignored unless continuityType is
    /// a locus discontinuity type and t1 is at the start or end of the curve.
    /// </param>
    /// <param name="t">If a discontinuity is found, then t reports the parameter at the discontinuity.</param>
    /// <returns>
    /// Parametric continuity tests c = (C0_continuous, ..., G2_continuous):
    ///  true if a parametric discontinuity was found strictly between t0 and t1. Note well that
    ///  all curves are parametrically continuous at the ends of their domains.
    /// 
    /// Locus continuity tests c = (C0_locus_continuous, ...,G2_locus_continuous):
    ///  true if a locus discontinuity was found strictly between t0 and t1 or at t1 is the at the end
    ///  of a curve. Note well that all open curves (IsClosed()=false) are locus discontinuous at the
    ///  ends of their domains.  All closed curves (IsClosed()=true) are at least C0_locus_continuous at 
    ///  the ends of their domains.
    /// </returns>
    public bool GetNextDiscontinuity(Continuity continuityType, double t0, double t1, out double t)
    {
      t = RhinoMath.UnsetValue;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_GetNextDiscontinuity(ptr, (int)continuityType, t0, t1, ref t);
    }
    #endregion

    #region size related methods
    /// <summary>
    /// Get the length of the curve with a fractional tolerance of 1.0e-8
    /// </summary>
    /// <returns>The length of the curve on success, or zero on failure.</returns>
    public double GetLength()
    {
      // default tolerance used in OpenNURBS
      return GetLength(1.0e-8);
    }
    /// <summary>Get the length of the curve.</summary>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision. 
    /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
    /// </param>
    /// <returns>The length of the curve on success, or zero on failure.</returns>
    public double GetLength(double fractionalTolerance)
    {
      double length = 0.0;
      Interval sub_domain = Interval.Unset;
      IntPtr ptr = ConstPointer();
      if (UnsafeNativeMethods.ON_Curve_GetLength(ptr, ref length, fractionalTolerance, sub_domain, true))
        return length;
      return 0;
    }
    /// <summary>Get the length of a sub-section of the curve with a fractional tolerance of 1e-8.</summary>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
    /// </param>
    /// <returns>The length of the sub-curve on success, or zero on failure.</returns>
    public double GetLength(Interval subdomain)
    {
      // default tolerance used in OpenNURBS
      return GetLength(1.0e-8, subdomain);
    }
    /// <summary>Get the length of a sub-section of the curve.</summary>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision. 
    /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve (must be non-decreasing).
    /// </param>
    /// <returns>The length of the sub-curve on success, or zero on failure.</returns>
    public double GetLength(double fractionalTolerance, Interval subdomain)
    {
      double length = 0.0;
      IntPtr ptr = ConstPointer();
      if (UnsafeNativeMethods.ON_Curve_GetLength(ptr, ref length, fractionalTolerance, subdomain, false))
        return length;
      return 0;
    }

    /// <summary>Used to quickly find short curves.</summary>
    /// <param name="tolerance">Length threshold value for "shortness".</param>
    /// <returns>True if the length of the curve is &lt;= tolerance.</returns>
    /// <remarks>Faster than calling Length() and testing the result.</remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public bool IsShort(double tolerance)
    {
      Interval subdomain = Interval.Unset;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsShort(ptr, tolerance, subdomain, true);
    }
    /// <summary>Used to quickly find short curves.</summary>
    /// <param name="tolerance">Length threshold value for "shortness".</param>
    /// <param name="subdomain">
    /// The test is performed on the interval that is the intersection of subdomain with Domain()
    /// </param>
    /// <returns>True if the length of the curve is &lt;= tolerance.</returns>
    /// <remarks>Faster than calling Length() and testing the result.</remarks>
    public bool IsShort(double tolerance, Interval subdomain)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_IsShort(ptr, tolerance, subdomain, false);
    }

    /// <summary>
    /// Looks for segments that are shorter than tolerance that can be removed. 
    /// Does not change the domain, but it will change the relative parameterization.
    /// </summary>
    /// <param name="tolerance">Tolerance which defines "short" segments.</param>
    /// <returns>
    /// True if removable short segments were found. 
    /// False if no removable short segments were found.
    /// </returns>
    public bool RemoveShortSegments(double tolerance)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Curve_RemoveShortSegments(ptr, tolerance);
    }

    /// <summary>
    /// Get the parameter along the curve which coincides with a given length along the curve. 
    /// A fractional tolerance of 1e-8 is used in this version of the function.
    /// </summary>
    /// <param name="segmentLength">
    /// Length of segment to measure. Must be less than or equal to the length of the curve.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from the curve start point to t equals length.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool LengthParameter(double segmentLength, out double t)
    {
      return LengthParameter(segmentLength, out t, 1.0e-8);
    }
    /// <summary>
    /// Get the parameter along the curve which coincides with a given length along the curve.
    /// </summary>
    /// <param name="segmentLength">
    /// Length of segment to measure. Must be less than or equal to the length of the curve.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from the curve start point to t equals s.
    /// </param>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision.
    /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool LengthParameter(double segmentLength, out double t, double fractionalTolerance)
    {
      t = 0.0;

      double length = GetLength(fractionalTolerance);
      if (segmentLength > length) { return false; }
      if (length == 0.0) { return false; }

      segmentLength /= length;

      return NormalizedLengthParameter(segmentLength, out t, fractionalTolerance);
    }
    /// <summary>
    /// Get the parameter along the curve which coincides with a given length along the curve. 
    /// A fractional tolerance of 1e-8 is used in this version of the function
    /// </summary>
    /// <param name="segmentLength">
    /// Length of segment to measure. Must be less than or equal to the length of the subdomain.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from the start of the subdomain to t is s.
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve rather than the whole curve.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool LengthParameter(double segmentLength, out double t, Interval subdomain)
    {
      return LengthParameter(segmentLength, out t, 1.0e-8, subdomain);
    }
    /// <summary>
    /// Get the parameter along the curve which coincides with a given length along the curve.
    /// </summary>
    /// <param name="segmentLength">
    /// Length of segment to measure. Must be less than or equal to the length of the subdomain.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from the start of the subdomain to t is s
    /// </param>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision. 
    /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve rather than the whole curve.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool LengthParameter(double segmentLength, out double t, double fractionalTolerance, Interval subdomain)
    {
      t = 0.0;

      double length = GetLength(fractionalTolerance);
      if (segmentLength > length) { return false; }
      if (length == 0.0) { return false; }

      segmentLength /= length;

      return NormalizedLengthParameter(segmentLength, out t, fractionalTolerance, subdomain);
    }

    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
    /// A fractional tolerance of 1e-8 is used in this version of the function.
    /// </summary>
    /// <param name="s">
    /// Normalized arc length parameter. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from its start to t is arc_length
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool NormalizedLengthParameter(double s, out double t)
    {
      return NormalizedLengthParameter(s, out t, 1.0e-8);
    }
    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    /// </summary>
    /// <param name="s">
    /// Normalized arc length parameter. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from its start to t is arc_length.
    /// </param>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision. 
    /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool NormalizedLengthParameter(double s, out double t, double fractionalTolerance)
    {
      t = 0.0;
      Interval subdomain = Interval.Unset;
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Curve_GetNormalizedArcLengthPoint(ptr, s, ref t, fractionalTolerance, subdomain, true);
      return rc;
    }
    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
    /// A fractional tolerance of 1e-8 is used in this version of the function.
    /// </summary>
    /// <param name="s">
    /// Normalized arc length parameter. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from its start to t is arc_length.
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool NormalizedLengthParameter(double s, out double t, Interval subdomain)
    {
      return NormalizedLengthParameter(s, out t, 1.0e-8, subdomain);
    }
    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    /// </summary>
    /// <param name="s">
    /// Normalized arc length parameter. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="t">
    /// Parameter such that the length of the curve from its start to t is arc_length.
    /// </param>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision. 
    /// fabs(("exact" length from start to t) - arc_length)/arc_length &lt;= fractionalTolerance.
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve.
    /// </param>
    /// <returns>True on success, false on failure.</returns>
    public bool NormalizedLengthParameter(double s, out double t, double fractionalTolerance, Interval subdomain)
    {
      t = 0.0;
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Curve_GetNormalizedArcLengthPoint(ptr, s, ref t, fractionalTolerance, subdomain, false);
      return rc;
    }

    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
    /// A fractional tolerance of 1e-8 is used in this version of the function.
    /// </summary>
    /// <param name="s">
    /// Array of normalized arc length parameters. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="absoluteTolerance">
    /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
    /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
    /// </param>
    /// <returns>
    /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
    /// Null on failure.
    /// </returns>
    public double[] NormalizedLengthParameters(double[] s, double absoluteTolerance)
    {
      return NormalizedLengthParameters(s, absoluteTolerance, 1.0e-8);
    }
    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    /// </summary>
    /// <param name="s">
    /// Array of normalized arc length parameters. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="absoluteTolerance">
    /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
    /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
    /// </param>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision for each segment. 
    /// fabs("true" length - actual length)/(actual length) &lt;= fractionalTolerance.
    /// </param>
    /// <returns>
    /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
    /// Null on failure.
    /// </returns>
    public double[] NormalizedLengthParameters(double[] s, double absoluteTolerance, double fractionalTolerance)
    {
      int count = s.Length;
      double[] t = new double[count];
      Interval sub_domain = Interval.Unset;
      IntPtr ptr = ConstPointer();
      if (UnsafeNativeMethods.ON_Curve_GetNormalizedArcLengthPoints(ptr, count, s, t, absoluteTolerance, fractionalTolerance, sub_domain, true))
        return t;
      return null;
    }
    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve. 
    /// A fractional tolerance of 1e-8 is used in this version of the function.
    /// </summary>
    /// <param name="s">
    /// Array of normalized arc length parameters. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="absoluteTolerance">
    /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
    /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve. 
    /// A 0.0 s value corresponds to subdomain->Min() and a 1.0 s value corresponds to subdomain->Max().
    /// </param>
    /// <returns>
    /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
    /// Null on failure.
    /// </returns>
    public double[] NormalizedLengthParameters(double[] s, double absoluteTolerance, Interval subdomain)
    {
      return NormalizedLengthParameters(s, absoluteTolerance, 1.0e-8, subdomain);
    }
    /// <summary>
    /// Input the parameter of the point on the curve that is a prescribed arc length from the start of the curve.
    /// </summary>
    /// <param name="s">
    /// Array of normalized arc length parameters. 
    /// E.g., 0 = start of curve, 1/2 = midpoint of curve, 1 = end of curve.
    /// </param>
    /// <param name="absoluteTolerance">
    /// If absoluteTolerance > 0, then the difference between (s[i+1]-s[i])*curve_length 
    /// and the length of the curve segment from t[i] to t[i+1] will be &lt;= absoluteTolerance.
    /// </param>
    /// <param name="fractionalTolerance">
    /// Desired fractional precision for each segment. 
    /// fabs("true" length - actual length)/(actual length) &lt;= fractionalTolerance.
    /// </param>
    /// <param name="subdomain">
    /// The calculation is performed on the specified sub-domain of the curve. 
    /// A 0.0 s value corresponds to subdomain->Min() and a 1.0 s value corresponds to subdomain->Max().
    /// </param>
    /// <returns>
    /// If successful, array of curve parameters such that the length of the curve from its start to t[i] is s[i]*curve_length. 
    /// Null on failure.
    /// </returns>
    public double[] NormalizedLengthParameters(double[] s, double absoluteTolerance, double fractionalTolerance, Interval subdomain)
    {
      int count = s.Length;
      double[] t = new double[count];
      IntPtr ptr = ConstPointer();
      if (UnsafeNativeMethods.ON_Curve_GetNormalizedArcLengthPoints(ptr, count, s, t, absoluteTolerance, fractionalTolerance, subdomain, false))
        return t;
      return null;
    }

    /// <summary>
    /// Divide the curve into a number of equal-length segments.
    /// </summary>
    /// <param name="segmentCount">Segment count. Note that the number of division points may differ from the segment count.</param>
    /// <param name="includeEnds">If true, then the points at the start and end of the curve are included.</param>
    /// <returns>
    /// List of curve parameters at the division points on success, null on failure.
    /// </returns>
    public double[] DivideByCount(int segmentCount, bool includeEnds)
    {
      if (segmentCount < 1)
        return null;

      int tcount = segmentCount - 1;

      if (IsClosed && includeEnds)
        tcount = segmentCount;
      else if (includeEnds)
        tcount = segmentCount + 1;

      double[] rc = new double[tcount];
      IntPtr curve_ptr = ConstPointer();
      bool success = UnsafeNativeMethods.RHC_RhinoDivideCurve1(curve_ptr, segmentCount, includeEnds, tcount, rc);
      if (!success)
        return null;
      return rc;
    }
    /// <summary>
    /// Divide the curve into a number of equal-length segments.
    /// </summary>
    /// <param name="segmentCount">Segment count. Note that the number of division points may differ from the segment count.</param>
    /// <param name="includeEnds">If true, then the points at the start and end of the curve are included.</param>
    /// <param name="points">A list of division points. If the function returns successfully, this point-array will be filled in.</param>
    /// <returns>Array containing division curve parameters on success, null on failure.</returns>
    public double[] DivideByCount(int segmentCount, bool includeEnds, out Point3d[] points)
    {
      points = null;

      if (segmentCount < 1)
        return null;

      int tcount = segmentCount - 1;

      if (IsClosed && includeEnds)
        tcount = segmentCount;
      else if (includeEnds)
        tcount = segmentCount + 1;

      double[] rc = new double[tcount];
      IntPtr curve_ptr = ConstPointer();

      SimpleArrayPoint3d outputPoints = new SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      bool success = UnsafeNativeMethods.RHC_RhinoDivideCurve2(curve_ptr, segmentCount, includeEnds, tcount, outputPointsPtr, ref rc[0]);

      if (success)
      {
        points = outputPoints.ToArray();
      }

      outputPoints.Dispose();

      if (!success)
        return null;
      return rc;
    }
    /// <summary>
    /// Divide the curve into specific length segments.
    /// </summary>
    /// <param name="segmentLength">The length of each and every segment (except potentially the last one).</param>
    /// <param name="includeStart">If true, then the points at the start of the curve is included.</param>
    /// <returns>Array containing division curve parameters if successful, null on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public double[] DivideByLength(double segmentLength, bool includeStart)
    {
      IntPtr pConstThis = ConstPointer();
      SimpleArrayDouble outputParams = new SimpleArrayDouble();
      IntPtr outputParamsPtr = outputParams.NonConstPointer();

      bool success = UnsafeNativeMethods.RHC_RhinoDivideCurve3(pConstThis, segmentLength, includeStart, outputParamsPtr);

      double[] rc = null;
      if (success)
      {
        rc = outputParams.ToArray();
      }
      outputParams.Dispose();

      if (!success)
        return null;
      return rc;
    }

    /// <summary>
    /// Divide the curve into specific length segments.
    /// </summary>
    /// <param name="segmentLength">The length of each and every segment (except potentially the last one).</param>
    /// <param name="includeStart">If true, then the point at the start of the curve is included.</param>
    /// <param name="points">If function is successful, points at each parameter value are returned in points</param>
    /// <returns>Array containing division curve parameters if successful, null on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dividebylength.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dividebylength.cs' lang='cs'/>
    /// <code source='examples\py\ex_dividebylength.py' lang='py'/>
    /// </example>
    public double[] DivideByLength(double segmentLength, bool includeStart, out Point3d[] points)
    {
      points = null;
      double[] rc = DivideByLength(segmentLength, includeStart);
      if (rc != null && rc.Length > 0)
      {
        IntPtr pConstThis = ConstPointer();
        points = new Point3d[rc.Length];
        for (int i = 0; i < rc.Length; i++)
        {
          double t = rc[i];
          Point3d pt = new Point3d();
          UnsafeNativeMethods.ON_Curve_PointAt(pConstThis, t, ref pt, idxPointAtT);
          points[i] = pt;
        }
      }
      return rc;
    }

    /// <summary>
    /// Calculates 3d points on a curve where the linear distance between the points is equal
    /// </summary>
    /// <param name="distance">The distance betwen division points</param>
    /// <returns></returns>
    public Point3d[] DivideEquidistant(double distance)
    {
      Point3d[] rc = null;
      SimpleArrayPoint3d points = new SimpleArrayPoint3d();
      IntPtr pConstThis = ConstPointer();
      IntPtr pPoints = points.NonConstPointer();
      if (UnsafeNativeMethods.RHC_RhinoDivideCurveEquidistant(pConstThis, distance, pPoints) > 0)
        rc = points.ToArray();
      points.Dispose();
      return rc;
    }

    /// <summary>
    /// Contour divide the curve by defining a contour line
    /// </summary>
    /// <param name="contourStart"></param>
    /// <param name="contourEnd"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public Point3d[] DivideAsContour(Point3d contourStart, Point3d contourEnd, double interval)
    {
      Point3d[] rc = null;
      using (SimpleArrayPoint3d points = new SimpleArrayPoint3d())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pPoints = points.NonConstPointer();
        if (UnsafeNativeMethods.RHC_MakeRhinoContours1(pConstThis, contourStart, contourEnd, interval, pPoints))
          rc = points.ToArray();
      }
      return rc;
    }

    //David: Do we really need these two functions? Me thinks they are a bit too geeky.
    /// <summary>
    /// Convert a NURBS curve parameter to a curve parameter.
    /// </summary>
    /// <param name="nurbsParameter">Nurbs form parameter.</param>
    /// <param name="curveParameter">Curve parameter</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>
    /// If HasNurbForm returns 2, this function converts the curve parameter to the NURBS curve parameter.
    /// </remarks>
    public bool GetCurveParameterFromNurbsFormParameter(double nurbsParameter, out double curveParameter)
    {
      curveParameter = 0.0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_GetNurbParameter(ptr, nurbsParameter, ref curveParameter, true);
    }
    /// <summary>Convert a curve parameter to a NURBS curve parameter</summary>
    /// <param name="curveParameter">Curve parameter</param>
    /// <param name="nurbsParameter">Nurbs form parameter</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>
    /// If GetNurbForm returns 2, this function converts the curve parameter to the NURBS curve parameter.
    /// </remarks>
    public bool GetNurbsFormParameterFromCurveParameter(double curveParameter, out double nurbsParameter)
    {
      nurbsParameter = 0.0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_GetNurbParameter(ptr, curveParameter, ref nurbsParameter, false);
    }
    #endregion

    #region shape related methods
    private Curve TrimExtendHelper(double t0, double t1, bool trimming)
    {
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Curve_TrimExtend(ptr, t0, t1, trimming);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Removes portions of the curve outside the specified interval.
    /// </summary>
    /// <param name="t0">
    /// Start of the trimming interval. Portions of the curve before curve(t0) are removed.
    /// </param>
    /// <param name="t1">
    /// End of the trimming interval. Portions of the curve after curve(t1) are removed.
    /// </param>
    /// <returns>Trimmed portion of this curve is successfull, null on failure.</returns>
    public Curve Trim(double t0, double t1)
    {
      return TrimExtendHelper(t0, t1, true);
    }
    /// <summary>
    /// Removes portions of the curve outside the specified interval.
    /// </summary>
    /// <param name="domain">
    /// Trimming interval. Portions of the curve before curve(domain[0])
    /// and after curve(domain[1]) are removed.
    /// </param>
    /// <returns>Trimmed portion of this curve is successfull, null on failure.</returns>
    public Curve Trim(Interval domain)
    {
      return Trim(domain.T0, domain.T1);
    }

    /// <summary>
    /// Splits (divides) the curve at the specified parameter. 
    /// The parameter must be in the interior of the curve's domain.
    /// </summary>
    /// <param name="t">
    /// Parameter to split the curve at in the interval returned by Domain().
    /// </param>
    /// <returns>
    /// Two curves on success, null on failure.
    /// </returns>
    public Curve[] Split(double t)
    {
      IntPtr leftptr = IntPtr.Zero;
      IntPtr rightptr = IntPtr.Zero;
      IntPtr pConstThis = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Curve_Split(pConstThis, t, ref leftptr, ref rightptr);
      Curve[] output = new Curve[2];
      if (leftptr != IntPtr.Zero)
        output[0] = GeometryBase.CreateGeometryHelper(leftptr, null) as Curve;
      if (rightptr != IntPtr.Zero)
        output[1] = GeometryBase.CreateGeometryHelper(rightptr, null) as Curve;
      if (rc)
        return output;
      return null;
    }

    /// <summary>
    /// Splits (divides) the curve at a series of specified parameters. 
    /// The parameter must be in the interior of the curve's domain.
    /// </summary>
    /// <param name="t">
    /// Parameters to split the curve at in the interval returned by Domain().
    /// </param>
    /// <returns>
    /// Multiple curves on success, null on failure.
    /// </returns>
    public Curve[] Split(IEnumerable<double> t)
    {
      Interval domain = this.Domain;
      RhinoList<double> parameters = new RhinoList<double>(t);
      parameters.Add(domain.Min);
      parameters.Add(domain.Max);
      parameters.Sort();
      RhinoList<Curve> rc = new RhinoList<Curve>();
      for (int i = 0; i < parameters.Count - 1; i++)
      {
        double start = parameters[i];
        double end = parameters[i + 1];
        if ((start - end) > RhinoMath.ZeroTolerance)
        {
          Curve trimcurve = this.Trim(start, end);
          if (trimcurve != null)
            rc.Add(trimcurve);
        }
      }
      if (rc.Count == 0)
        return null;
      return rc.ToArray();
    }

    /// <summary>
    /// Where possible, analytically extends curve to include the given domain. 
    /// This will not work on closed curves. The original curve will be identical to the 
    /// restriction of the resulting curve to the original curve domain.
    /// </summary>
    /// <param name="t0">Start of extension domain, if the start is not inside the 
    /// Domain of this curve, an attempt will be made to extend the curve.</param>
    /// <param name="t1">End of extension domain, if the end is not inside the 
    /// Domain of this curve, an attempt will be made to extend the curve.</param>
    /// <returns>Extended curve on success, null on failure.</returns>
    public Curve Extend(double t0, double t1)
    {
      return TrimExtendHelper(t0, t1, false);
    }
    /// <summary>
    /// Where possible, analytically extends curve to include the given domain. 
    /// This will not work on closed curves. The original curve will be identical to the 
    /// restriction of the resulting curve to the original curve domain.
    /// </summary>
    /// <param name="domain">Extension domain.</param>
    /// <returns>Extended curve on success, null on failure.</returns>
    public Curve Extend(Interval domain)
    {
      return Extend(domain.T0, domain.T1);
    }
    /// <summary>
    /// Extend a curve by a specific length.
    /// </summary>
    /// <param name="side">Curve end to extend.</param>
    /// <param name="length">Length to add to the curve end.</param>
    /// <param name="style">Extension style.</param>
    /// <returns>A curve with extended ends or null on failure.</returns>
    public Curve Extend(CurveEnd side, double length, CurveExtensionStyle style)
    {
      if (side == CurveEnd.None)
        return DuplicateCurve();

      length = Math.Max(length, 0.0);

      double l0 = length;
      double l1 = length;

      if (side == CurveEnd.End)
        l0 = 0.0;
      if (side == CurveEnd.Start)
        l1 = 0.0;

      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoExtendCurve(ptr, l0, l1, (int)style);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    const int idxExtendTypeLine = 0;
    const int idxExtendTypeArc = 1;
    const int idxExtendTypeSmooth = 2;

    /// <summary>
    /// Extend a curve until it intersects a collection of objects
    /// </summary>
    /// <param name="side">The end of the curve to extend.</param>
    /// <param name="style"></param>
    /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
    /// <returns>New extended curve result on success, null on failure.</returns>
    public Curve Extend(CurveEnd side, CurveExtensionStyle style, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
    {
      if (CurveEnd.None == side)
        return null;
      int _side = 0;
      if (CurveEnd.End == side)
        _side = 1;
      else if (CurveEnd.Both == side)
        _side = 2;

      IntPtr pConstPtr = ConstPointer();
      Runtime.INTERNAL_GeometryArray geometryArray = new Runtime.INTERNAL_GeometryArray(geometry);

      IntPtr geometryArrayPtr = geometryArray.ConstPointer();

      int extendStyle = idxExtendTypeLine;
      if (style == CurveExtensionStyle.Arc)
        extendStyle = idxExtendTypeArc;
      else if (style == CurveExtensionStyle.Smooth)
        extendStyle = idxExtendTypeSmooth;

      IntPtr rc = UnsafeNativeMethods.RHC_RhinoExtendCurve1(pConstPtr, extendStyle, _side, geometryArrayPtr);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Extend a curve to a point
    /// </summary>
    /// <param name="side">The end of the curve to extend.</param>
    /// <param name="style"></param>
    /// <param name="endPoint"></param>
    /// <returns>New extended curve result on success, null on failure.</returns>
    public Curve Extend(CurveEnd side, CurveExtensionStyle style, Point3d endPoint)
    {
      if (CurveEnd.None == side)
        return null;
      int _side = 0;
      if (CurveEnd.End == side)
        _side = 1;
      else if (CurveEnd.Both == side)
        _side = 2;

      IntPtr pConstPtr = ConstPointer();

      int extendStyle = idxExtendTypeLine;
      if (style == CurveExtensionStyle.Arc)
        extendStyle = idxExtendTypeArc;
      else if (style == CurveExtensionStyle.Smooth)
        extendStyle = idxExtendTypeSmooth;

      IntPtr rc = UnsafeNativeMethods.RHC_RhinoExtendCurve2(pConstPtr, extendStyle, _side, endPoint);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Extend a curve by a line until it intersects a collection of objects.
    /// </summary>
    /// <param name="side">The end of the curve to extend.</param>
    /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
    /// <returns>New extended curve result on success, null on failure.</returns>
    public Curve ExtendByLine(CurveEnd side, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
    {
      return Extend(side, CurveExtensionStyle.Line, geometry);
    }
    /// <summary>
    /// Extend a curve by an Arc until it intersects a collection of objects.
    /// </summary>
    /// <param name="side">The end of the curve to extend.</param>
    /// <param name="geometry">A collection of objects. Allowable object types are Curve, Surface, Brep.</param>
    /// <returns>New extended curve result on success, null on failure.</returns>
    public Curve ExtendByArc(CurveEnd side, System.Collections.Generic.IEnumerable<GeometryBase> geometry)
    {
      return Extend(side, CurveExtensionStyle.Arc, geometry);
    }

    static int SimplifyOptionsToInt(CurveSimplifyOptions options)
    {
      int none = 63;
      if ((options & CurveSimplifyOptions.SplitAtFullyMultipleKnots) == CurveSimplifyOptions.SplitAtFullyMultipleKnots)
      {
        // remove DontSplitFMK flag
        none -= (1 << 0);
      }
      if ((options & CurveSimplifyOptions.RebuildLines) == CurveSimplifyOptions.RebuildLines)
      {
        //remove DontRebuildLines flag
        none -= (1 << 1);
      }
      if ((options & CurveSimplifyOptions.RebuildArcs) == CurveSimplifyOptions.RebuildArcs)
      {
        //remove DontRebuildArcs flag
        none -= (1 << 2);
      }
      if ((options & CurveSimplifyOptions.RebuildRationals) == CurveSimplifyOptions.RebuildRationals)
      {
        // remove DontRebuildRationals flag
        none -= (1 << 3);
      }
      if ((options & CurveSimplifyOptions.AdjustG1) == CurveSimplifyOptions.AdjustG1)
      {
        // remove DontAdjustG1 flag
        none -= (1 << 4);
      }
      if ((options & CurveSimplifyOptions.Merge) == CurveSimplifyOptions.Merge)
      {
        // remove DontMerge flag
        none -= (1 << 5);
      }
      return none;
    }
    /// <summary>
    /// Returns a geometrically equivalent PolyCurve.
    /// <para>The PolyCurve has the following properties</para>
    /// <para>
    ///	1. All the PolyCurve segments are LineCurve, PolylineCurve, ArcCurve, or NurbsCurve.
    /// </para>
    /// <para>
    ///	2. The Nurbs Curves segments do not have fully multiple interior knots.
    /// </para>
    /// <para>
    ///	3. Rational Nurbs curves do not have constant weights.
    /// </para>
    /// <para>
    ///	4. Any segment for which IsLinear() or IsArc() is true is a Line, 
    ///    Polyline segment, or an Arc.
    /// </para>
    /// <para>
    ///	5. Adjacent Colinear or Cocircular segments are combined.
    /// </para>
    /// <para>
    ///	6. Segments that meet with G1-continuity have there ends tuned up so
    ///    that they meet with G1-continuity to within machine precision.
    /// </para>
    /// </summary>
    /// <param name="options">Simplification options</param>
    /// <param name="distanceTolerance"></param>
    /// <param name="angleToleranceRadians"></param>
    /// <returns>New simplified curve on success, null on failure.</returns>
    public Curve Simplify(CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians)
    {
      IntPtr pConstPtr = ConstPointer();
      int _options = SimplifyOptionsToInt(options);
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoSimplifyCurve(pConstPtr, _options, distanceTolerance, angleToleranceRadians);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Same as SimplifyCurve, but simplifies only the last two segments at "side" end.
    /// </summary>
    /// <param name="end">If CurveEnd.Start the function simplifies the last two start 
    /// side segments, otherwise if CurveEnd.End the last two end side segments are simplified.
    /// </param>
    /// <param name="options">simplification options.</param>
    /// <param name="distanceTolerance"></param>
    /// <param name="angleToleranceRadians"></param>
    /// <returns>New simplified curve on success, null on failure.</returns>
    public Curve SimplifyEnd(CurveEnd end, CurveSimplifyOptions options, double distanceTolerance, double angleToleranceRadians)
    {
      // CurveEnd must be Start or End
      if (end != CurveEnd.Start && end != CurveEnd.End)
        return null;

      int side = 0;//Start
      if (CurveEnd.End == end)
        side = 1; //end
      int _options = SimplifyOptionsToInt(options);
      IntPtr pConstPtr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoSimplifyCurveEnd(pConstPtr, side, _options, distanceTolerance, angleToleranceRadians);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Fairs a curve object. Fair works best on degree 3 (cubic) curves. Attempts to 
    /// remove large curvature variations while limiting the geometry changes to be no 
    /// more than the specified tolerance. 
    /// </summary>
    /// <param name="distanceTolerance">Maximum allowed distance the faired curve is allowed to deviate from the input.</param>
    /// <param name="angleTolerance">(in radians) kinks with angles &lt;= angleTolerance are smoothed out 0.05 is a good default.</param>
    /// <param name="clampStart">The number of (control vertices-1) to preserve at start. 
    /// <para>0 = preserve start point</para>
    /// <para>1 = preserve start point and 1st derivative</para>
    /// <para>2 = preserve start point, 1st and 2nd derivative</para>
    /// </param>
    /// <param name="clampEnd">Same as clampStart</param>
    /// <param name="iterations">The number of iteratoins to use in adjusting the curve.</param>
    /// <returns>Returns new faired Curve on success, null on failure.</returns>
    public Curve Fair(double distanceTolerance, double angleTolerance, int clampStart, int clampEnd, int iterations)
    {
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoFairCurve(ptr, distanceTolerance, angleTolerance, clampStart, clampEnd, iterations);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Fits a new curve through an existing curve.
    /// </summary>
    /// <param name="degree">The degree of the returned Curve. Must be bigger than 1.</param>
    /// <param name="fitTolerance">The fitting tolerance. If fitTolerance is RhinoMath.UnsetValue or &lt;=0.0,
    /// the document absolute tolerance is used.</param>
    /// <param name="angleTolerance">The kink smoothing tolerance in radians.
    /// <para>If angleTolerance is 0.0, all kinks are smoothed</para>
    /// <para>If angleTolerance is &gt;0.0, kinks smaller than angleTolerance are smoothed</para>  
    /// <para>If angleTolerance is RhinoMath.UnsetValue or &lt;0.0, the document angle tolerance is used for the kink smoothing</para>
    /// </param>
    /// <returns>Returns a new fitted Curve if successful, null on failure.</returns>
    public Curve Fit(int degree, double fitTolerance, double angleTolerance)
    {
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoFitCurve(ptr, degree, fitTolerance, angleTolerance);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }

    /// <summary>
    /// Rebuild a curve with a specific point count.
    /// </summary>
    /// <param name="pointCount">Number of control points in the rebuild curve.</param>
    /// <param name="degree">Degree of curve. Valid values are between and including 1 and 11.</param>
    /// <param name="preserveTangents">If true, the end tangents of the input curve will be preserved.</param>
    /// <returns>A Nurbs curve on success or null on failure.</returns>
    public NurbsCurve Rebuild(int pointCount, int degree, bool preserveTangents)
    {
      pointCount = Math.Max(pointCount, 2);
      degree = Math.Max(degree, 1);
      degree = Math.Min(degree, 11);

      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoRebuildCurve(ptr, pointCount, degree, preserveTangents);
      return GeometryBase.CreateGeometryHelper(rc, null) as NurbsCurve;
    }
    #endregion

    //David: we should use an Enum here. This function should also be a Property I think.
    /// <summary>
    /// Does a NURBS curve representation of this curve exist?
    /// </summary>
    /// <returns>
    /// 0   unable to create NURBS representation with desired accuracy.
    /// 1   success - NURBS parameterization matches the curve's to the desired accuracy
    /// 2   success - NURBS point locus matches the curve's and the domain of the NURBS
    ///               curve is correct. However, This curve's parameterization and the
    ///               NURBS curve parameterization may not match. This situation happens
    ///               when getting NURBS representations of curves that have a
    ///               transendental parameterization like circles
    /// </returns>
    public int HasNurbsForm()
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Curve_HasNurbForm(ptr);
    }

    /// <summary>
    /// Create a NURBS curve representation of this curve.
    /// </summary>
    /// <returns>NURBS representation of the curve on success, null on failure.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      double tolerance = 0.0;
      Interval sub_domain = Interval.Unset;
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Curve_NurbsCurve(ptr, tolerance, sub_domain, true);
      return GeometryBase.CreateGeometryHelper(rc, null) as NurbsCurve;
    }
    /// <summary>
    /// Create a NURBS curve representation of this curve.
    /// </summary>
    /// <param name="subdomain">The NURBS representation for this portion of the curve is returned.</param>
    /// <returns>NURBS representation of the curve on success, null on failure.</returns>
    public NurbsCurve ToNurbsCurve(Interval subdomain)
    {
      double tolerance = 0.0;
      IntPtr ptr = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Curve_NurbsCurve(ptr, tolerance, subdomain, false);
      return GeometryBase.CreateGeometryHelper(rc, null) as NurbsCurve;
    }

    /// <summary>
    /// Get a polyline approximation of a curve.
    /// </summary>
    /// <param name="mainSegmentCount">
    /// If mainSegmentCount &lt;= 0, then both subSegmentCount and mainSegmentCount are ignored. 
    /// If mainSegmentCount &gt; 0, then subSegmentCount must be &gt;= 1. In this 
    /// case the nurb will be broken into mainSegmentCount equally spaced 
    /// chords. If needed, each of these chords can be split into as many 
    /// subSegmentCount sub-parts if the subdivision is necessary for the 
    /// mesh to meet the other meshing constraints. In particular, if 
    /// subSegmentCount = 0, then the curve is broken into mainSegmentCount 
    /// pieces and no further testing is performed.</param>
    /// <param name="subSegmentCount"></param>
    /// <param name="maxAngleRadians">
    /// ( 0 to pi ) Maximum angle (in radians) between unit tangents at 
    /// adjacent vertices.</param>
    /// <param name="maxChordLengthRatio">Maximum permitted value of 
    /// (distance chord midpoint to curve) / (length of chord).</param>
    /// <param name="maxAspectRatio">If maxAspectRatio &lt; 1.0, the parameter is ignored. 
    /// If 1 &lt;= maxAspectRatio &lt; sqrt(2), it is treated as if maxAspectRatio = sqrt(2). 
    /// This parameter controls the maximum permitted value of 
    /// (length of longest chord) / (length of shortest chord).</param>
    /// <param name="tolerance">If tolerance = 0, the parameter is ignored. 
    /// This parameter controls the maximum permitted value of the 
    /// distance from the curve to the polyline.</param>
    /// <param name="minEdgeLength"></param>
    /// <param name="maxEdgeLength">If maxEdgeLength = 0, the parameter 
    /// is ignored. This parameter controls the maximum permitted edge length.
    /// </param>
    /// <param name="keepStartPoint">If true the starting point of the curve 
    /// is added to the polyline. If false the starting point of the curve is 
    /// not added to the polyline.</param>
    /// <returns>PolylineCurve on success, null on error.</returns>
    public PolylineCurve ToPolyline(int mainSegmentCount, int subSegmentCount,
                                    double maxAngleRadians, double maxChordLengthRatio, double maxAspectRatio,
                                    double tolerance, double minEdgeLength, double maxEdgeLength, bool keepStartPoint)
    {
      IntPtr ptr = ConstPointer();
      PolylineCurve poly = new PolylineCurve();
      IntPtr polyOut = poly.NonConstPointer();
      Interval curve_domain = Interval.Unset;

      bool rc = UnsafeNativeMethods.RHC_RhinoConvertCurveToPolyline(ptr,
        mainSegmentCount, subSegmentCount, maxAngleRadians,
        maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, polyOut,
        keepStartPoint, curve_domain, true);

      if (!rc)
      {
        poly.Dispose();
        poly = null;
      }

      return poly;
    }
    /// <summary>
    /// Get a polyline approximation of a curve.
    /// </summary>
    /// <param name="mainSegmentCount">
    /// If mainSegmentCount &lt;= 0, then both subSegmentCount and mainSegmentCount are ignored. 
    /// If mainSegmentCount &gt; 0, then subSegmentCount must be &gt;= 1. In this 
    /// case the nurb will be broken into mainSegmentCount equally spaced 
    /// chords. If needed, each of these chords can be split into as many 
    /// subSegmentCount sub-parts if the subdivision is necessary for the 
    /// mesh to meet the other meshing constraints. In particular, if 
    /// subSegmentCount = 0, then the curve is broken into mainSegmentCount 
    /// pieces and no further testing is performed.</param>
    /// <param name="subSegmentCount"></param>
    /// <param name="maxAngleRadians">
    /// ( 0 to pi ) Maximum angle (in radians) between unit tangents at 
    /// adjacent vertices.</param>
    /// <param name="maxChordLengthRatio">Maximum permitted value of 
    /// (distance chord midpoint to curve) / (length of chord).</param>
    /// <param name="maxAspectRatio">If maxAspectRatio &lt; 1.0, the parameter is ignored. 
    /// If 1 &lt;= maxAspectRatio &lt; sqrt(2), it is treated as if maxAspectRatio = sqrt(2). 
    /// This parameter controls the maximum permitted value of 
    /// (length of longest chord) / (length of shortest chord).</param>
    /// <param name="tolerance">If tolerance = 0, the parameter is ignored. 
    /// This parameter controls the maximum permitted value of the 
    /// distance from the curve to the polyline.</param>
    /// <param name="minEdgeLength"></param>
    /// <param name="maxEdgeLength">If maxEdgeLength = 0, the parameter 
    /// is ignored. This parameter controls the maximum permitted edge length.
    /// </param>
    /// <param name="keepStartPoint">If true the starting point of the curve 
    /// is added to the polyline. If false the starting point of the curve is 
    /// not added to the polyline.</param>
    /// <param name="curveDomain">This subdomain of the NURBS curve is approximated.</param>
    /// <returns>PolylineCurve on success, null on error.</returns>
    public PolylineCurve ToPolyline(int mainSegmentCount, int subSegmentCount,
                                    double maxAngleRadians, double maxChordLengthRatio, double maxAspectRatio,
                                    double tolerance, double minEdgeLength, double maxEdgeLength, bool keepStartPoint,
                                    Interval curveDomain)
    {
      IntPtr ptr = ConstPointer();
      PolylineCurve poly = new PolylineCurve();
      IntPtr polyOut = poly.NonConstPointer();

      bool rc = UnsafeNativeMethods.RHC_RhinoConvertCurveToPolyline(ptr,
        mainSegmentCount, subSegmentCount, maxAngleRadians,
        maxChordLengthRatio, maxAspectRatio, tolerance, minEdgeLength, maxEdgeLength, polyOut,
        keepStartPoint, curveDomain, false);

      if (!rc)
      {
        poly.Dispose();
        poly = null;
      }

      return poly;
    }

    /// <summary>
    /// Makes a polyline approximation of the curve and gets the closest point on the mesh for each point on the curve. 
    /// Then it "connects the points" so that you have a polyline on the mesh.
    /// </summary>
    /// <param name="mesh">Mesh to project onto.</param>
    /// <param name="tolerance">Input tolerance (RhinoDoc.ModelAbsoluteTolerance is a good default)</param>
    /// <returns>A polyline curve on success, null on failure.</returns>
    public PolylineCurve PullToMesh(Mesh mesh, double tolerance)
    {
      IntPtr pConstCurve = ConstPointer();
      IntPtr pConstMesh = mesh.ConstPointer();
      IntPtr pPolylineCurve = UnsafeNativeMethods.RHC_RhinoPullCurveToMesh(pConstCurve, pConstMesh, tolerance);
      return GeometryBase.CreateGeometryHelper(pPolylineCurve, null) as PolylineCurve;
    }

    /// <summary>
    /// Offsets a curve. If you have a nice offset, then there will be one entry in 
    /// the array. If the original curve had kinks or the offset curve had self 
    /// intersections, you will get multiple segments in the offset_curves[] array.
    /// </summary>
    /// <param name="normal">The normal to the offset plane.</param>
    /// <param name="origin">A point on offset plane.</param>
    /// <param name="distance">The positive or negative distance to offset.</param>
    /// <param name="tolerance">The offset or fitting tolerance.</param>
    /// <param name="angleTolerance">The angle tolerance (radians).</param>
    /// <param name="cornerStyle">Corner style for offset kinks.</param>
    /// <returns>Offset curves on success, null on failure.</returns>
    [Obsolete("This method is obsolete and will be removed in a future release of RhinoCommon")]
    public Curve[] Offset(Vector3d normal, Point3d origin, double distance, double tolerance, double angleTolerance, CurveOffsetCornerStyle cornerStyle)
    {
      Plane pl = new Plane(origin, normal);
      Point3d directionPoint = pl.PointAt(1, 0);
      Curve[] rc = Offset(directionPoint, normal, distance, tolerance, cornerStyle);
      return rc;
    }
    /// <summary>
    /// Offsets a curve. If you have a nice offset, then there will be one entry in 
    /// the array. If the original curve had kinks or the offset curve had self 
    /// intersections, you will get multiple segments in the offset_curves[] array.
    /// </summary>
    /// <param name="plane">Offset solution plane.</param>
    /// <param name="distance">The positive or negative distance to offset.</param>
    /// <param name="tolerance">The offset or fitting tolerance.</param>
    /// <param name="cornerStyle">Corner style for offset kinks.</param>
    /// <returns>Offset curves on success, null on failure.</returns>
    public Curve[] Offset(Plane plane, double distance, double tolerance, CurveOffsetCornerStyle cornerStyle)
    {
      IntPtr ptr = ConstPointer();
      SimpleArrayCurvePointer offsetCurves = new SimpleArrayCurvePointer();
      IntPtr pCurveArray = offsetCurves.NonConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhinoOffsetCurve(ptr, plane.ZAxis, plane.Origin, distance, pCurveArray, tolerance, 0.015, (int)cornerStyle);
      Curve[] curves = offsetCurves.ToNonConstArray();
      offsetCurves.Dispose();
      if (!rc)
        return null;
      return curves;
    }
    /// <summary>
    /// Offsets a curve. If you have a nice offset, then there will be one entry in 
    /// the array. If the original curve had kinks or the offset curve had self 
    /// intersections, you will get multiple segments in the offset_curves[] array.
    /// </summary>
    /// <param name="directionPoint">A point that indicates the direction of the offset.</param>
    /// <param name="normal">The normal to the offset plane.</param>
    /// <param name="distance">The positive or negative distance to offset.</param>
    /// <param name="tolerance">The offset or fitting tolerance.</param>
    /// <param name="cornerStyle">Corner style for offset kinks.</param>
    /// <returns>Offset curves on success, null on failure.</returns>
    public Curve[] Offset(Point3d directionPoint, Vector3d normal, double distance, double tolerance, CurveOffsetCornerStyle cornerStyle)
    {
      IntPtr ptr = ConstPointer();
      SimpleArrayCurvePointer offsetCurves = new SimpleArrayCurvePointer();
      IntPtr pCurveArray = offsetCurves.NonConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhinoOffsetCurve2(ptr, distance, directionPoint, normal, (int)cornerStyle, tolerance, pCurveArray);
      Curve[] curves = offsetCurves.ToNonConstArray();
      offsetCurves.Dispose();
      if (!rc)
        return null;
      return curves;
    }

    /// <summary>
    /// Offset a curve on a surface. This curve must lie on the surface.
    /// </summary>
    /// <param name="face"></param>
    /// <param name="distance">distance to offset (+)left, (-)right</param>
    /// <param name="fittingTolerance"></param>
    /// <returns></returns>
    public Curve[] OffsetOnSurface(BrepFace face, double distance, double fittingTolerance)
    {
      int fid = face.m_index;
      IntPtr pConstBrep = face.m_brep.ConstPointer();
      SimpleArrayCurvePointer offsetCurves = new SimpleArrayCurvePointer();
      IntPtr pCurveArray = offsetCurves.NonConstPointer();
      IntPtr pConstCurve = ConstPointer();
      int count = UnsafeNativeMethods.RHC_RhinoOffsetCurveOnSrf(pConstCurve, pConstBrep, fid, distance, fittingTolerance, pCurveArray);
      Curve[] curves = offsetCurves.ToNonConstArray();
      offsetCurves.Dispose();
      if (count < 1)
        return null;
      return curves;
    }
    /// <summary>
    /// Offset a curve on a surface. This curve must lie on the surface.
    /// </summary>
    /// <param name="face"></param>
    /// <param name="throughPoint">2d point on the brep face to offset through</param>
    /// <param name="fittingTolerance"></param>
    /// <returns></returns>
    public Curve[] OffsetOnSurface(BrepFace face, Point2d throughPoint, double fittingTolerance)
    {
      int fid = face.m_index;
      IntPtr pConstBrep = face.m_brep.ConstPointer();
      SimpleArrayCurvePointer offsetCurves = new SimpleArrayCurvePointer();
      IntPtr pCurveArray = offsetCurves.NonConstPointer();
      IntPtr pConstCurve = ConstPointer();
      int count = UnsafeNativeMethods.RHC_RhinoOffsetCurveOnSrf2(pConstCurve, pConstBrep, fid, throughPoint, fittingTolerance, pCurveArray);
      Curve[] curves = offsetCurves.ToNonConstArray();
      offsetCurves.Dispose();
      if (count < 1)
        return null;
      return curves;
    }
    /// <summary>
    /// Offset a curve on a surface. This curve must lie on the surface.
    /// </summary>
    /// <param name="face"></param>
    /// <param name="curveParameters">curve parameters corresponding to the offset distances</param>
    /// <param name="offsetDistances">distances to offset (+)left, (-)right</param>
    /// <param name="fittingTolerance"></param>
    /// <returns></returns>
    public Curve[] OffsetOnSurface(BrepFace face, double[] curveParameters, double[] offsetDistances, double fittingTolerance)
    {
      int array_count = curveParameters.Length;
      if (offsetDistances.Length != array_count)
        throw new ArgumentException("curveParameters and offsetDistances must be the same length");

      int fid = face.m_index;
      IntPtr pConstBrep = face.m_brep.ConstPointer();
      SimpleArrayCurvePointer offsetCurves = new SimpleArrayCurvePointer();
      IntPtr pCurveArray = offsetCurves.NonConstPointer();
      IntPtr pConstCurve = ConstPointer();
      int count = UnsafeNativeMethods.RHC_RhinoOffsetCurveOnSrf3(pConstCurve, pConstBrep, fid, array_count, curveParameters, offsetDistances, fittingTolerance, pCurveArray);
      Curve[] curves = offsetCurves.ToNonConstArray();
      offsetCurves.Dispose();
      if (count < 1)
        return null;
      return curves;
    }
    /// <summary>
    /// Offset a curve on a surface. This curve must lie on the surface.
    /// </summary>
    /// <param name="surface"></param>
    /// <param name="distance">distance to offset (+)left, (-)right</param>
    /// <param name="fittingTolerance"></param>
    /// <returns></returns>
    public Curve[] OffsetOnSurface(Surface surface, double distance, double fittingTolerance)
    {
      Brep b = Brep.CreateFromSurface(surface);
      return OffsetOnSurface(b.Faces[0], distance, fittingTolerance);
    }
    /// <summary>
    /// Offset a curve on a surface. This curve must lie on the surface.
    /// </summary>
    /// <param name="surface"></param>
    /// <param name="throughPoint">2d point on the brep face to offset through</param>
    /// <param name="fittingTolerance"></param>
    /// <returns></returns>
    public Curve[] OffsetOnSurface(Surface surface, Point2d throughPoint, double fittingTolerance)
    {
      Brep b = Brep.CreateFromSurface(surface);
      return OffsetOnSurface(b.Faces[0], throughPoint, fittingTolerance);
    }
    /// <summary>
    /// Offset a curve on a surface. This curve must lie on the surface.
    /// </summary>
    /// <param name="surface"></param>
    /// <param name="curveParameters">curve parameters corresponding to the offset distances</param>
    /// <param name="offsetDistances">distances to offset (+)left, (-)right</param>
    /// <param name="fittingTolerance"></param>
    /// <returns></returns>
    public Curve[] OffsetOnSurface(Surface surface, double[] curveParameters, double[] offsetDistances, double fittingTolerance)
    {
      Brep b = Brep.CreateFromSurface(surface);
      return OffsetOnSurface(b.Faces[0], curveParameters, offsetDistances, fittingTolerance);
    }
    #endregion methods
  }
}


// static bool ON_ForceMatchCurveEnds( OnCurve^% Crv0, int end0, OnCurve^% Crv1, int end1 );
//static bool ON_SortCurves( array<IOnCurve^>^ curve_list,
//                          [System::Runtime::InteropServices::Out]array<int>^% index,
//                          [System::Runtime::InteropServices::Out]array<bool>^% bReverse);
//static OnCurve^ RhinoFairCurve(MArgsRhinoFair^% args);
//static bool RhinoMakeCurveEndsMeet(OnCurve^ pCrv0, int end0, OnCurve^ pCrv1, int end1);
//static bool RhinoRemoveShortSegments(OnCurve^ curve, double tolerance);
//static bool RhinoProjectToPlane(OnNurbsCurve^% curve, OnPlane^% plane);
//static bool RhinoGetLineExtremes(IOnCurve^ curve, OnLine^% line);
//static OnNurbsCurve^ RhinoInterpCurve(int degree, IArrayOn3dPoint^ Pt, IOn3dVector^ start_tan, IOn3dVector^ end_tan, int knot_style, OnNurbsCurve^ nurbs_curve);
//static bool RhinoDoCurveDirectionsMatch(IOnCurve^ c0, IOnCurve^ c1);
//static bool RhinoExtendCurve(OnCurve^% crv, IRhinoExtend::Type type, int side, double length);
//static bool RhinoExtendCurve(OnCurve^% crv, IRhinoExtend::Type type, int side, array<IOnGeometry^>^ geom);
//static bool RhinoExtendCurve(OnCurve^% crv, IRhinoExtend::Type type, int side, IOn3dPoint^ end);
//static bool RhinoSimplifyCurve(OnCurve^% crv, int flags, double dist_tol, double angle_tol);
//static bool RhinoSimplifyCurveEnd(OnCurve^% pC, int side, int flags, double dist_tol, double angle_tol);
//static bool RhinoRepairCurve(OnCurve^ pCurve, double repair_tolerance, int dim);
//static int RhinoPlanarClosedCurveContainmentTest(IOnCurve^ closed_curveA, IOnCurve^ closed_curveB, IOnPlane^ plane, double tolerance);
//static bool RhinoPlanarCurveCollisionTest(IOnCurve^ curveA, IOnCurve^ curveB, IOnPlane^ plane, double tolerance);
//static int RhinoPointInPlanarClosedCurve(IOn3dPoint^ point, IOnCurve^ closed_curve, IOnPlane^ plane, double tolerance);
//static bool RhinoDivideCurve(IOnCurve^ curve, double seg_count, double seg_length, bool bReverse, bool bIncludeEnd, ArrayOn3dPoint^ curve_P, Arraydouble^ curve_t);
//static bool RhinoGetOverlapDistance(IOnCurve^ crv_a, IOnInterval^ dom_a, IOnCurve^ crv_b, IOnInterval^ dom_b, double tol, double lim,
//  [System::Runtime::InteropServices::Out]int% cnt,
//  [System::Runtime::InteropServices::Out]array<double,2>^% int_a,
//  [System::Runtime::InteropServices::Out]array<double,2>^% int_b,
//  [System::Runtime::InteropServices::Out]double% max_a,
//  [System::Runtime::InteropServices::Out]double% max_b,
//  [System::Runtime::InteropServices::Out]double% max_d,
//  [System::Runtime::InteropServices::Out]double% min_a,
//  [System::Runtime::InteropServices::Out]double% min_b,
//  [System::Runtime::InteropServices::Out]double% min_d);
//static int RhinoMakeCubicBeziers( IOnCurve^ Curve, [System::Runtime::InteropServices::Out]array<OnBezierCurve^>^% BezArray, double dist_tol, double kink_tol);
//static int Rhino_dup_cmp_curve( IOnCurve^ crva, IOnCurve^ crvb );
//static OnNurbsCurve^ RhinoFitCurve( IOnCurve^ curve_in, int degree, double dFitTol, double dAngleTol );
//      static ArrayOn3dPoint^ RhinoDivideCurveEquidistant( IOnCurve^ curve, double distance );