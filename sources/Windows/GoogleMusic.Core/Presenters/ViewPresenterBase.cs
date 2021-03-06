﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Views;

    /// <summary>
    /// The ViewPresenterBase interface.
    /// </summary>
    public interface IViewPresenterBase
    {
        void Initialize(IView view);
    }

    /// <summary>
    /// The view presenter base.
    /// </summary>
    /// <typeparam name="TView">
    /// The type of view.
    /// </typeparam>
    public class ViewPresenterBase<TView> : BindingModelBase, IViewPresenterBase
        where TView : IView
    {
        public IDependencyResolverContainer Container { get; private set; }

        /// <summary>
        /// Gets the view.
        /// </summary>
        public TView View { get; private set; }

        /// <summary>
        /// Gets the main frame.
        /// </summary>
        protected IMainFrame MainFrame { get; private set; }

        /// <summary>
        /// Gets event aggregator..
        /// </summary>
        protected IEventAggregator EventAggregator { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the dispatcher.
        /// </summary>
        protected IDispatcher Dispatcher { get; private set; }

        /// <inheritdoc />
        void IViewPresenterBase.Initialize(IView view)
        {
            this.MainFrame = this.Container.Resolve<IMainFrame>();
            this.View = (TView)view;
            this.OnInitialized();
        }

        [Inject]
        internal void InjectMethod(
            IDependencyResolverContainer container,
            IEventAggregator eventAggregator,
            ILogManager logManager, 
            IDispatcher dispatcher)
        {
            this.Logger = logManager.CreateLogger(this.GetType().Name);
            this.Dispatcher = dispatcher;
            this.Container = container;
            this.EventAggregator = eventAggregator;
        }

        /// <summary>
        /// The on initialized.
        /// </summary>
        protected virtual void OnInitialized()
        {
        }
    }
}
