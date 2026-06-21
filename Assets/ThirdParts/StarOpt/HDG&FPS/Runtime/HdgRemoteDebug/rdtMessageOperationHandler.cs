using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameDLL.Hdg
{
    public class rdtMessageOperationHandler
	{
		struct Method
        {
			public object target;
			public MethodInfo method;
			public Type returnType;

			public object Execute(object[] parameters)
            {
				try
				{
					object result = method.Invoke(target, parameters);
					if (returnType != null)
						return result;
					return null;
				}
				catch(Exception e)
                {
					return e;
                }
			}
		}


		private RemoteDebugServer m_server;

		private Dictionary<string, Method> m_operationCallbacks = new Dictionary<string, Method>();

		public rdtMessageOperationHandler(RemoteDebugServer server)
		{
			m_server = server;
			m_server.AddCallback(typeof(rdtTcpMessageOperation), OnExecuteOperation);
		}

		private void OnExecuteOperation(rdtTcpMessage message)
		{
			rdtTcpMessageOperation operation = (rdtTcpMessageOperation)message;
			Method method;
			if (m_operationCallbacks.TryGetValue(operation.m_operation, out method))
			{
				object result = method.Execute(operation.GetParameters());
				if (result is Exception)
				{
					rdtTcpMessageReturnResult msg = default(rdtTcpMessageReturnResult);
					msg.m_result = new rdtTcpMessageComponents.Property
					{
						m_isArray = false,
						m_name = rdtTcpMessageReturnResult.RETURN_FAIL,
						m_value = m_server.SerializerRegistry.Serialize(((Exception)result).StackTrace),
						m_type = rdtTcpMessageComponents.Property.Type.Property,
					};

				}
				else if (method.returnType != null)
				{
					rdtTcpMessageReturnResult msg = default(rdtTcpMessageReturnResult);
					msg.m_operation = operation.m_operation;
					if (method.returnType == typeof(void) || result == null)
					{
						msg.m_result = new rdtTcpMessageComponents.Property
						{
							m_isArray = false,
							m_name = rdtTcpMessageReturnResult.RETURN_SUCCESS,
							m_value = result,
							m_type = rdtTcpMessageComponents.Property.Type.Property,
						};
					}
					else
                    {
						object value = m_server.SerializerRegistry.Serialize(result);
						if (result != null && (value == null || value.Equals(null)))
							return;
						msg.m_result = new rdtTcpMessageComponents.Property
						{
							m_isArray = (value is rdtSerializerContainerArray || method.returnType.IsGenericList() || method.returnType.IsArray),
							m_name = rdtTcpMessageReturnResult.RETURN_SUCCESS,
							m_value = value,
							m_type = rdtTcpMessageComponents.Property.Type.Property,
						};
					}

					m_server.EnqueueMessage(msg);
				}
			}
		}

		public void AddOperation(string name, MethodInfo method, object target = null, bool returnValue = false)
		{
			if (!string.IsNullOrEmpty(name) && method != null)
			{
				Method m;
				if (!m_operationCallbacks.TryGetValue(name, out m))
				{
					m = new Method()
					{
						target = target,
						method = method,
						returnType = returnValue ? method.ReturnType : null,
					};
					m_operationCallbacks.Add(name, m);
				}
				else
					rdtDebug.Error(this, "AddOperation fail. name: {0} already exists.", name);
			}
			else if(method == null)
				rdtDebug.Error("AddOperation fail, method is null.");
			else
				rdtDebug.Error("AddOperation fail, name is null.");
		}
	}
}
