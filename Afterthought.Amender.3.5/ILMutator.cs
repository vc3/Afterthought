using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cci.MutableCodeModel;
using Microsoft.Cci;
using System.Runtime.Serialization;
using System.IO;

namespace ExoGraph.Injector
{
	/// <summary>
	/// Injects a graph instance parasite and property notification eventing into eligible classes.
	/// </summary>
	public class ILMutator : MutatingVisitor
	{
		protected PdbReader pdbReader;

		public ILMutator(IMetadataHost host, PdbReader pdbReader)
			: base(host, true)
		{
			this.pdbReader = pdbReader;
		}
	
		List<ILocalDefinition> currentLocals;
		public MethodBody Process(MethodBody methodBody)
		{
			IMethodDefinition currentMethod = this.GetCurrentMethod();
			methodBody.MethodDefinition = currentMethod;
			this.currentLocals = new List<ILocalDefinition>(methodBody.LocalVariables);

			try
			{
				ProcessOperations(methodBody);
			}
			catch (ILMutatorException)
			{
				Console.WriteLine("Internal error during IL mutation for the method '{0}'.",
				  MemberHelper.GetMemberSignature(currentMethod, NameFormattingOptions.SmartTypeName));
			}
			finally
			{
				this.currentLocals = null;
			}

			return methodBody;
		}

		private void ProcessOperations(MethodBody methodBody)
		{

			List<IOperation> operations = methodBody.Operations;
			int count = methodBody.Operations.Count;

			ILGenerator generator = new ILGenerator(this.host, methodBody.MethodDefinition);

			var methodName = MemberHelper.GetMemberSignature(methodBody.MethodDefinition, NameFormattingOptions.SmartTypeName);


			
			#endregion Pass 2: Emit each operation, along with labels

			#region Retrieve the operations (and the exception information) from the generator
			generator.AdjustBranchSizesToBestFit();
			methodBody.OperationExceptionInformation = new List<IOperationExceptionInformation>(generator.GetOperationExceptionInformation());
			methodBody.Operations = new List<IOperation>(generator.GetOperations());
			#endregion Retrieve the operations (and the exception information) from the generator
			methodBody.MaxStack++;

		}
	}
}
