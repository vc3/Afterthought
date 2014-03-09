namespace Afterthought.UnitTest.Target
{
    [GenericAmendment(typeof (GenericTestAmendment))]
    public class GenericCalculator<T>: IHaveExecutableMethod
    {
        public bool MethodExecuted { get; set; }

        public T Value;

        public T GetResult()
        {
            return default(T);
        }
    }
}