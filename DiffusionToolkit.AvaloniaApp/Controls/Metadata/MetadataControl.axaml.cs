using Avalonia;
using Avalonia.Controls;

namespace DiffusionToolkit.AvaloniaApp.Controls.Metadata;

public partial class MetadataControl : UserControl
{
    public static readonly DirectProperty<MetadataControl, MetadataViewModel?> MetadataProperty =
        AvaloniaProperty.RegisterDirect<MetadataControl, MetadataViewModel?>(
            nameof(Metadata),
            (o) => o.Metadata,
            (o, v) =>
            {
                o.Metadata = v;
            });

    private MetadataViewModel? _metadata;

    public MetadataViewModel? Metadata
    {
        get => _metadata;
        set => this.SetAndRaise(MetadataProperty, ref _metadata, value);
    }

    public MetadataControl()
    {
        InitializeComponent();
    }
}