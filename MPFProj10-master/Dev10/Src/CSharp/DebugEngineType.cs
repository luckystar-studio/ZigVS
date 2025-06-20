namespace Microsoft.VisualStudio.Project
{
	[PropertyPageTypeConverterAttribute(typeof(DebugEngineTypeConverter))]
	public enum DebugEngineType
	{
        WindowsNative,
		MIEngine
	}
}
