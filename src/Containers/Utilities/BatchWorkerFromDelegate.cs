using System;
using System.Threading.Tasks;

namespace TestContainers.Containers.Utilities
{
    public class BatchWorkerFromDelegate : BatchWorker
    {
        private readonly Func<Task> _work;

        public BatchWorkerFromDelegate(Func<Task> work)
        {
            _work = work;
        }

        protected override Task Work()
        {
            return _work();
        }
    }
}