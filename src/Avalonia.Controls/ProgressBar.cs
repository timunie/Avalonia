using System;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Automation.Peers;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Reactive;

namespace Avalonia.Controls
{
    /// <summary>
    /// A control used to indicate the progress of an operation.
    /// </summary>
    [TemplatePart("PART_Indicator", typeof(Border))]
    [PseudoClasses(":vertical", ":horizontal", ":indeterminate")]
    public class ProgressBar : RangeBase
    {
        /// <summary>
        /// Provides calculated values for use with the <see cref="ProgressBar"/>'s control theme or template.
        /// </summary>
        /// <remarks>
        /// This class is NOT intended for general use outside of control templates.
        /// </remarks>
        public class ProgressBarTemplateSettings : AvaloniaObject
        {
            private double _container2Width;
            private double _containerWidth;
            private double _containerAnimationStartPosition;
            private double _containerAnimationEndPosition;
            private double _container2AnimationStartPosition;
            private double _container2AnimationEndPosition;

            public static readonly DirectProperty<ProgressBarTemplateSettings, double> ContainerAnimationStartPositionProperty =
           AvaloniaProperty.RegisterDirect<ProgressBarTemplateSettings, double>(
               nameof(ContainerAnimationStartPosition),
               p => p.ContainerAnimationStartPosition,
               (p, o) => p.ContainerAnimationStartPosition = o, 0d);

            public static readonly DirectProperty<ProgressBarTemplateSettings, double> ContainerAnimationEndPositionProperty =
                AvaloniaProperty.RegisterDirect<ProgressBarTemplateSettings, double>(
                    nameof(ContainerAnimationEndPosition),
                    p => p.ContainerAnimationEndPosition,
                    (p, o) => p.ContainerAnimationEndPosition = o, 0d);

            public static readonly DirectProperty<ProgressBarTemplateSettings, double> Container2AnimationStartPositionProperty =
                AvaloniaProperty.RegisterDirect<ProgressBarTemplateSettings, double>(
                    nameof(Container2AnimationStartPosition),
                    p => p.Container2AnimationStartPosition,
                    (p, o) => p.Container2AnimationStartPosition = o, 0d);

            public static readonly DirectProperty<ProgressBarTemplateSettings, double> Container2AnimationEndPositionProperty =
                AvaloniaProperty.RegisterDirect<ProgressBarTemplateSettings, double>(
                    nameof(Container2AnimationEndPosition),
                    p => p.Container2AnimationEndPosition,
                    (p, o) => p.Container2AnimationEndPosition = o);

            public static readonly DirectProperty<ProgressBarTemplateSettings, double> Container2WidthProperty =
                AvaloniaProperty.RegisterDirect<ProgressBarTemplateSettings, double>(
                    nameof(Container2Width),
                    p => p.Container2Width,
                    (p, o) => p.Container2Width = o);

            public static readonly DirectProperty<ProgressBarTemplateSettings, double> ContainerWidthProperty =
                AvaloniaProperty.RegisterDirect<ProgressBarTemplateSettings, double>(
                    nameof(ContainerWidth),
                    p => p.ContainerWidth,
                    (p, o) => p.ContainerWidth = o);

            public double ContainerAnimationStartPosition
            {
                get => _containerAnimationStartPosition;
                set => SetAndRaise(ContainerAnimationStartPositionProperty, ref _containerAnimationStartPosition, value);
            }

            public double ContainerAnimationEndPosition
            {
                get => _containerAnimationEndPosition;
                set => SetAndRaise(ContainerAnimationEndPositionProperty, ref _containerAnimationEndPosition, value);
            }

            public double Container2AnimationStartPosition
            {
                get => _container2AnimationStartPosition;
                set => SetAndRaise(Container2AnimationStartPositionProperty, ref _container2AnimationStartPosition, value);
            }

            public double Container2Width
            {
                get => _container2Width;
                set => SetAndRaise(Container2WidthProperty, ref _container2Width, value);
            }

            public double ContainerWidth
            {
                get => _containerWidth;
                set => SetAndRaise(ContainerWidthProperty, ref _containerWidth, value);
            }

