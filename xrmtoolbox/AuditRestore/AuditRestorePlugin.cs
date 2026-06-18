using System.ComponentModel.Composition;
using XrmToolBox.Extensibility;
using XrmToolBox.Extensibility.Interfaces;

namespace AuditRestore
{
    /// <summary>
    /// XrmToolBox plugin registration. The UI lives in <see cref="AuditRestoreControl"/>
    /// (a WebView2 host around the shared audit-restore HTML). XrmToolBox discovers this
    /// via MEF (the [Export] attributes) when the dll is dropped into its Plugins folder.
    /// </summary>
    [Export(typeof(IXrmToolBoxPlugin))]
    [ExportMetadata("Name", "Audit Restore")]
    [ExportMetadata("Description", "Restore Dataverse records to their field state at a point in time using the audit log. Find affected records by table/user/date window, preview before→after changes, and apply in pages.")]
    [ExportMetadata("Author", "Mark Christie")]
    [ExportMetadata("BackgroundColor", "DarkSlateGray")]
    [ExportMetadata("PrimaryFontColor", "White")]
    [ExportMetadata("SecondaryFontColor", "WhiteSmoke")]
    [ExportMetadata("SmallImageBase64", IconData.Small)]
    [ExportMetadata("BigImageBase64", IconData.Big)]
    public class AuditRestorePlugin : PluginBase
    {
        public override IXrmToolBoxPluginControl GetControl() => new AuditRestoreControl();
    }
}
