namespace SafetyPortal.Api.Attributes
{
    /// <summary>
    /// Controls whether a property is included as a column in the generated document (e.g. Excel, PDF table).
    /// <para>
    /// Applied by <c>ExcelOrCsvCreator.CreateDataTable&lt;T&gt;</c> during reflection-based DataTable building.
    /// A property is included when this attribute is absent or <see cref="Include"/> is <c>true</c>.
    /// A property is excluded when <see cref="Include"/> is <c>false</c>.
    /// </para>
    /// <example>
    /// <code>
    /// [IncludeInDocument(false)]   // excluded from document output
    /// public string InternalField { get; set; }
    ///
    /// [IncludeInDocument(true)]    // explicitly included (same as no attribute)
    /// public string VisibleField { get; set; }
    ///
    /// public string DefaultField { get; set; }  // included — no attribute means include
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IncludeInDocumentAttribute(bool include = true) : Attribute
    {
        /// <summary>
        /// Whether the property should appear as a column in the document output.
        /// Defaults to <c>true</c>.
        /// </summary>
        public bool Include { get; } = include;
    }
}
