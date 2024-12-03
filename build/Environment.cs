using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<Environment>))]
public class Environment : Enumeration
{
  public static Environment Development = new Environment { Value = nameof(Development) };
  public static Environment Test = new Environment { Value = nameof(Test) };
  public static Environment Staging = new Environment { Value = nameof(Staging) };
  public static Environment Production = new Environment { Value = nameof(Production) };

  public static implicit operator string(Environment environment)
  {
    return environment.Value;
  }
}
