﻿using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DIPS.Xamarin.UI.Controls.Modality;
using DIPS.Xamarin.UI.Internal.xaml;
using DIPS.Xamarin.UI.Resources.Colors;
using DIPS.Xamarin.UI.Resources.LocalizedStrings;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DIPS.Xamarin.UI.Controls.Sheet
{
    /// <summary>
    ///     A behavior to that can be added to a <see cref="ModalityLayout" /> to display a sheet that animates into the view,
    ///     either from top or bottom, when <see cref="IsOpen" /> is set.
    /// </summary>
    [ContentProperty(nameof(SheetContent))]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class SheetBehavior : Behavior<ModalityLayout>, IModalityHandler
    {
        private bool m_supressNextTranslation;
        private bool m_fromIsOpenContext;
        private ModalityLayout? m_modalityLayout;
        private double? m_originalPosition = null;
        private SheetView? m_sheetView;

        /// <summary>
        ///     <see cref="OnBeforeOpenCommand" />
        /// </summary>
        public static readonly BindableProperty OnBeforeOpenCommandProperty = BindableProperty.Create(
            nameof(OnBeforeOpenCommand),
            typeof(ICommand),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="OnBeforeOpenCommandParameter" />
        /// </summary>
        public static readonly BindableProperty OnBeforeOpenCommandParameterProperty = BindableProperty.Create(
            nameof(OnBeforeOpenCommandParameter),
            typeof(object),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="OnOpenCommand" />
        /// </summary>
        public static readonly BindableProperty OnOpenCommandProperty = BindableProperty.Create(
            nameof(OnOpenCommand),
            typeof(ICommand),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="OnOpenCommandProperty" />
        /// </summary>
        public static readonly BindableProperty OnOpenCommandParameterProperty =
            BindableProperty.Create(nameof(OnOpenCommandParameter), typeof(object), typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="OnBeforeCloseCommand" />
        /// </summary>
        public static readonly BindableProperty OnBeforeCloseCommandProperty = BindableProperty.Create(
            nameof(OnBeforeCloseCommand),
            typeof(ICommand),
            typeof(SheetBehavior));

        /// <summary>
        /// The size of the title label.
        /// This is a bindable property.
        /// </summary>
        /// <remarks>Default value is <see cref="NamedSize.Medium"/></remarks>
        [TypeConverter(typeof(FontSizeConverter))]
        public double TitleSize
        {
            get => (double)GetValue(TitleSizeProperty);
            set => SetValue(TitleSizeProperty, value);
        }

        /// <summary>
        /// <see cref="TitleSize"/>
        /// </summary>
        public static readonly BindableProperty TitleSizeProperty = BindableProperty.Create(nameof(TitleSize), typeof(double), typeof(SheetBehavior), defaultValueCreator:b => Device.GetNamedSize(NamedSize.Medium, typeof(Label)));


        /// <summary>
        /// Title font attributes.
        /// This is a bindable property.
        /// </summary>
        /// <remarks>Default is bold</remarks>
        public FontAttributes TitleFontAttributes
        {
            get => (FontAttributes)GetValue(TitleFontAttributesProperty);
            set => SetValue(TitleFontAttributesProperty, value);
        }

        /// <summary>
        /// <see cref="TitleFontAttributes"/>
        /// </summary>
        public static readonly BindableProperty TitleFontAttributesProperty = BindableProperty.Create(nameof(TitleFontAttributes), typeof(FontAttributes), typeof(SheetBehavior), defaultValue: FontAttributes.Bold);

        /// <summary>
        /// The size of the label of the cancel button.
        /// This is a bindable property.
        /// </summary>
        /// <remarks>Default value is <see cref="NamedSize.Small"/></remarks>
        [TypeConverter(typeof(FontSizeConverter))]
        public double CancelButtonSize
        {
            get => (double)GetValue(CancelButtonSizeProperty);
            set => SetValue(CancelButtonSizeProperty, value);
        }

        /// <summary>
        /// <see cref="CancelButtonSize"/>
        /// </summary>
        public static readonly BindableProperty CancelButtonSizeProperty = BindableProperty.Create(nameof(CancelButtonSize), typeof(double), typeof(SheetBehavior), defaultValueCreator: b => Device.GetNamedSize(NamedSize.Small, typeof(Button)));

        /// <summary>
        /// The size of the label of the action button.
        /// This is a bindable property.
        /// <remarks>Default value is <see cref="NamedSize.Small"/></remarks>
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        public double ActionButtonSize
        {
            get => (double)GetValue(ActionButtonSizeProperty);
            set => SetValue(ActionButtonSizeProperty, value);
        }

        /// <summary>
        /// <see cref="ActionButtonSize"/>
        /// </summary>
        public static readonly BindableProperty ActionButtonSizeProperty = BindableProperty.Create(nameof(ActionButtonSize), typeof(double), typeof(SheetBehavior), defaultValueCreator: b => Device.GetNamedSize(NamedSize.Small, typeof(Button)));



        /// <summary>
        ///     <see cref="OnBeforeCloseCommandParameter" />
        /// </summary>
        public static readonly BindableProperty OnBeforeCloseCommandParameterProperty = BindableProperty.Create(
            nameof(OnBeforeCloseCommandParameter),
            typeof(object),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="OnCloseCommand" />
        /// </summary>
        public static readonly BindableProperty OnCloseCommandProperty = BindableProperty.Create(
            nameof(OnCloseCommand),
            typeof(ICommand),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="OnCloseCommandParameter" />
        /// </summary>
        public static readonly BindableProperty OnCloseCommandParameterProperty = BindableProperty.Create(
            nameof(OnCloseCommandParameter),
            typeof(object),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="VerticalContentAlignment" />
        /// </summary>
        public static readonly BindableProperty VerticalContentAlignmentProperty = BindableProperty.Create(
            nameof(VerticalContentAlignment),
            typeof(ContentAlignment),
            typeof(SheetBehavior),
            ContentAlignment.Fit,
            propertyChanged: OnVerticalContentAlignmentPropertyChanged);

        /// <summary>
        ///     <see cref="ActionTitle" />
        /// </summary>
        public static readonly BindableProperty ActionTitleProperty = BindableProperty.Create(
            nameof(ActionTitle),
            typeof(string),
            typeof(SheetBehavior),
            string.Empty);

        /// <summary>
        ///     <see cref="CancelTitle" />
        /// </summary>
        public static readonly BindableProperty CancelTitleProperty = BindableProperty.Create(
            nameof(CancelTitle),
            typeof(string),
            typeof(SheetBehavior),
            InternalLocalizedStrings.Cancel);

        /// <summary>
        ///     <see cref="TitleSeparatorColor" />
        /// </summary>
        public static readonly BindableProperty TitleSeparatorColorProperty = BindableProperty.Create(
            nameof(TitleSeparatorColor),
            typeof(Color),
            typeof(SheetBehavior),
            ColorPalette.QuinaryLight);

        /// <summary>
        ///     <see cref="IsTitleSeparatorVisible" />
        /// </summary>
        public static readonly BindableProperty IsTitleSeparatorVisibleProperty = BindableProperty.Create(
            nameof(IsTitleSeparatorVisible),
            typeof(bool),
            typeof(SheetBehavior),
            true);

        /// <summary>
        ///     <see cref="TitleColor" />
        /// </summary>
        public static readonly BindableProperty TitleColorProperty = BindableProperty.Create(
            nameof(TitleColor),
            typeof(Color),
            typeof(SheetBehavior),
            Label.TextColorProperty.DefaultValue);

        /// <summary>
        ///     <see cref="Title" />
        /// </summary>
        public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(SheetBehavior),
            string.Empty);

        /// <summary>
        ///     <see cref="Alignment" />
        /// </summary>
        public static readonly BindableProperty AlignmentProperty = BindableProperty.Create(
            nameof(Alignment),
            typeof(AlignmentOptions),
            typeof(SheetView),
            AlignmentOptions.Bottom);

        /// <summary>
        ///     <see cref="IsOpen" />
        /// </summary>
        public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
            nameof(IsOpen),
            typeof(bool),
            typeof(SheetView),
            false,
            BindingMode.TwoWay,
            propertyChanged: IsOpenPropertyChanged);

        /// <summary>
        ///     <see cref="OnPositionChangedCommand" />
        /// </summary>
        public static readonly BindableProperty OnPositionChangedCommandProperty = BindableProperty.Create(
            nameof(OnPositionChangedCommand),
            typeof(ICommand),
            typeof(SheetBehavior));

        /// <summary>
        /// <see cref="ShouldAutoClose"/>
        /// </summary>
        public static readonly BindableProperty ShouldAutoCloseProperty = BindableProperty.Create(nameof(ShouldAutoClose), typeof(bool), typeof(SheetBehavior), true);

        /// <summary>
        ///     <see cref="ShouldRememberPosition" />
        /// </summary>
        public static readonly BindableProperty ShouldRememberPositionProperty = BindableProperty.Create(
            nameof(ShouldRememberPosition),
            typeof(bool),
            typeof(SheetBehavior),
            false);

        /// <summary>
        ///     <see cref="SheetContent" />
        /// </summary>
        public static readonly BindableProperty SheetContentProperty = BindableProperty.Create(
            nameof(SheetContent),
            typeof(View),
            typeof(SheetView),
            new ContentView { HeightRequest = 100, VerticalOptions = LayoutOptions.Start },
            propertyChanged: OnSheetContentPropertyChanged);




        /// <summary>
        ///     The content template to use when the sheet opens.
        ///     This is a bindable property.
        ///     <remarks><see cref="BindingContextFactory" /> to set the binding context of the datatemplate when the sheet opens</remarks>
        /// </summary>
        public DataTemplate SheetContentTemplate
        {
            get => (DataTemplate)GetValue(SheetContentTemplateProperty);
            set => SetValue(SheetContentTemplateProperty, value);
        }

        /// <summary>
        /// <see cref="SheetContentTemplate"/>
        /// </summary>
        public static readonly BindableProperty SheetContentTemplateProperty = BindableProperty.Create(nameof(SheetContentTemplate), typeof(DataTemplate), typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="BackgroundColor" />
        /// </summary>
        public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
            nameof(BackgroundColor),
            typeof(Color),
            typeof(SheetView),
            Color.White,
            propertyChanged: BackgroundColorPropertyChanged);

        /// <summary>
        ///     <see cref="HeaderColor" />
        /// </summary>
        public static readonly BindableProperty HeaderColorProperty = BindableProperty.Create(
            nameof(HeaderColor),
            typeof(Color),
            typeof(SheetBehavior),
            BackgroundColorProperty.DefaultValue);

        /// <summary>
        ///     <see cref="ContentColor" />
        /// </summary>
        public static readonly BindableProperty ContentColorProperty = BindableProperty.Create(
            nameof(ContentColor),
            typeof(Color),
            typeof(SheetBehavior),
            BackgroundColorProperty.DefaultValue);

        /// <summary>
        ///     <see cref="Position" />
        /// </summary>
        public static readonly BindableProperty PositionProperty = BindableProperty.Create(
            nameof(Position),
            typeof(double),
            typeof(SheetBehavior),
            0.0,
            BindingMode.TwoWay,
            propertyChanged: OnPositionPropertyChanged);

        /// <summary>
        ///     <see cref="MaxPosition" />
        /// </summary>
        public static readonly BindableProperty MaxPositionProperty = BindableProperty.Create(
            nameof(MaxPosition),
            typeof(double),
            typeof(SheetBehavior),
            1.0,
            BindingMode.TwoWay);

        /// <summary>
        ///     <see cref="MinPosition" />
        /// </summary>
        public static readonly BindableProperty MinPositionProperty = BindableProperty.Create(
            nameof(MinPosition),
            typeof(double),
            typeof(SheetBehavior),
            0.05,
            BindingMode.TwoWay);

        /// <summary>
        ///     <see cref="BindingContextFactory" />
        /// </summary>
        public static readonly BindableProperty BindingContextFactoryProperty = BindableProperty.Create(
            nameof(BindingContextFactory),
            typeof(Func<object>),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="IsDraggable" />
        /// </summary>
        public static readonly BindableProperty IsDraggableProperty = BindableProperty.Create(
            nameof(IsDraggable),
            typeof(bool),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="HasShadow" />
        /// </summary>
        public static readonly BindableProperty HasShadowProperty = BindableProperty.Create(
            nameof(HasShadow),
            typeof(bool),
            typeof(SheetBehavior),
            false);

        /// <summary>
        ///     <see cref="HandleColor" />
        /// </summary>
        public static readonly BindableProperty HandleColorProperty = BindableProperty.Create(
            nameof(HandleColor),
            typeof(Color),
            typeof(SheetBehavior),
            ColorPalette.QuinaryAir);

        /// <summary>
        ///     <see cref="CloseOnOverlayTapped" />
        /// </summary>
        public static readonly BindableProperty CloseOnOverlayTappedProperty = BindableProperty.Create(
            nameof(CloseOnOverlayTapped),
            typeof(bool),
            typeof(SheetBehavior),
            true);

        /// <summary>
        ///     <see cref="ActionTitleColor" />
        /// </summary>
        public static readonly BindableProperty ActionTitleColorProperty = BindableProperty.Create(
            nameof(ActionTitleColor),
            typeof(Color),
            typeof(SheetBehavior),
            Theme.TealPrimary);

        /// <summary>
        ///     <see cref="CancelTitleColor" />
        /// </summary>
        public static readonly BindableProperty CancelTitleColorProperty = BindableProperty.Create(
            nameof(CancelTitleColor),
            typeof(Color),
            typeof(SheetBehavior),
            Theme.TealPrimary);

        /// <summary>
        ///     <see cref="IsCancelButtonVisible" />
        /// </summary>
        public static readonly BindableProperty IsCancelButtonVisibleProperty = BindableProperty.Create(
            nameof(IsCancelButtonVisible),
            typeof(bool),
            typeof(SheetBehavior),
            false);

        /// <summary>
        /// <see cref="CancelCommand"/>
        /// </summary>
        public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(
            nameof(CancelCommand),
            typeof(ICommand),
            typeof(SheetBehavior),
            defaultValueCreator: DefaultValueCreator);

        /// <summary>
        ///     <see cref="CancelCommandParameter" />
        /// </summary>
        public static readonly BindableProperty CancelCommandParameterProperty =
            BindableProperty.Create(nameof(CancelCommandParameter), typeof(object), typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="ActionCommand" />
        /// </summary>
        public static readonly BindableProperty ActionCommandProperty = BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(SheetBehavior));

        /// <summary>
        ///     <see cref="ActionCommandParameter" />
        /// </summary>
        public static readonly BindableProperty ActionCommandParameterProperty =
            BindableProperty.Create(nameof(ActionCommandParameter), typeof(object), typeof(SheetBehavior));
        
        internal static readonly BindablePropertyKey OpenCommandPropertyKey = BindableProperty.CreateReadOnly(
            nameof(OpenCommand),
            typeof(ICommand),
            typeof(SheetBehavior),
            default,
            BindingMode.OneWayToSource,
            defaultValueCreator: OpenSheetCommandValueCreator);

        internal static readonly BindableProperty OpenCommandProperty = OpenCommandPropertyKey.BindableProperty;
        static object? OpenSheetCommandValueCreator(BindableObject? b)
        {
            if (b is SheetBehavior sheetBehavior)
            {
                return new Command(() => sheetBehavior.IsOpen = true);
            }

            return null;
        }

        /// <summary>
        ///     Parameter passed to <see cref="ActionCommand" />.
        ///     This is a bindable property.
        /// </summary>
        public object ActionCommandParameter
        {
            get => GetValue(ActionCommandParameterProperty);
            set => SetValue(ActionCommandParameterProperty, value);
        }

        /// <summary>
        ///     Gets or sets the command that is invoked when the action button is activated.
        ///     This is a bindable property.
        /// </summary>
        public ICommand ActionCommand
        {
            get => (ICommand)GetValue(ActionCommandProperty);
            set => SetValue(ActionCommandProperty, value);
        }

        /// <summary>
        ///     Parameter passed to <see cref="CancelCommand" />.
        ///     This is a bindable property.
        /// </summary>
        public object CancelCommandParameter
        {
            get => GetValue(CancelCommandParameterProperty);
            set => SetValue(CancelCommandParameterProperty, value);
        }

        /// <summary>
        ///     Gets or sets the command that is invoked when the cancel button is activated.
        ///     This is a bindable property.
        /// </summary>
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        /// <summary>
        /// Command to open the sheet. This is not meant to be used in your view model, but you can bind to it with for example a button.Command to open the sheet directly in xaml
        /// </summary>
        public ICommand? OpenCommand => (ICommand?)GetValue(OpenCommandProperty);

        /// <summary>
        ///     Determines if the cancel button is visible.
        ///     This is a bindable property.
        /// </summary>
        public bool IsCancelButtonVisible
        {
            get => (bool)GetValue(IsCancelButtonVisibleProperty);
            set => SetValue(IsCancelButtonVisibleProperty, value);
        }

        /// <summary>
        ///     Color of the text in cancel button.
        ///     This is a bindable property.
        /// </summary>
        public Color CancelTitleColor
        {
            get => (Color)GetValue(CancelTitleColorProperty);
            set => SetValue(CancelTitleColorProperty, value);
        }

        /// <summary>
        ///     Color of text in action button.
        ///     This is a bindable property.
        /// </summary>
        public Color ActionTitleColor
        {
            get => (Color)GetValue(ActionTitleColorProperty);
            set => SetValue(ActionTitleColorProperty, value);
        }

        /// <summary>
        ///     Action button text.
        ///     This is a bindable property.
        /// </summary>
        public string ActionTitle
        {
            get => (string)GetValue(ActionTitleProperty);
            set => SetValue(ActionTitleProperty, value);
        }

        /// <summary>
        ///     Cancel button text.
        ///     This is a bindable property.
        /// </summary>
        public string CancelTitle
        {
            get => (string)GetValue(CancelTitleProperty);
            set => SetValue(CancelTitleProperty, value);
        }

        /// <summary>
        ///     Color of the title separator.
        ///     This is a bindable property.
        /// </summary>
        public Color TitleSeparatorColor
        {
            get => (Color)GetValue(TitleSeparatorColorProperty);
            set => SetValue(TitleSeparatorColorProperty, value);
        }

        /// <summary>
        ///     Determines if the separator between the title and the sheet content should be visible.
        ///     This is a bindable property.
        /// </summary>
        public bool IsTitleSeparatorVisible
        {
            get => (bool)GetValue(IsTitleSeparatorVisibleProperty);
            set => SetValue(IsTitleSeparatorVisibleProperty, value);
        }

        /// <summary>
        ///     Color of the text in <see cref="Title" />.
        ///     This is a bindable property.
        /// </summary>
        public Color TitleColor
        {
            get => (Color)GetValue(TitleColorProperty);
            set => SetValue(TitleColorProperty, value);
        }

        /// <summary>
        ///     Title text.
        ///     This is a bindable property.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        ///     A command that executes when the position of the sheet changes.
        ///     The command parameter will be the new positional value, same as <see cref="Position" />.
        ///     This is a bindable property.
        /// </summary>
        public ICommand OnPositionChangedCommand
        {
            get => (ICommand)GetValue(OnPositionChangedCommandProperty);
            set => SetValue(OnPositionChangedCommandProperty, value);
        }

        /// <summary>
        /// Determines if the sheet should auto close at the minimum position
        /// This is a bindable property.
        /// </summary>
        public bool ShouldAutoClose
        {
            get => (bool)GetValue(ShouldAutoCloseProperty);
            set => SetValue(ShouldAutoCloseProperty, value);
        }

        /// <summary>
        ///     Determines if the sheet should remember its position when it is opened and closed.
        ///     This is a bindable property.
        /// </summary>
        public bool ShouldRememberPosition
        {
            get => (bool)GetValue(ShouldRememberPositionProperty);
            set => SetValue(ShouldRememberPositionProperty, value);
        }

        /// <summary>
        ///     Determines the position of the sheet when it appears.
        ///     This is a bindable property.
        /// </summary>
        public AlignmentOptions Alignment
        {
            get => (AlignmentOptions)GetValue(AlignmentProperty);
            set => SetValue(AlignmentProperty, value);
        }

        /// <summary>
        ///     Determines the color of the background of the sheet.
        ///     This is a bindable property.
        /// </summary>
        [Obsolete(
            "Please use HeaderColor and ContentColor to set the background colors of the sheet. This will be removed in future major versions greater than 3.0.0")]
        public Color BackgroundColor
        {
            get => (Color)GetValue(BackgroundColorProperty);
            set => SetValue(BackgroundColorProperty, value);
        }

        /// <summary>
        ///     Determines the background color of the header part of the sheet.
        ///     This is a bindable property.
        /// </summary>
        public Color HeaderColor
        {
            get => (Color)GetValue(HeaderColorProperty);
            set => SetValue(HeaderColorProperty, value);
        }

        /// <summary>
        ///     Determines the background color of the content part of the sheet.
        ///     This is a bindable property.
        /// </summary>
        public Color ContentColor
        {
            get => (Color)GetValue(ContentColorProperty);
            set => SetValue(ContentColorProperty, value);
        }

        /// <summary>
        ///     Used to set the binding context of the content of the sheet when the sheet opens.
        ///     This is a bindable property.
        /// </summary>
        public Func<object> BindingContextFactory
        {
            get => (Func<object>)GetValue(BindingContextFactoryProperty);
            set => SetValue(BindingContextFactoryProperty, value);
        }

        /// <summary>
        ///     Determines the color of the handle in the sheet.
        ///     This is a bindable property.
        /// </summary>
        public Color HandleColor
        {
            get => (Color)GetValue(HandleColorProperty);
            set => SetValue(HandleColorProperty, value);
        }

        /// <summary>
        ///     Determines if the sheet should have shadow.
        ///     This is a bindable property
        /// </summary>
        /// <remarks>This only works for iOS.</remarks>
        public bool HasShadow
        {
            get => (bool)GetValue(HasShadowProperty);
            set => SetValue(HasShadowProperty, value);
        }

        /// <summary>
        ///     Determines if the sheet should be draggable or not.
        ///     This is a bindable property.
        /// </summary>
        public bool IsDraggable
        {
            get => (bool)GetValue(IsDraggableProperty);
            set => SetValue(IsDraggableProperty, value);
        }

        /// <summary>
        ///     Determines if the sheet should be visible or not.
        ///     This is a bindable property.
        /// </summary>
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        /// <summary>
        ///     Command that execute when the sheet is about to start it's animation to open.
        ///     This is a bindable property.
        /// </summary>
        public ICommand OnBeforeOpenCommand
        {
            get => (ICommand)GetValue(OnBeforeOpenCommandProperty);
            set => SetValue(OnBeforeOpenCommandProperty, value);
        }

        /// <summary>
        ///     Parameter to pass to <see cref="OnBeforeOpenCommand" />.
        ///     This is a bindable property.
        /// </summary>
        public object OnBeforeOpenCommandParameter
        {
            get => GetValue(OnBeforeOpenCommandParameterProperty);
            set => SetValue(OnBeforeOpenCommandParameterProperty, value);
        }

        /// <summary>
        ///     Command that executes when the sheet has completed it's animation and is open.
        ///     This is a bindable property.
        /// </summary>
        public ICommand OnOpenCommand
        {
            get => (ICommand)GetValue(OnOpenCommandProperty);
            set => SetValue(OnOpenCommandProperty, value);
        }

        /// <summary>
        ///     The parameter to pass to the <see cref="OnOpenCommand" />.
        ///     This is a bindable property.
        /// </summary>
        public object OnOpenCommandParameter
        {
            get => GetValue(OnOpenCommandParameterProperty);
            set => SetValue(OnOpenCommandParameterProperty, value);
        }

        /// <summary>
        ///     Command that execute before the sheet start it's animation when closing.
        ///     This is a bindable property.
        /// </summary>
        public ICommand OnBeforeCloseCommand
        {
            get => (ICommand)GetValue(OnBeforeCloseCommandProperty);
            set => SetValue(OnBeforeCloseCommandProperty, value);
        }

        /// <summary>
        ///     The parameter to pass to <see cref="OnBeforeCloseCommand" />.
        ///     This is a bindable property.
        /// </summary>
        public object OnBeforeCloseCommandParameter
        {
            get => GetValue(OnBeforeCloseCommandParameterProperty);
            set => SetValue(OnBeforeCloseCommandParameterProperty, value);
        }

        /// <summary>
        ///     Command that executes when the sheet has completed it's animation and is closed.
        ///     This is a bindable property.
        /// </summary>
        public ICommand OnCloseCommand
        {
            get => (ICommand)GetValue(OnCloseCommandProperty);
            set => SetValue(OnCloseCommandProperty, value);
        }

        /// <summary>
        ///     The parameter to pass to the <see cref="OnCloseCommand" />.
        /// </summary>
        public object OnCloseCommandParameter
        {
            get => GetValue(OnCloseCommandParameterProperty);
            set => SetValue(OnCloseCommandParameterProperty, value);
        }

        /// <summary>
        ///     Determines the maximum position of the sheet when it is visible.
        ///     This is a bindable property.
        /// </summary>
        /// <remarks>This will affect the size of the sheet if <see cref="Position" /> is set to 0</remarks>
        /// <remarks>This will affect the people that are dragging the sheet</remarks>
        /// <remarks>The value have to be between 0 and 1.0 (percentage of the screen)</remarks>
        public double MaxPosition
        {
            get => (double)GetValue(MaxPositionProperty);
            set => SetValue(MaxPositionProperty, value);
        }

        /// <summary>
        ///     Determines the minimum position of the sheet.
        ///     This is a bindable property.
        /// </summary>
        /// <remarks>This position is used to determine where the sheet will auto close if <see cref="ShouldAutoClose"/> is set to true</remarks>
        /// <remarks>This position is used to determine where the sheet will snap to when <see cref="ShouldAutoClose"/> is set to false</remarks>
        /// <remarks>This will affect the size of the sheet if <see cref="Position" /> is set to 0</remarks>
        /// <remarks>The value have to be between 0 and 1.0 (percentage of the screen)</remarks>
        public double MinPosition
        {
            get => (double)GetValue(MinPositionProperty);
            set => SetValue(MinPositionProperty, value);
        }

        /// <summary>
        ///     Determines the position of the sheet when it is visible.
        ///     This is a bindable property.
        ///     <remarks>This will affect the size of the sheet if <see cref="Position" /> is set to 0</remarks>
        ///     <remarks>The value have to be between 0 and 1.0 (percentage of the screen)</remarks>
        /// </summary>
        public double Position
        {
            get => (double)GetValue(PositionProperty);
            set => SetValue(PositionProperty, value);
        }

        /// <summary>
        ///     The content of the sheet.
        ///     This is a bindable property.
        ///     <remarks><see cref="BindingContextFactory" /> to set the binding context when the sheet opens</remarks>
        /// </summary>
        public View? SheetContent
        {
            get => (View)GetValue(SheetContentProperty);
            set => SetValue(SheetContentProperty, value);
        }

        /// <summary>
        ///     Determines how the content of the sheet should align.
        ///     This is a bindable property.
        /// </summary>
        public ContentAlignment VerticalContentAlignment
        {
            get => (ContentAlignment)GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        internal bool IsDragging { get; set; }

        /// <inheritdoc />
        public void Hide()
        {
            CancelCommand?.Execute(CancelCommandParameter);
            CancelClickedInternal();
        }

        /// <inheritdoc />
        public async Task BeforeRemoval()
        {
            if (m_modalityLayout == null) return;
            if (m_sheetView == null) return;

            var y = Alignment switch
            {
                AlignmentOptions.Bottom => m_modalityLayout.Height,
                AlignmentOptions.Top => -m_modalityLayout.Height,
                _ => throw new ArgumentOutOfRangeException()
            };

            var translationTask = m_sheetView.SheetFrame.TranslateTo(m_sheetView.SheetFrame.X, y);
            await Task.Delay(250);
            await translationTask;
        }

        /// <inheritdoc />
        public Task AfterRemoval()
        {
            OnCloseCommand?.Execute(OnCloseCommandParameter);
            OnClose?.Invoke(this, EventArgs.Empty);

            return Task.CompletedTask;
        }

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        public bool CloseOnOverlayTapped
        {
            get => (bool)GetValue(CloseOnOverlayTappedProperty);
            set => SetValue(CloseOnOverlayTappedProperty, value);
        }

        private static object DefaultValueCreator(BindableObject bindable)
        {
            if (bindable is SheetBehavior sheetBehavior) return new CancelSheetCommand(() => sheetBehavior.IsOpen = false);

            return new CancelSheetCommand(() => { });
        }

        internal void ActionClickedInternal()
        {
            ActionClicked?.Invoke(this, EventArgs.Empty);
        }

        internal async void CancelClickedInternal()
        {
            CancelClicked?.Invoke(this, EventArgs.Empty);
            if (!(CancelCommand is CancelSheetCommand))
            {
                IsOpen = false;
                return;
            }

            if (CancelCommand is CancelSheetCommand cancelSheetCommand && await cancelSheetCommand.CanCloseSheet(CancelCommandParameter))
            {
                IsOpen = false;
            }

        }

        private static void OnVerticalContentAlignmentPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (!(bindable is SheetBehavior sheetBehavior)) return;
            sheetBehavior.m_sheetView?.ChangeVerticalContentAlignment();
        }

        /// <summary>
        ///     Event that gets raised when the sheet has changed it's position.
        /// </summary>
        public event EventHandler<PositionEventArgs>? OnPositionChanged;

        private static void OnSheetContentPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (!(bindable is SheetBehavior sheetBehavior)) return;
            sheetBehavior.UpdateBindingContextForSheetContent();
        }

        private static void BackgroundColorPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (!(bindable is SheetBehavior sheetBehavior)) return;
            sheetBehavior.ContentColor = (Color)newvalue;
            sheetBehavior.HeaderColor = (Color)newvalue;
        }

        /// <summary>
        ///     Event that gets raised when the sheet is about to start it's animation to open.
        /// </summary>
        public event EventHandler? OnBeforeOpen;

        /// <summary>
        ///     Event that gets raised when the sheet has completed it's animation and is open.
        /// </summary>
        public event EventHandler? OnOpen;

        /// <summary>
        ///     Event that gets raised before the sheet start it's animation when closing
        /// </summary>
        public event EventHandler? OnBeforeClose;

        /// <summary>
        ///     Event that gets raised when the sheet has completed it's animation and is closed.
        /// </summary>
        public event EventHandler? OnClose;

        /// <summary>
        ///     Event that gets raised when the user has clicked the action button.
        /// </summary>
        public event EventHandler? ActionClicked;

        /// <summary>
        ///     Event that gets raised when the user has clicked the cancel button.
        /// </summary>
        public event EventHandler? CancelClicked;

        private static async void OnPositionPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (!(bindable is SheetBehavior sheetBehavior)) return;
            if (!(oldvalue is double doubleOldValue)) return;
            if (!(newvalue is double doubleNewValue)) return;
            if (Math.Abs(doubleOldValue - doubleNewValue) < 0.0001) return;
            await sheetBehavior.TranslateBasedOnPosition(doubleNewValue);

            sheetBehavior.OnPositionChangedCommand?.Execute(doubleNewValue);
            sheetBehavior.OnPositionChanged?.Invoke(sheetBehavior, new PositionEventArgs(doubleNewValue, doubleOldValue));
        }

        /// <inheritdoc />
        protected override void OnAttachedTo(ModalityLayout bindable)
        {
            base.OnAttachedTo(bindable);
            m_modalityLayout = bindable;
            m_modalityLayout.BindingContextChanged += OnBindingContextChanged;
            m_modalityLayout.SizeChanged += OnModalityLayoutSizeChanged;
        }

        private void OnModalityLayoutSizeChanged(object sender, EventArgs e)
        {
            if (m_modalityLayout?.CurrentShowingModalityLayout == this)
                return; //Jump out of the size changed event if the modality layout size changes and the sheet is currently visible
            ToggleSheetVisibility();
        }

        /// <inheritdoc />
        protected override void OnDetachingFrom(ModalityLayout bindable)
        {
            base.OnDetachingFrom(bindable);
            if (m_modalityLayout == null) return;
            m_modalityLayout.BindingContextChanged -= OnBindingContextChanged;
            ToggleSheetVisibility();
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            BindingContext = m_modalityLayout?.BindingContext;
        }

        private static void IsOpenPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (!(bindable is SheetBehavior sheetBehavior))
                return;
            if (!(oldvalue is bool boolOldValue)) return;
            if (!(newvalue is bool boolNewvalue)) return;
            if (boolOldValue == boolNewvalue) return;
            sheetBehavior.ToggleSheetVisibility();
        }

        private async void ToggleSheetVisibility()
        {
            m_fromIsOpenContext = true;
            if (m_modalityLayout == null) return;

            if (!ShouldRememberPosition)
            {
                if (m_originalPosition == null) //If original position is not set
                {
                    m_originalPosition = Position; //Set the original position when opened
                }
                else
                {
                    if(m_originalPosition > 0) //If it is set and it's set to more than 0
                    {
                        Position = (double)m_originalPosition; //Set Position
                    }
                }
            }

            if (IsOpen)
            {
                OnBeforeOpenCommand?.Execute(OnBeforeOpenCommandParameter);
                OnBeforeOpen?.Invoke(this, EventArgs.Empty);

                m_sheetView = new SheetView(this);
                if (SheetContentTemplate != null)
                {
                    SheetContent = (View)SheetContentTemplate.CreateContent();
                }
                UpdateBindingContextForSheetContent();
                m_sheetView.Initialize();

                //Set height / width
                var widthConstraint = Constraint.RelativeToParent(r => m_modalityLayout.Width);
                var heightConstraint =
                    Constraint.RelativeToParent(
                        r => m_modalityLayout.Height +
                             m_sheetView.SheetFrame
                                 .CornerRadius); //Respect the corner radius to make sure that we do not display the corner radius at the "start" of the sheet

                m_modalityLayout.Show(this, m_sheetView.SheetFrame, widthConstraint: widthConstraint, heightConstraint: heightConstraint);
                //Set start position
                m_sheetView.SheetFrame.TranslationY = Alignment switch
                {
                    AlignmentOptions.Bottom => m_sheetView.SheetFrame.Height,
                    AlignmentOptions.Top => -m_sheetView.SheetFrame.Height,
                    _ => throw new ArgumentOutOfRangeException()
                };
                
                // Wait until all the bindings are done
                await Task.Delay(100);

                //Set position based on size of content
                if (Position <= MinPosition)
                {
                    //Calculate what size the content needs if the position is set to 0
                    var newPosition = m_sheetView.SheetContentHeightRequest / m_modalityLayout.Height;
                    await TranslateBasedOnPosition(newPosition);
                }
                else //Set position from input
                {
                    await TranslateBasedOnPosition(Position);
                }
            }
            else
            {
                if (m_sheetView == null) return;

                OnBeforeCloseCommand?.Execute(OnBeforeCloseCommandParameter);
                OnBeforeClose?.Invoke(this, EventArgs.Empty);

                m_modalityLayout.Hide(m_sheetView.SheetFrame);
            }

            m_fromIsOpenContext = false;
        }

        private void UpdateBindingContextForSheetContent()
        {
            if (m_sheetView == null) return;
            if (m_sheetView.SheetContentView == null)return;
            m_sheetView.SheetContentView.Content.BindingContext = BindingContextFactory?.Invoke() ?? BindingContext;
        }

        private async Task TranslateBasedOnPosition(double newPosition)
        {
            // IMPORTANT: Remember to always return when you set position in this method, else the app can go into a recursive forever loop

            if (m_supressNextTranslation)
                return;
            if (!IsOpen) return;
            if (m_modalityLayout == null) return;
            if (m_sheetView == null) return;
            


            if (MinPosition <= 0 || MinPosition > 1) //Min position should be between 0-1
            {
                throw new XamlParseException($"{nameof(SheetBehavior)} {nameof(MinPosition)} has to be between 0 and 1");
            }

            if (MaxPosition <= 0 || MaxPosition > 1) //Max position should be between 0-1
            {
                throw new XamlParseException($"{nameof(SheetBehavior)} {nameof(MaxPosition)} has to be between 0 and 1");
            }

            if (MinPosition > MaxPosition) //Min position should be less than max position
            {
                throw new XamlParseException($"{nameof(SheetBehavior)} {nameof(MinPosition)} can not be larger than {nameof(MaxPosition)}");
            }


            if (m_fromIsOpenContext)
            {
                if (newPosition < MinPosition) // If its calculated to open at a position thats less than min, use the same position as min
                {
                    newPosition = SetPositionAndSupressNextTranslation(MinPosition);
                }
            }
            else
            {
                if(newPosition <= MinPosition) // If its dragged to a position thats less or equal to min
                {
                    if(ShouldAutoClose && !IsDragging) 
                    {
                        IsOpen = false; //Auto-close
                        return;
                    }
                    else if(!IsDragging)
                    {
                        newPosition = SetPositionAndSupressNextTranslation(MinPosition);
                    }
                }
            }

            if (newPosition > MaxPosition) //If the content is to big
            {
                newPosition = SetPositionAndSupressNextTranslation(MaxPosition);
            }

            var yTranslation = Alignment switch
            {
                AlignmentOptions.Bottom => m_sheetView.SheetFrame.Height * (1 - newPosition),
                AlignmentOptions.Top => -m_sheetView.SheetFrame.Height * (1 - newPosition) - m_sheetView.SheetFrame.CornerRadius,
                _ => 0
            };

            if (m_fromIsOpenContext || !IsDragging)
            {
                var translationTask = m_sheetView.SheetFrame.TranslateTo(m_sheetView.SheetFrame.X, yTranslation);
                
                await Task.Delay(250);
                await translationTask;

                OnOpenCommand?.Execute(OnOpenCommandParameter);
                OnOpen?.Invoke(this, new EventArgs());
            }
            else
            {
                m_sheetView.SheetFrame.TranslationY = yTranslation;
            }
        }

        private double SetPositionAndSupressNextTranslation(double position)
        {
            m_supressNextTranslation = true;
            var newPosition = Position = position; //Lock the sheet in position
            m_supressNextTranslation = false;
            return newPosition;
        }

        internal async Task UpdatePosition(double newYPosition)
        {
            if (m_modalityLayout == null) return;
            if (m_sheetView == null) return;

            Position = Alignment switch
            {
                AlignmentOptions.Bottom => (m_sheetView.SheetFrame.Height - newYPosition) / m_modalityLayout.Height,
                AlignmentOptions.Top => (m_sheetView.SheetFrame.Height + newYPosition) / m_modalityLayout.Height,
                _ => throw new ArgumentOutOfRangeException()
            };
            if(!IsDragging)
            {
                await TranslateBasedOnPosition(Position);
            }
        }
    }

    /// <summary>
    ///     The positions of the sheet when it appears.
    /// </summary>
    public enum AlignmentOptions
    {
        /// <summary>
        ///     Positions the sheet at the bottom of the screen and will be draggable from bottom to top.
        /// </summary>
        Bottom = 0,

        /// <summary>
        ///     Positions the sheet at the top of the screen and will be draggable from top to bottom.
        /// </summary>
        Top
    }

    /// <summary>
    ///     The alignment of the content of the sheet.
    /// </summary>
    public enum ContentAlignment
    {
        /// <summary>
        ///     The content will only use as much space as it needs.
        /// </summary>
        Fit = 0,

        /// <summary>
        ///     The content will use the entire sheet as space.
        /// </summary>
        Fill,
        /// <summary>
        ///     The content will use the same space as the sheet
        /// </summary>
        SameAsSheet
    }

    /// <summary>
    ///     Event args that will be sent when the position changes
    /// </summary>
    public class PositionEventArgs : EventArgs
    {
        /// <inheritdoc />
        public PositionEventArgs(double newPosition, double oldPosition)
        {
            NewPosition = newPosition;
            OldPosition = oldPosition;
        }

        /// <summary>
        ///     The new position when the sheet changed it's position
        /// </summary>
        public double NewPosition { get; }

        /// <summary>
        ///     The old position when the sheet changed it's position
        /// </summary>
        public double OldPosition { get; }
    }
}