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
        private string label;
        public string CssClass { get => string.Join(" ", CssClasses.ToArray()); }

        [Parameter]
        public List<string> CssClasses { get; set; }

        [Parameter]
        public object DataContext { get; set; }

        [Parameter]
        public List<string> DefaultFieldClasses { get; set; }

        [Parameter]
        public PropertyInfo FieldIdentifier { get; set; }

        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string Label
        {
            get
            {
                var dd = DataContext
                    .GetType()
                    .GetProperty(FieldIdentifier.Name)
                    .GetCustomAttributes(typeof(DisplayAttribute), false)
                    .FirstOrDefault() as DisplayAttribute;
                return label ?? dd?.Name;
            }
            set
            {
                label = value;
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
            builder.OpenComponent(0, typeof(TElement));
            var s = propInfo.GetValue(dataContext);
            builder.AddAttribute(1, nameof(InputBase<T>.Value), s);
            builder.AddAttribute(3, nameof(InputBase<T>.ValueChanged),
                RuntimeHelpers.TypeCheck(EventCallback.Factory.Create<T>(
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

            CheckForInterfaceActions<T, TElement>(this, DataContext, propInfo, builder, instance, 6);
            builder.CloseComponent();
        }

        private static bool TypeImplementsInterface(Type type, Type typeToImplement)
        {
            Type foundInterface = type
                .GetInterfaces()
                .Where(i =>
                {
                    return i.Name == typeToImplement.Name;
                })
                .Select(i => i)
                .FirstOrDefault();

            return foundInterface != null;
        }

        private void CheckForInterfaceActions<T, TElement>(object target, object dataContext, PropertyInfo propInfo, RenderTreeBuilder builder, InputBase<T> instance, int indexBuilder)
        {
            /*
            if (TypeImplementsInterface(typeof(TElement), typeof(IRenderAsFormElement)))
            {
                this.CssClasses.AddRange((instance as IRenderAsFormElement).FormElementClasses);
            }
            if (TypeImplementsInterface(typeof(TElement), typeof(IRenderChildren)))
            {
                (instance as RenderChildren).RenderChildren(builder, indexBuilder, dataContext, propInfo);
            }
            */
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

        private bool IsTypeDerivedFromGenericType(Type typeToCheck, Type genericType)
        {
            if (typeToCheck == typeof(object)) { return false; }
            if (typeToCheck == null) { return false; }
            if (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericType) { return true; }
            return IsTypeDerivedFromGenericType(typeToCheck.BaseType, genericType);
        }
    }
}