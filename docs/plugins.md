# Plugins

## Table of Contents

* [Overview](#overview)
* [IShortDashAction](#ishortdashaction)
* [ShortDashResult](#shortdashresult)
* [ShortDashAction Attribute](#shortdashaction-attribute)
* [ShortDashActionDefaultSettings Attribute](#shortdashactiondefaultsettings-attribute)
* [Logging](#logging)
* [Example Plugin](#example-plugin)

## Overview

ShortDash plugins are .NET core class libraries stored in a `plugins` folder for the ShortDash Server and Target.  Plugins can declare actions by including classes that implement the `IShortDashAction` interface.

All of the required interfaces and class definitions can be found in the `ShortDash.Core.Plugins` Nuget Package.  Plugin libraries must add this package as a dependency.

By convention, plugins should be named `ShortDash.Plugins.{DomainOrCompany}.PluginName` where {DomainOrCompany} is a domain or company name used to ensure a unique qualified name for the plugin.  Plugins must not use `Core` as the `{DomainOrCompany}` value as it is reserved for core functions that are provided by default.

## IShortDashAction

Any public class found in a .NET core plugin assembly that implements the `IShortDashAction` interface will be available for selection in ShortDash.

```csharp
public interface IShortDashAction
{
    ShortDashActionResult Execute(object parameters, bool toggleState);
}
```

## ShortDashResult

The `ShortDashResult` class has the following properties:

Property Name | Description
------------- | -----------
Success | The result of the action: True for success, False for failure.
ToggleState | Indicates the new toggle state for the action after the action has been executed.
UserMessage | A message that will be displayed to the user who executed the action.  If this is not assigned, no message will be displayed.

## ShortDashAction Attribute

The `ShortDashAction` attribute can be added to class implementing `IShortDashAction` to provide additional information for the action.

The ShortDashAction attribute has the following optional properties:

Property Name | Description
------------- | -----------
Description | A detailed description of the action.
ParametersType | A class type whose properties define the parameters that are associated with the action. |
Title | The title that is displayed when selecting an action type.
Toggle | Indicates if the action is able to be toggled on/off.

When an action is executed, an instance of the object of the type defined in the `ParametersType` property will be passed into the `Execute` function.  You can then cast it to the real class to access the parameters.

The attributes in the `System.ComponentModel.DataAnnotations` namespace can be used to decorate the properties for data validation.  For example, the `Required` attribute can be added to validate that the user enters in a value for the property.

The `FormInput` attribute can be added to a property to indicate the name of the input control type that will be used when the action instance is being edited.  A default input control will be selected based on the property type but this can be overridden when required. Currently, the only `TypeName` that can be used is `textarea` which can be used for `string` properties to allow a large amount of text to be used including line break.

## ShortDashActionDefaultSettings Attribute

The `ShortDashActionDefaultSettings` attribute can be added to the class to provide default settings that are available to all actions that dictate how it will be displayed.  The user can override these default values at the time the action instance is created in ShortDash.

Property Name | Description
------------- | -----------
BackgroundColor | The background color of the action button.
Icon | An icon name or file that will be used.  If blank, the action Label will be displayed instead.
Label | The label that will be displayed for the action.
ToggleBackgroundColor | The background color of the action button when the action has been toggled.
ToggleIcon | The icon that will be used when the action has been toggled.
ToggleLabel | The label that will be displayed when the action has been toggled.

## Logging

Plugins can use constructor injection to obtain a `IShortDashPluginLogger` instance.  This interface has methods such as `LogDebug` and `LogError` that can be used to send log message to the Server or Target that is executing the action.  The log entries for actions executed on Targets are automatically sent to the server as well.

## Example Plugin

Execute the following steps to create an example plugin:

```bash
dotnet new classlib --name ShortDash.Plugins.Examples.HelloWorld --framework netcoreapp3.1
cd ShortDash.Plugins.Examples.HelloWorld
dotnet add package ShortDash.Core.Plugins
```

Rename Class1.cs to HelloWorldAction.cs.
Edit HelloWorldAction.cs and replace the contents with the following:

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using ShortDash.Core.Plugins;

namespace ShortDash.Plugins.Examples.HelloWorld
{
    [ShortDashAction(
        Title = "Hello, World!",
        Description = "Displays a user definable message.",
        ParametersType = typeof(HelloWorldParameters))]
    public class HelloWorldAction : IShortDashAction
    {
        public ShortDashActionResult Execute(object parametersObject, bool toggleState)
        {
            var parameters = parametersObject as HelloWorldParameters;
            return new ShortDashActionResult { Success = true, UserMessage = parameters.Message };
        }
    }

    public class HelloWorldParameters
    {
        [Required]
        public string Message { get; set; } = "Hello, World!";
    }
}
```

Execute the following command to publish the plugin to the ShortDash Server directory.  Replace `{ShortDashServerPath}` with the directory where the ShortDash Server binaries are located.

```bash
dotnet publish -c Release -o {ShortDashServerPath}/plugins/ShortDash.Plugins.Examples.HelloWorld
```

Restart ShortDash Server and the example action will be available for selection.  When the action is executed, a toast message will be displayed with the configured message.  It defaults to "Hello, World!" but can be modified when setting up the action instance in ShortDash.
