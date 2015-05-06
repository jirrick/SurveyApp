using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SurveyApp.Utils
{
    /// <summary>
    /// Třída umožnující dynamickou změnu vzhledu výpisu na základě typu vstupní položky
    /// </summary>
    class SurveyTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate SelectTemplateCore(object item, Windows.UI.Xaml.DependencyObject container)
        {
            var uiElement = container as UIElement;
            VariableSizedWrapGrid.SetColumnSpan(uiElement, 1);
            VariableSizedWrapGrid.SetRowSpan(uiElement, 1);

            var field = item as FieldInfo;

            // pro každý typ je nutné přiřadit styl, kterým se bude vykreslovat a v případě potřeby také udat velikost políčka
            // samotné styly jsou nadefinované v souboru App.xaml

            if (field.FieldObject.TypedObject.GetType().Equals(typeof(FieldLookup)))
            {
                return App.Current.Resources["LookupTemplate"] as DataTemplate;
            }

            if (field.FieldObject.TypedObject.GetType().Equals(typeof(FieldText)))
            {
                return App.Current.Resources["TextTemplate"] as DataTemplate;
            }

            if (field.FieldObject.TypedObject.GetType().Equals(typeof(FieldMultiLineText)))
            {
                // například typ políčka pro vkládání dlouhého textu zabírá dvojnásobou výšku ne ostatní políčka
                VariableSizedWrapGrid.SetRowSpan(uiElement, 2);
                return App.Current.Resources["NoteTemplate"] as DataTemplate;
            }

            if (field.FieldObject.TypedObject.GetType().Equals(typeof(FieldNumber)))
            {
                return App.Current.Resources["NumberTemplate"] as DataTemplate;
            }

            if (field.FieldObject.TypeAsString.Equals("Boolean"))
            {
                return App.Current.Resources["BooleanTemplate"] as DataTemplate;
            }

            if (field.FieldObject.TypedObject.GetType().Equals(typeof(FieldChoice)))
            {
                return App.Current.Resources["ChoiceTemplate"] as DataTemplate;
            }

            if (field.FieldObject.TypedObject.GetType().Equals(typeof(FieldMultiChoice)))
            {
                VariableSizedWrapGrid.SetRowSpan(uiElement, 2);
                return App.Current.Resources["MultiChoiceTemplate"] as DataTemplate;
            }

            return App.Current.Resources["DefaultTemplate"] as DataTemplate;
        }
    }
}
