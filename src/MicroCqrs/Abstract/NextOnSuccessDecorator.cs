using System;
using MicroCqrs.Attributes;
using MicroCqrs.Interfaces;

namespace MicroCqrs.Abstract
{
    [CqrsIgnore]
    public abstract class NextOnSuccessDecorator<TCmd> : ICmdHandler<TCmd>
    {
        private ICmdHandler<TCmd> Next { get; }
        protected abstract ICmdResult CmdResult { get; }

        protected NextOnSuccessDecorator(ICmdHandler<TCmd> next)
            => Next = next;
        
        // ReSharper disable once UnusedParameter.Global
        protected abstract void ExecuteBody(TCmd cmd);

        private ICmdResult TryCatchNext(TCmd cmd)
        {
            var current = CmdResult;
            
            try
            {
                ExecuteBody(cmd);
            }
            catch (Exception ex)
            {
                current.AddError(ex.Message);
            }
            
            if (current.IsSuccessful())
            {
                if (Next == null) // For Unit tests to not require the chain.
                    return current;
                
                var nextResult = Next.Execute(cmd);
                return nextResult;
            }

            return current;
        }
        
        public ICmdResult Execute(TCmd cmd)
            => TryCatchNext(cmd);
    }
}