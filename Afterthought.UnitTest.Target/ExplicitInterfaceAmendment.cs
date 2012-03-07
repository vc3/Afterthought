namespace Afterthought.UnitTest.Target
{
	public class ExplicitInterfaceAmendment<T> : Amendment<T,T>
	{
        public ExplicitInterfaceAmendment()
		{
            Implement<IExplicitInterface>();
		}
	}
}