using System;
using System.Reflection;

namespace MirrorMapper
{
    internal class MirrorProperty<T> : IMirrorProperty
    {
        private Action<T, object> _setterDelegate; 

        public MirrorProperty(PropertyInfo property)
        {
            MethodInfo methodTwo = property.GetSetMethod();
            if (methodTwo != null)
            {
                MethodInfo methodOne = GetType().GetMethod("CreateSetterDelegate", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo methodZero = methodOne.MakeGenericMethod(methodTwo.GetParameters()[0].ParameterType);
                _setterDelegate = (Action<T, object>)methodZero.Invoke(this, new object[] { methodTwo });
            }
            else
            {
                _setterDelegate = (x, y) => { };
            }
        }

        public void Set(object target, object value)
        {
            _setterDelegate((T)target, value);
        }

        // This method is called via reflection during the mapping process
        private static Action<T, object> CreateSetterDelegate<TInput>(MethodInfo method)
        {
            Action<T, TInput> internalDelegate;
            internalDelegate = (Action<T, TInput>) Delegate.CreateDelegate(typeof(Action<T, TInput>), method);

            return (target, param) =>
            {
                try
                {
                    internalDelegate(target, (TInput)(param != DBNull.Value ? param : null));
                }
                catch (Exception ex)
                {
                    string objectType = internalDelegate.Method.DeclaringType.Name;
                    string propertyName = internalDelegate.Method.Name.Substring(4);
                    string propertyType = internalDelegate.Method.GetParameters()[0].ParameterType.Name;
                    string valueType = param == DBNull.Value || param == null ? "NULL" : param.GetType().ToString();
                    string message = string.Format("{0}.{1} is a {2} but the value in the data table is a {3}. {4}", objectType, propertyName, propertyType, valueType, param);
                    InvalidCastException ice = new InvalidCastException(message, ex);
                    throw ice;
                }
            };
        }
    }
}
