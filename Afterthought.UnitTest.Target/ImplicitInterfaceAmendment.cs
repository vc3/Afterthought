namespace Afterthought.UnitTest.Target
{
	public class ImplicitInterfaceAmendment<T> : Amendment<T,T>
	{
        public ImplicitInterfaceAmendment()
		{
			Implement<IImplicitInterface>(explicitImplementation : false);
		}
	}
}