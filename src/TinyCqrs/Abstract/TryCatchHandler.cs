using System;
using TinyCqrs.Attributes;
using TinyCqrs.Classes;
using TinyCqrs.Interfaces;

namespace TinyCqrs.Abstract
{
    [CqrsIgnore]
    public abstract class TryCatchHandler<TCmd> : ICmdHandler<TCmd>
    {
        protected CmdResult CmdResult { get; set; }
        protected abstract void ExecuteBody(TCmd cmd);

        private CmdResult TryCatchNext(TCmd cmd)
        {
            var current = CmdResult ?? new CmdResult();
            
            try
            {
                ExecuteBody(cmd);
            }
            catch (Exception ex)
            {
                current.AddIssue(ex.Message);
            }

            return current;
        }
        
        public CmdResult Execute(TCmd cmd)
            => TryCatchNext(cmd);
    }
    
    [CqrsIgnore]
    public abstract class TryCatchHandler<TCmd, TResult> : ICmdHandler<TCmd, TResult>
        where TResult : ICmdResult, new()
    {
        protected TResult CmdResult { get; set; }
        protected abstract void ExecuteBody(TCmd cmd);

        private TResult TryCatchNext(TCmd cmd)
        {
            var current = CmdResult ?? new TResult();
            
            try
            {
                ExecuteBody(cmd);
            }
            catch (Exception ex)
            {
                current.AddIssue(ex.Message);
            }

            return current;
        }
        
        public TResult Execute(TCmd cmd)
            => TryCatchNext(cmd);
    }
}