﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Morphous.Api;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment;
using Orchard.Fields.Settings;
using Orchard.Localization;

namespace Morphous.FormsApi.Shapes {
    public class FieldShapes : ApiShapesBase, IShapeTableProvider {
        private readonly Work<IContentManager> _contentManager;
        private readonly Work<IBindingTypeCreateAlterations> _alterations;

        public FieldShapes(
            Work<IContentManager> contentManager,
            Work<IBindingTypeCreateAlterations> alterations
            ) {
            _contentManager = contentManager;
            _alterations = alterations;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public void Discover(ShapeTableBuilder builder) {

        }


        [Shape(bindingType: "Translate")]
        public void Fields_Boolean__api__Forms(dynamic Display, dynamic Shape, TextWriter Output) {
            using (Display.ViewDataContainer.Model.Node("a-list-item")) {
                Display.ViewDataContainer.Model.Set("Type", Shape.ContentField.FieldDefinition.Name);
                Display.ViewDataContainer.Model.Set("Name", Shape.ContentField.Name);
                bool? booleanValue = Shape.ContentField.Value;
                Display.ViewDataContainer.Model.Set("Value", booleanValue.HasValue ? booleanValue.Value : booleanValue);
            }
        }


        [Shape(bindingType: "Translate")]
        public void Fields_MediaLibraryPicker__api__Forms(dynamic Display, dynamic Shape)
        {
            using (Display.ViewDataContainer.Model.Node("a-list-item"))
            {
                Display.ViewDataContainer.Model.Set("Type", Shape.ContentField.FieldDefinition.Name);
                Display.ViewDataContainer.Model.Set("Name", Shape.ContentField.Name);

                using (Display.ViewDataContainer.Model.List("Media"))
                {
                    using (_alterations.Value.CreateScope("Translate"))
                    {
                        foreach (var item in Shape.ContentField.MediaParts)
                        {
                            var mediaShape = _contentManager.Value.BuildDisplay(item, "Summary");
                            Display(mediaShape);
                        }
                    }
                }
            }
        }

        private void Fields_MediaLibraryPicker__api__Flat(dynamic Display, dynamic Shape)
        {
            var field = Shape.ContentField;
            string name = field.DisplayName;
            var contents = field.MediaParts;
            var mediaShapes = new List<dynamic>();

            using (Display.ViewDataContainer.Model.List(Shape.ContentField))
            {
                using (_alterations.Value.CreateScope("Translate"))
                {
                    foreach (var item in contents)
                    {
                        Display(_contentManager.Value.BuildDisplay(item, "Summary"));
                    }
                }
            }
        }


        // Not in the forms format yet
        [Shape(bindingType: "Translate")]
        public void Fields_Common_Text__api__Forms(dynamic Display, dynamic Shape) {
            using (Display.ViewDataContainer.Model.Node(Shape.ContentField)) {
                Display.ViewDataContainer.Model.Set(Shape.ContentField.Name, Shape.Value.ToString());
            }
        }

        [Shape(bindingType: "Translate")]
        public void Fields_Input__api__Forms(dynamic Display, dynamic Shape) {
            using (Display.ViewDataContainer.Model.Node(Shape.ContentField)) {
                Display.ViewDataContainer.Model.Value = Shape.Value;
            }
        }
        
        private void Fields_Input__api__Flat(dynamic Display, dynamic Shape) {
            Display.ViewDataContainer.Model.Set(Shape.ContentField.Name, Shape.Value);
        }

        [Shape(bindingType: "Translate")]
        public void Fields_DateTime__api__Forms(dynamic Display, dynamic Shape) {
            using (Display.ViewDataContainer.Model.Node(Shape.ContentField)) {
                Display.ViewDataContainer.Model.Value = Shape.ContentField.DateTime;
            }
        }

        private void Fields_DateTime__api__Flat(dynamic Display, dynamic Shape) {
            Display.ViewDataContainer.Model.Set(Shape.ContentField.Name, Shape.ContentField.DateTime);
        }

        [Shape(bindingType: "Translate")]
        public void Fields_Numeric__api__Forms(dynamic Display, dynamic Shape) {
            using (Display.ViewDataContainer.Model.Node(Shape.ContentField)) {
                Display.ViewDataContainer.Model.Value = Shape.Value;
            }
        }

        private void Fields_Numeric__api__Flat(dynamic Display, dynamic Shape) {
            Display.ViewDataContainer.Model.Set(Shape.ContentField.Name, Shape.Value);
        }

        [Shape(bindingType: "Translate")]
        public void Fields_Enumeration__api__Forms(dynamic Display, dynamic Shape) {

            string valueToDisplay = string.Empty;
            string[] selectedValues = Shape.ContentField.SelectedValues;
            string[] translatedValues = new string[0];
            if (selectedValues != null) {
                string valueFormat = T("{0}").ToString();
                translatedValues = selectedValues.Select(v => string.Format(valueFormat, T(v).Text)).ToArray();
            }

            using (Display.ViewDataContainer.Model.Node(Shape.ContentField)) {
                Display.ViewDataContainer.Model.Value = translatedValues;
            }
        }

        private void Fields_Enumeration__api__Flat(dynamic Display, dynamic Shape) {
            string valueToDisplay = string.Empty;
            string[] selectedValues = Shape.ContentField.SelectedValues;
            string[] translatedValues = new string[0];
            if (selectedValues != null) {
                string valueFormat = T("{0}").ToString();
                translatedValues = selectedValues.Select(v => string.Format(valueFormat, T(v).Text)).ToArray();
            }

            Display.ViewDataContainer.Model.Set(Shape.ContentField.Name, translatedValues);
        }

        [Shape(bindingType: "Translate")]
        public void Fields_Link__api__Forms(dynamic Display, dynamic Shape) {
                Fields_Link__api__Flat(Display, Shape);
        }
        
        private void Fields_Link__api__Flat(dynamic Display, dynamic Shape) {

            string name = Shape.ContentField.DisplayName;
            LinkFieldSettings settings = Shape.ContentField.PartFieldDefinition.Settings.GetModel<LinkFieldSettings>();
            string text = Shape.ContentField.Text;
            switch (settings.LinkTextMode) {
                case LinkTextMode.Static:
                    text = settings.StaticText;
                    break;
                case LinkTextMode.Url:
                    text = Shape.ContentField.Value;
                    break;
                case LinkTextMode.Optional:
                    if (String.IsNullOrWhiteSpace(text)) {
                        text = Shape.ContentField.Value;
                    }
                    break;
            }


            using (Display.ViewDataContainer.Model.Node(Shape.ContentField)) {
                Display.ViewDataContainer.Model.Url = Shape.ContentField.Value;
                Display.ViewDataContainer.Model.Text = text;
                Display.ViewDataContainer.Model.Target = Shape.ContentField.Target;
            }
        }
    }
}
