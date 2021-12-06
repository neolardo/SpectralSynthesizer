using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpectralSynthesizer
{
    /// <summary>
    /// A task that can be easily cancelled.
    /// </summary>
    public class CancellableTask
    {
        #region Properties

        /// <summary>
        /// The cancellation token source.
        /// </summary>
        private CancellationTokenSource Source { get; set; }

        /// <summary>
        /// The task that should be completeds.
        /// </summary>
        private Task Task { get; set; }

        /// <summary>
        /// Indicates whether the task is completed or not.
        /// </summary>
        public bool IsCompleted => (Task == null) ? false : Task.IsCompleted;

        /// <summary>
        /// The cancellation token.
        /// </summary>
        public CancellationToken Token { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the task.
        /// </summary>
        public void Start(Action action)
        {
            Task = Task.Run(action, Token);
        }

        /// <summary>
        /// Waits for the task to finish or to be cancelled.
        /// </summary>
        public void Wait()
        {
            try
            {
                Task.Wait(Token);
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// Cancels the task.
        /// </summary>
        public void Cancel()
        {
            if (Task.IsCompleted == false)
            {
                Source.Cancel();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableTask"/> class.
        /// </summary>
        /// <param name="action">The action to be completed.</param>
        public CancellableTask()
        {
            Source = new CancellationTokenSource();
            Token = Source.Token;
        }

        #endregion
    }
}
