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
    /// <c>GmlGeometryNormalizer</c> come out the same. See ADR 0007.
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

        /// <summary>
        /// Whether an SRID <em>label</em> is one of the two this registry supports. Strict: an absent label
        /// is not one of them - see <see cref="ReferenceSystem"/> for reading a geometry's actual system.
        /// </summary>
        public static bool IsSupported(int srid)
            => srid is SystemReferenceId.SridLambert72 or SystemReferenceId.SridLambert2008;

        /// <summary>
        /// The reference system a geometry is in, reading the ones written before the event store wrote
        /// EWKB - which carry no SRID at all - as Lambert 72, which is what they are.
        /// </summary>
        /// <remarks>
        /// A reader does not necessarily label them: <see cref="WKBReaderFactory.CreateForEwkb"/> falls back
        /// to the Lambert 72 reader, whose geometry factory stamps 31370, but a plain
        /// <c>WKBReader</c> on the default geometry services - which is what <c>BuildingGeometry</c> reads
        /// with - leaves the SRID at -1. So the SRID cannot be taken at face value without this.
        /// See ADR 0007.
        /// </remarks>
        public static int ReferenceSystem(this Geometry geometry)
            => geometry.SRID <= 0 ? SystemReferenceId.SridLambert72 : geometry.SRID;

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
        /// about to be persisted would mean coordinates ~500 km from where the building is. See ADR 0007.
        ///
        /// Which system the geometry is in is read from its SRID, exactly as <c>GmlGeometryNormalizer</c>
        /// does (ADR 0003) — through <see cref="ReferenceSystem"/>, so a geometry read without a label is
        /// Lambert 72 rather than an error. Every geometry reaching this point came either from GML carrying
        /// its own <c>srsName</c> or from persisted EWKB, so the system is not a guess. Parcel-registry
        /// decides this from the coordinates instead, because its GRB reader labels every polygon 31370 by
        /// construction; building-registry has no such path.
        /// </remarks>
        private static Geometry ToReferenceSystem(Geometry geometry, int srid, int? roundingPrecision)
        {
            if (!IsSupported(srid))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(srid), srid, "Only Lambert 72 (31370) and Lambert 2008 (3812) are supported.");
            }

            var sourceSrid = geometry.ReferenceSystem();

            if (!IsSupported(sourceSrid))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(geometry), geometry.SRID, "Geometry is not in Lambert 72 (31370) or Lambert 2008 (3812).");
            }

            if (sourceSrid == srid)
            {
                if (geometry.SRID == srid && !roundingPrecision.HasValue)
                {
                    return geometry;
                }

                // A geometry that carries no SRID gets one here: it is already in this reference system, so
                // there is nothing to transform, but putting the missing label on is the other half of what
                // the transformation is for.
                // The copy is also what keeps RoundCoordinates, which mutates in place, off the caller's
                // geometry.
                var relabelled = geometry.Copy();
                relabelled.SRID = srid;

                return roundingPrecision.HasValue
                    ? relabelled.RoundCoordinates(roundingPrecision.Value)
                    : relabelled;
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
