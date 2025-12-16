namespace FunctionalDdd.CommonValueObjectGenerator;

internal class RequiredPartialClassInfo
{
    public readonly string NameSpace;
    public readonly string ClassName;
    public readonly string ClassBase;
    public readonly string Accessibility;

    public RequiredPartialClassInfo(string nameSpace, string className, string classBase, string accessibility)
    {
        NameSpace = nameSpace;
        ClassName = className;
        ClassBase = classBase;
        Accessibility = accessibility;
    }

}
