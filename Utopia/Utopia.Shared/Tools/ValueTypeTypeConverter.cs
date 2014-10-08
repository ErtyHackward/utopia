using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace Utopia.Shared.Tools
{
    public class ValueTypeTypeConverter<T> : ExpandableObjectConverter where T : struct
    {
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null)
                throw new ArgumentNullException("propertyValues");

            T ret = default(T);
            object boxed = ret;
            try
            {
                foreach (DictionaryEntry entry in propertyValues)
                {
                    PropertyInfo pi = ret.GetType().GetProperty(entry.Key.ToString());
                    if ((pi != null) && (pi.CanWrite))
                    {
                        pi.SetValue(boxed, Convert.ChangeType(entry.Value, pi.PropertyType), null);
                    }
                }
            }
            catch (Exception x)
            {
                MessageBox.Show(x.Message);
            }
            return (T)boxed;
        }
    }
}