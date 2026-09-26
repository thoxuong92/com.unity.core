using System;

namespace Unity.Core.FSM
{
    public interface ITransition
    {
        IState From { get; }
        IState To { get; }
        Func<bool> Condition { get; }
        bool Evaluate();
    }

    /// <summary>
    /// Represents a conditional transition between two specific states.
    /// </summary>
    public class Transition : ITransition
    {
        public IState From { get; }
        public IState To { get; }
        public Func<bool> Condition { get; }

        public Transition(IState from, IState to, Func<bool> condition)
        {
            From = from;
            To = to;
            Condition = condition ?? (() => true);
        }

        public bool Evaluate()
        {
            return Condition != null && Condition.Invoke();
        }
    }
}
