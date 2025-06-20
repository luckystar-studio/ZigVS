namespace Microsoft.VisualStudio.Project
{
	using System;
	using System.ComponentModel;
	using System.Globalization;

	public class DebugEngineTypeConverter : EnumConverter
	{
		public DebugEngineTypeConverter()
			: base(typeof(DebugEngineType))
		{

		}

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType == typeof(string)) return true;

			return base.CanConvertFrom(context, sourceType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			string str = value as string;

			if(str != null)
			{
				if(str == SR.GetString(SR.WindowsNative, culture)) return DebugEngineType.WindowsNative;
				if (str == SR.GetString(SR.MIEngine, culture)) return DebugEngineType.MIEngine;
 			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == typeof(string))
			{
				string result = null;
				// In some cases if multiple nodes are selected the windows form engine
				// calls us with a null value if the selected node's property values are not equal
				if(value != null)
				{
					result = SR.GetString(((DebugEngineType)value).ToString(), culture);
				}
				else
				{
					result = SR.GetString(DebugEngineType.MIEngine.ToString(), culture);
				}

				if(result != null) return result;
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		public override bool GetStandardValuesSupported(System.ComponentModel.ITypeDescriptorContext context)
		{
			return true;
		}

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(System.ComponentModel.ITypeDescriptorContext context)
		{
			return new StandardValuesCollection(new DebugEngineType[] { DebugEngineType.WindowsNative, DebugEngineType.MIEngine });
		}
	}
}
