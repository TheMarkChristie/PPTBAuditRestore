namespace AuditRestore
{
    /// <summary>
    /// Base64-encoded PNG icons for the XrmToolBox tool tile (32×32 small, 80×80 big).
    ///
    /// TODO: replace these placeholders with proper PNG renders of icon.svg
    /// (the red restore-arrow + white database mark). XrmToolBox rejects reuse of its own
    /// icon, so these must be the tool's own artwork. The 1×1 transparent placeholders below
    /// keep the build valid until the real PNGs are dropped in.
    /// </summary>
    internal static class IconData
    {
        // 1×1 transparent PNG (placeholder).
        public const string Small =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

        public const string Big =
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";
    }
}
