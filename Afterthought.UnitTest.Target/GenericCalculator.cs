namespace Afterthought.UnitTest.Target
{
    [GenericAmendment(typeof (GenericTestAmendment))]
    public class GenericCalculator<T>: IHaveExecutableMethod
    {
        public bool MethodExecuted { get; set; }

        public T Value;

        public T ValueProperty
        {
            get { return Value; }
            set { Value = value; }
        }

        public T GetResult()
        {
            return default(T);
        }

        public void SetResult(T result)
        {
            Value = result;
        }
    }
}