            public double Container2AnimationEndPosition
            {
                get => _container2AnimationEndPosition;
                set => SetAndRaise(Container2AnimationEndPositionProperty, ref _container2AnimationEndPosition, value);
            }
        }

        private double _percentage;
        private Border? _indicator;
        private IDisposable? _trackSizeChangedListener;

        /// <summary>
        /// Defines the <see cref="IsIndeterminate"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsIndeterminateProperty =
            AvaloniaProperty.Register<ProgressBar, bool>(nameof(IsIndeterminate));

        /// <summary>
        /// Defines the <see cref="ShowProgressText"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowProgressTextProperty =
            AvaloniaProperty.Register<ProgressBar, bool>(nameof(ShowProgressText));

        /// <summary>
        /// Defines the <see cref="ProgressTextFormat"/> property.
        /// </summary>
        public static readonly StyledProperty<string> ProgressTextFormatProperty =
            AvaloniaProperty.Register<ProgressBar, string>(nameof(ProgressTextFormat), "{1:0}%");

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<ProgressBar, Orientation>(nameof(Orientation), Orientation.Horizontal);

        /// <summary>
        /// Defines the <see cref="Percentage"/> property.
        /// </summary>
        public static readonly DirectProperty<ProgressBar, double> PercentageProperty =
            AvaloniaProperty.RegisterDirect<ProgressBar, double>(
                nameof(Percentage),
                o => o.Percentage);

        /// <summary>
        /// Defines the <see cref="IndeterminateStartingOffset"/> property.
        /// </summary>
        public static readonly StyledProperty<double> IndeterminateStartingOffsetProperty =
            AvaloniaProperty.Register<ProgressBar, double>(nameof(IndeterminateStartingOffset));

        /// <summary>
        /// Defines the <see cref="IndeterminateEndingOffset"/> property.
        /// </summary>
        public static readonly StyledProperty<double> IndeterminateEndingOffsetProperty =
            AvaloniaProperty.Register<ProgressBar, double>(nameof(IndeterminateEndingOffset));

        /// <summary>
        /// Gets the overall percentage complete of the progress 
        /// </summary>
        /// <remarks>
        /// This read-only property is automatically calculated using the current <see cref="RangeBase.Value"/> and
        /// the effective range (<see cref="RangeBase.Maximum"/> - <see cref="RangeBase.Minimum"/>).
        /// </remarks>
        public double Percentage
        {
            get { return _percentage; }
            private set { SetAndRaise(PercentageProperty, ref _percentage, value); }
        }

        public double IndeterminateStartingOffset
        {
            get => GetValue(IndeterminateStartingOffsetProperty);
            set => SetValue(IndeterminateStartingOffsetProperty, value);
        }

        public double IndeterminateEndingOffset
        {
            get => GetValue(IndeterminateEndingOffsetProperty);
            set => SetValue(IndeterminateEndingOffsetProperty, value);
        }

