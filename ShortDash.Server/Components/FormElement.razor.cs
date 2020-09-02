using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace ShortDash.Server.Components
{
    // Adapted from https://github.com/Aaltuj/BlazorFormGeneratorDemo
    public class FormElementComponent : OwningComponentBase
    {
        private readonly FormGeneratorComponentsRepository componentsRepository = new FormGeneratorComponentsRepository();

        [CascadingParameter(Name = "DataContext")]
        public object DataContext { get; set; }

        [Parameter]
        public List<string> DefaultFieldClasses { get; set; }

        [Parameter]
        public PropertyInfo FieldIdentifier { get; set; }

        public string Id { get => FieldIdentifier.Name; }

        public string Label
        {
            get
            {
                var displayAttribute = DataContext
                    .GetType()
                    .GetProperty(FieldIdentifier.Name)
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault() as DisplayAttribute;
                return displayAttribute?.Name ?? FieldIdentifier.Name;
            }
        }

        public RenderFragment CreateComponent(PropertyInfo propInfo) => builder =>
        {
            var componentType = componentsRepository.GetComponent(propInfo.PropertyType.ToString());
            if (componentType == null) { throw new Exception($"No component found: {propInfo.PropertyType}"); }
            if (componentType == null) { return; }
            var elementType = componentType;
            if (elementType.IsGenericTypeDefinition)
            {
                Type[] typeArgs = { propInfo.PropertyType };
                elementType = elementType.MakeGenericType(typeArgs);
            }

            var instance = Activator.CreateInstance(elementType);
            var method = typeof(FormElementComponent).GetMethod(nameof(FormElementComponent.CreateFormComponent));
            var genericMethod = method.MakeGenericMethod(propInfo.PropertyType, elementType);
            genericMethod.Invoke(this, new object[] { this, DataContext, propInfo, builder, instance });
        };

        public void CreateFormComponent<T, TElement>(object target, object dataContext, PropertyInfo propInfo, RenderTreeBuilder builder, InputBase<T> instance)
        {
            // Generate the Label
            if (!string.IsNullOrWhiteSpace(Label))
            {
                builder.OpenRegion(0);
                builder.OpenElement(0, "label");
                builder.AddAttribute(1, "for", Id);
                builder.AddContent(1, Label);
                builder.CloseElement();
                builder.CloseRegion();
            }

            // Generate the InputBase component
            builder.OpenRegion(1);
            builder.OpenComponent(0, typeof(TElement));
            var s = propInfo.GetValue(dataContext);
            builder.AddAttribute(1, "id", Id);
            builder.AddAttribute(2, nameof(InputBase<T>.Value), s);
            builder.AddAttribute(3, nameof(InputBase<T>.ValueChanged),
                RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(
                    target,
                    EventCallback.Factory.CreateInferred(
                        target,
                        value => propInfo.SetValue(dataContext, value),
                        (T)propInfo.GetValue(dataContext)))));
            var expressionConstant = Expression.Constant(dataContext, dataContext.GetType());
            var expressionProperty = Expression.Property(expressionConstant, propInfo.Name);
            var expressionLambda = Expression.Lambda<Func<T>>(expressionProperty);
            builder.AddAttribute(4, nameof(InputBase<T>.ValueExpression), expressionLambda);
            builder.AddAttribute(5, "class", GetDefaultFieldClasses(instance));
            builder.CloseComponent();
            builder.CloseRegion();

            // Generate validator
            builder.OpenRegion(2);
            builder.OpenComponent(0, typeof(ValidationMessage<T>));
            builder.AddAttribute(1, nameof(ValidationMessage<T>.For), expressionLambda);
            builder.CloseComponent();
            builder.CloseRegion();
        }

        private string GetDefaultFieldClasses<T>(InputBase<T> instance)
        {
            var output = DefaultFieldClasses == null ? "" : string.Join(" ", DefaultFieldClasses);
            if (instance == null) { return output; }
            var additionalAttributes = instance.AdditionalAttributes;
            if (additionalAttributes != null && additionalAttributes.TryGetValue("class", out var cssClass) && !string.IsNullOrEmpty(Convert.ToString(cssClass)))
            {
                return $"{cssClass} {output}";
            }

            return output;
        }
    }
}