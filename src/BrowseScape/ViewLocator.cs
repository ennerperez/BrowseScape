using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BrowseScape.ViewModels;

namespace BrowseScape
{
  public class ViewLocator : IDataTemplate
  {

    public Control? Build(object? data)
    {
      var fullName = data?.GetType().FullName;
      if (fullName == null)
      {
        return null;
      }
      var name = fullName.Replace("ViewModel", "View", StringComparison.Ordinal);
      var type = Type.GetType(name);

      if (type == null)
      {
        return new TextBlock { Text = "Not Found: " + name };
      }
      var control = (Control)Activator.CreateInstance(type)!;
      control.DataContext = data;
      return control;
    }

    public bool Match(object? data)
    {
      return data is ViewModelBase;
    }
  }
}