        static ProgressBar()
        {
            ValueProperty.OverrideMetadata<ProgressBar>(new(defaultBindingMode: BindingMode.OneWay));
            ValueProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            MinimumProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            MaximumProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            IsIndeterminateProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
            OrientationProperty.Changed.AddClassHandler<ProgressBar>((x, e) => x.UpdateIndicatorWhenPropChanged(e));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressBar"/> class.
        /// </summary>
        public ProgressBar()
        {
            UpdatePseudoClasses(IsIndeterminate, Orientation);
        }

        /// <summary>
        /// Gets or sets the TemplateSettings for the <see cref="ProgressBar"/>.
        /// </summary>
        public ProgressBarTemplateSettings TemplateSettings { get; } = new ProgressBarTemplateSettings();

        /// <summary>
        /// Gets or sets a value indicating whether the progress bar shows the actual value or a generic,
        /// continues progress indicator (indeterminate state).
        /// </summary>
        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether progress text will be shown.
        /// </summary>
        public bool ShowProgressText
        {
            get => GetValue(ShowProgressTextProperty);
            set => SetValue(ShowProgressTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the format string applied to the internally calculated progress text before it is shown.
        /// </summary>
        public string ProgressTextFormat
        {
            get => GetValue(ProgressTextFormatProperty);
            set => SetValue(ProgressTextFormatProperty, value);
        }

        /// <summary>
        /// Gets or sets the orientation of the <see cref="ProgressBar"/>.
        /// </summary>
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateIndicator();
            return result;
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == IsIndeterminateProperty)
            {
                UpdatePseudoClasses(change.GetNewValue<bool>(), null);
            }
            else if (change.Property == OrientationProperty)
            {
                UpdatePseudoClasses(null, change.GetNewValue<Orientation>());
            }
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            // dispose any previous track size listener
            _trackSizeChangedListener?.Dispose();

            _indicator = e.NameScope.Get<Border>("PART_Indicator");

            // listen to size changes of the indicators track (parent) and update the indicator there. 
            _trackSizeChangedListener = _indicator.Parent?.GetPropertyChangedObservable(BoundsProperty)
                .Subscribe(_ => UpdateIndicator());

            UpdateIndicator();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ProgressBarAutomationPeer(this);
        }

        private void UpdateIndicator()
        {
            // Gets the size of the parent indicator container
            var barSize = _indicator?.VisualParent?.Bounds.Size ?? Bounds.Size;

            if (_indicator != null)
            {
                if (IsIndeterminate)
                {
                    // Pulled from ModernWPF.

                    var dim = Orientation == Orientation.Horizontal ? barSize.Width : barSize.Height;
                    var barIndicatorWidth = dim * 0.4; // Indicator width at 40% of ProgressBar
                    var barIndicatorWidth2 = dim * 0.6; // Indicator width at 60% of ProgressBar

                    TemplateSettings.ContainerWidth = barIndicatorWidth;
                    TemplateSettings.Container2Width = barIndicatorWidth2;

                    TemplateSettings.ContainerAnimationStartPosition = barIndicatorWidth * -1.8; // Position at -180%
                    TemplateSettings.ContainerAnimationEndPosition = barIndicatorWidth * 3.0; // Position at 300%

                    TemplateSettings.Container2AnimationStartPosition = barIndicatorWidth2 * -1.5; // Position at -150%
                    TemplateSettings.Container2AnimationEndPosition = barIndicatorWidth2 * 1.66; // Position at 166%

                    // Remove these properties when we switch to fluent as default and removed the old one.
                    SetCurrentValue(IndeterminateStartingOffsetProperty,-dim);
                    SetCurrentValue(IndeterminateEndingOffsetProperty,dim);

                    var padding = Padding;
                    var rectangle = new RectangleGeometry(
                        new Rect(
                            padding.Left,
                            padding.Top,
                            barSize.Width - (padding.Right + padding.Left),
                            barSize.Height - (padding.Bottom + padding.Top)
                            ));
                }
                else
                {
                    double percent = Maximum == Minimum ? 1.0 : (Value - Minimum) / (Maximum - Minimum);

                    // When the Orientation changed, the indicator's Width or Height should set to double.NaN.
                    // Indicator size calculation should consider the ProgressBar's Padding property setting
                    if (Orientation == Orientation.Horizontal)
                    {
                        _indicator.Width = (barSize.Width - _indicator.Margin.Left - _indicator.Margin.Right) * percent;
                        _indicator.Height = double.NaN;
                    }
                    else
                    {
                        _indicator.Width = double.NaN;
                        _indicator.Height = (barSize.Height - _indicator.Margin.Top - _indicator.Margin.Bottom) * percent;
                    }


                    Percentage = percent * 100;
                }
            }
        }

        private void UpdateIndicatorWhenPropChanged(AvaloniaPropertyChangedEventArgs e)
        {
            UpdateIndicator();
        }

        private void UpdatePseudoClasses(
            bool? isIndeterminate,
            Orientation? o)
        {
            if (isIndeterminate.HasValue)
            {
                PseudoClasses.Set(":indeterminate", isIndeterminate.Value);
            }

            if (o.HasValue)
            {
                PseudoClasses.Set(":vertical", o == Orientation.Vertical);
                PseudoClasses.Set(":horizontal", o == Orientation.Horizontal);
            }
        }
    }
}
