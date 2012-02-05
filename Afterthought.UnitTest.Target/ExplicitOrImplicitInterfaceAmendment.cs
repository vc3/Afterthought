namespace Afterthought.UnitTest.Target
{
	public class ExplicitOrImplicitInterfaceAmendment<T> : Amendment<T,T>
	{
        public ExplicitOrImplicitInterfaceAmendment()
		{
			Implement<IImplicitInterface>(explicitImplementation : false);
            Implement<IExplicitInterface>();
		}
	}
}