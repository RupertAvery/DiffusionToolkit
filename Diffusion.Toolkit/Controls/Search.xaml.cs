using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Diffusion.Toolkit.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Diffusion.Database;

namespace Diffusion.Toolkit.Controls
{
    /// <summary>
    /// Interaction logic for Search.xaml
    /// </summary>
    public partial class Search : UserControl
    {
        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register(
                name: nameof(Filter),
                propertyType: typeof(SearchControlModel),
                ownerType: typeof(Search),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(
                name: nameof(SearchCommand),
                propertyType: typeof(ICommand),
                ownerType: typeof(Search),
                typeMetadata: new FrameworkPropertyMetadata(
                    defaultValue: null,
                    propertyChangedCallback: PropertyChangedCallback)
            );

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var search = d as Search;

            if (e.Property.Name == nameof(Filter))
            {
                search.Filter.PropertyChanged += FilterOnPropertyChanged;

            }
        }

        static PropertyInfo[] props = typeof(SearchControlModel).GetProperties();

        private static void FilterOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {

            //if (!e.PropertyName.StartsWith("Use"))
            //{
            //    var prop = props.FirstOrDefault(x => x.Name == $"Use{e.PropertyName}");
            //    if (prop != null)
            //    {
            //        prop.SetValue(sender, true);
            //    }
            //}
        }

        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }


        public SearchControlModel Filter
        {
            get => (SearchControlModel)GetValue(FilterProperty);
            set => SetValue(FilterProperty, value);
        }

        public Search()
        {
            InitializeComponent();
        }



        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var binding = textBox.GetBindingExpression(TextBox.TextProperty);

            if (e.Key == Key.Enter)
            {
                binding.UpdateSource();

                if (SearchCommand.CanExecute(null))
                {
                    SearchCommand.Execute(null);
                }
            }
            else
            {
                var propertyName = binding.ResolvedSourcePropertyName;

                string checkBoxName = $"Use{propertyName}";

                switch (propertyName)
                {
                    case nameof(Filter.Height):
                    case nameof(Filter.Width):
                        checkBoxName = nameof(Filter.UseSize);
                        break;
                    case nameof(Filter.SeedStart):
                    case nameof(Filter.SeedEnd):
                        checkBoxName = nameof(Filter.UseSeed);
                        break;
                }

                var prop = props.FirstOrDefault(x => x.Name == checkBoxName);
                if (prop != null)
                {
                    prop.SetValue(Filter, true);
                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as RadioButton;
            var binding = checkBox.GetBindingExpression(RadioButton.IsCheckedProperty);

            var propertyName = binding.ResolvedSourcePropertyName;

            if (propertyName.EndsWith("Op"))
            {
                propertyName = propertyName.Substring(0, propertyName.Length - 2);
            }

            string checkBoxName = $"Use{propertyName}";

            var prop = props.FirstOrDefault(x => x.Name == checkBoxName);
            if (prop != null)
            {
                prop.SetValue(Filter, true);
            }
        }
    }



}
