namespace BuildingRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using NetTopologySuite.Geometries;

    /// <summary>
    /// Moves a geometry between the two reference systems this registry supports, Lambert 72 (EPSG 31370)
    /// and Lambert 2008 (EPSG 3812). The single place that decides how that transformation is done, so a
    /// geometry the migrator transformed and the same geometry normalized on the way in by
    /// <c>GmlGeometryNormalizer</c> come out the same. See ADR 0006.
    /// </summary>
    public static class GeometryReferenceSystem
    {
        /// <summary>
        /// Building unit positions are rounded to centimetres, buildings geometries are not.
        /// </summary>
        /// <remarks>
        /// A unit position is a single point, and the BackOffice already persists it at that precision:
        /// <c>GeometryExtensions.ConvertToGml(false)</c>, which <c>GmlGeometryNormalizer</c> re-serializes
        /// through, writes a point with 2 decimals (ADR 0003). Rounding here keeps a transformed position
        /// identical to the same position normalized on the way in, and drops the transform noise below the
        /// precision anyone reads it at.
        ///
        /// A building geometry is an outline or a GRB measurement whose vertices carry far more decimals
        /// than a centimetre; rounding those would move the boundary rather than tidy it, so the
        /// transformation leaves them alone.
        /// </remarks>
        public const int PositionRoundingPrecision = 2;

        public static bool IsSupported(int srid)
            => srid is SystemReferenceId.SridLambert72 or SystemReferenceId.SridLambert2008;

        /// <summary>Puts a geometry in <paramref name="srid"/>, transforming it when it is in the other system.</summary>
        public static Geometry ToReferenceSystem(this Geometry geometry, int srid)
            => ToReferenceSystem(geometry, srid, null);

        /// <summary>
        /// <see cref="ToReferenceSystem(Geometry,int)"/>, rounding the result's coordinates to
        /// <paramref name="roundingPrecision"/> decimals.
        /// </summary>
        public static Geometry ToReferenceSystem(this Geometry geometry, int srid, int roundingPrecision)
            => ToReferenceSystem(geometry, srid, (int?)roundingPrecision);

        /// <remarks>
        /// The explicit transform rather than <c>EnsureLambert08</c> / <c>EnsureLambert72</c>: those relabel
        /// whatever falls outside their envelope instead of transforming it, which for something that is
        /// about to be persisted would mean coordinates ~500 km from where the building is. See ADR 0006.
        ///
        /// Which system the geometry is in is read from its SRID, exactly as <c>GmlGeometryNormalizer</c>
        /// does (ADR 0003). Every geometry reaching this point has been read either from GML carrying its own
        /// <c>srsName</c> or from persisted EWKB through <see cref="WKBReaderFactory.CreateForEwkb"/>, which
        /// falls back to Lambert 72 for the SRID-less bytes written before the event store wrote EWKB — so
        /// the label is not a guess. Parcel-registry decides this from the coordinates instead, because its
        /// GRB reader labels every polygon 31370 by construction; building-registry has no such path.
        /// </remarks>
        private static Geometry ToReferenceSystem(Geometry geometry, int srid, int? roundingPrecision)
        {
            if (!IsSupported(srid))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(srid), srid, "Only Lambert 72 (31370) and Lambert 2008 (3812) are supported.");
            }

            if (!IsSupported(geometry.SRID))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(geometry), geometry.SRID, "Geometry is not in Lambert 72 (31370) or Lambert 2008 (3812).");
            }

            if (geometry.SRID == srid)
            {
                // RoundCoordinates mutates in place, so it never gets the caller's geometry.
                return roundingPrecision.HasValue
                    ? geometry.Copy().RoundCoordinates(roundingPrecision.Value)
                    : geometry;
            }

            if (srid == SystemReferenceId.SridLambert2008)
            {
                return roundingPrecision.HasValue
                    ? geometry.TransformFromLambert72To08(roundingPrecision.Value)
                    : geometry.TransformFromLambert72To08();
            }

            var transformed = geometry.TransformFromLambert08To72();

            // No rounding overload for this direction in LambertTransformation; the transform already
            // returned a new geometry, so rounding it in place is safe.
            return roundingPrecision.HasValue
                ? transformed.RoundCoordinates(roundingPrecision.Value)
                : transformed;
        }
    }
}
