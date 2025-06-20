namespace Microsoft.VisualStudio.Project
{
	using System;
	using System.ComponentModel;
	using System.Globalization;

	public class OSTypeConverter : EnumConverter
	{
		public OSTypeConverter()
			: base(typeof(OSType))
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
				if (str == SR.GetString(SR.Windows, culture)) return OSType.Windows;
                if (str == SR.GetString(SR.Linux, culture)) return OSType.Linux;
                if (str == SR.GetString(SR.Android, culture)) return OSType.Android;
                if (str == SR.GetString(SR.iOS, culture)) return OSType.iOS;
                if (str == SR.GetString(SR.MacOS, culture)) return OSType.macOS;
                if (str == SR.GetString(SR.WASI, culture)) return OSType.WASI;
            }

            return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(destinationType == typeof(string))
			{
				string result = null;
				if(value != null)
				{
					result = SR.GetString(((OSType)value).ToString(), culture);
				}
				else
				{
					result = SR.GetString(OSType.Windows.ToString(), culture);
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
			return new StandardValuesCollection(new OSType[] { OSType.Windows, OSType.Linux, OSType.Android, OSType.iOS, OSType.macOS, OSType.WASI });
		}
	}
}
