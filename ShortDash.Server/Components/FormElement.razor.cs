using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.CompilerServices;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private FormGeneratorPropertyMapper componentsMapper;

        [Parameter]
        public string DescriptionFieldClasses { get; set; }

        public string Id { get => ModelProperty.Name; }

        [Parameter]
        public string InputFieldClasses { get; set; }

        [Parameter]
        public object Model { get; set; }

        [Parameter]
        public PropertyInfo ModelProperty { get; set; }

        public RenderFragment RenderComponent(PropertyInfo property) => builder =>
        {
            var componentType = GetInputType(property);
            if (componentType == null) { throw new Exception($"No component found: {property.PropertyType}"); }
            if (componentType == null) { return; }
            var elementType = componentType;
            if (elementType.IsGenericTypeDefinition)
            {
                Type[] typeArgs = { property.PropertyType };
                elementType = elementType.MakeGenericType(typeArgs);
            }

            var instance = Activator.CreateInstance(elementType);
            var method = typeof(FormElementComponent).GetMethod(nameof(FormElementComponent.RenderFormComponent), BindingFlags.NonPublic | BindingFlags.Instance);
            var genericMethod = method.MakeGenericMethod(property.PropertyType, elementType);
            genericMethod.Invoke(this, new object[] { this, Model, property, builder, instance });
        };

        protected override void OnInitialized()
        {
            base.OnInitialized();
            componentsMapper = ScopedServices.GetService(typeof(FormGeneratorPropertyMapper)) as FormGeneratorPropertyMapper;
        }

        protected void RenderFormComponent<T, TElement>(object target, object dataContext, PropertyInfo property, RenderTreeBuilder builder, InputBase<T> instance)
        {
            var displayAttribute = GetDisplayAttribute(property);

            // Generate the Label
            var label = displayAttribute?.GetName() ?? ModelProperty.Name;
            if (!string.IsNullOrWhiteSpace(label))
            {
                builder.OpenRegion(0);
                builder.OpenElement(0, "label");
                builder.AddAttribute(1, "for", Id);
                builder.AddContent(1, label);
                builder.CloseElement();
                builder.CloseRegion();
            }

            // Generate the InputBase component
            builder.OpenRegion(1);
            builder.OpenComponent(0, typeof(TElement));
            var s = property.GetValue(dataContext);
            builder.AddAttribute(1, "id", Id);
            builder.AddAttribute(2, nameof(InputBase<T>.Value), s);
            builder.AddAttribute(3, nameof(InputBase<T>.ValueChanged),
                RuntimeHelpers.TypeCheck(EventCallback.Factory.Create(
                    target,
                    EventCallback.Factory.CreateInferred(
                        target,
                        value => property.SetValue(dataContext, value),
                        (T)property.GetValue(dataContext)))));
            var expressionConstant = Expression.Constant(dataContext, dataContext.GetType());
            var expressionProperty = Expression.Property(expressionConstant, property.Name);
            var expressionLambda = Expression.Lambda<Func<T>>(expressionProperty);
            builder.AddAttribute(4, nameof(InputBase<T>.ValueExpression), expressionLambda);
            builder.AddAttribute(5, "class", GetInputFieldClasses(instance));

            var prompt = displayAttribute?.Prompt;
            if (!string.IsNullOrWhiteSpace(prompt))
            {
                builder.AddAttribute(6, "placeholder", prompt);
            }

            builder.CloseComponent();
            builder.CloseRegion();

            // Generate Description Text
            var description = displayAttribute?.GetDescription();
            if (!string.IsNullOrWhiteSpace(description))
            {
                builder.OpenRegion(2);
                builder.OpenElement(0, "small");
                if (!string.IsNullOrWhiteSpace(DescriptionFieldClasses))
                {
                    builder.AddAttribute(1, "class", DescriptionFieldClasses);
                }
                builder.AddContent(2, description);
                builder.CloseElement();
                builder.CloseRegion();
            }

            // Generate validator
            builder.OpenRegion(3);
            builder.OpenComponent(0, typeof(ValidationMessage<T>));
            builder.AddAttribute(1, nameof(ValidationMessage<T>.For), expressionLambda);
            builder.CloseComponent();
            builder.CloseRegion();
        }

        private DisplayAttribute GetDisplayAttribute(PropertyInfo property)
        {
            return property
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;
        }

        private string GetInputFieldClasses<T>(InputBase<T> instance)
        {
            var output = InputFieldClasses;
            if (instance == null) { return output; }
            var additionalAttributes = instance.AdditionalAttributes;
            if (additionalAttributes != null && additionalAttributes.TryGetValue("class", out var cssClass) && !string.IsNullOrEmpty(Convert.ToString(cssClass)))
            {
                return $"{cssClass} {output}";
            }
            return output;
        }

        private Type GetInputType(PropertyInfo property)
        {
            return property.GetCustomAttribute<FormInputAttribute>()?.Type
                ?? componentsMapper.GetComponent(property.PropertyType.ToString());
        }
    }
}